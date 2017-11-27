﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace WpfLocalization
{
    partial class LocalizationManager
    {
        /// <summary>
        /// Manages the localized value for a particular dispatcher.
        /// </summary>
        class DispatcherHandler
        {
            /// <summary>
            /// The dispatcher handled by this instance.
            /// </summary>
            public Dispatcher Dispatcher { get; }

            /// <summary>
            /// The list of localized values.
            /// </summary>
            LinkedList<LocalizedValue> _localizedValueList = new LinkedList<LocalizedValue>();

            /// <summary>
            /// The list of localized values.
            /// </summary>
            LinkedList<SetterLocalizedValue> _setterLocalizedValueList = new LinkedList<SetterLocalizedValue>();

            /// <summary>
            /// Dictionary of localized values.
            /// </summary>
            ConditionalWeakTable<DependencyObject, object> _localizedValueDict = new ConditionalWeakTable<DependencyObject, object>();

            int _lastPurgeValueCount = 0;
            int _currentPurgeValueCount = 0;
            int _purgeInProgress = 0;

            public DispatcherHandler(Dispatcher dispatcher)
            {
                this.Dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));

                // Purge values at least once every 5m
                var timer = new DispatcherTimer(TimeSpan.FromMinutes(5), DispatcherPriority.Background, PurgeValues, dispatcher);
                timer.Start();
            }

            #region Add & Remove

            /// <summary>
            /// Adds a localized value to the manager.
            /// </summary>
            /// <param name="value"></param>
            /// <exception cref="InvalidOperationException">The method is called on a thread different from the <see cref="Dispatcher"/>'s thread.</exception>
            public void Add(LocalizedValue value)
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                // This verification is performed by the "RemoveProperty" method
                //Dispatcher.VerifyAccess();

                RemoveProperty(value.TargetObject, value.TargetProperty);

                var valueNode = _localizedValueList.AddLast(value);

                if (_localizedValueDict.TryGetValue(value.TargetObject, out object oldValue))
                {
                    if (oldValue is LinkedListNode<LocalizedValue> oldValueNode)
                    {
                        // A second value of the same DependencyObject has been localized so create a list of values
                        var valueNodeList = new List<LinkedListNode<LocalizedValue>>()
                        {
                            oldValueNode,
                            valueNode
                        };

                        // Replace the old value with the list
                        _localizedValueDict.Remove(value.TargetObject);
                        _localizedValueDict.Add(value.TargetObject, valueNodeList);
                    }
                    else if (oldValue is List<LinkedListNode<LocalizedValue>> oldValueNodeList)
                    {
                        oldValueNodeList.Add(valueNode);
                    }
                    else
                    {
                        // This should be impossible
                        throw new NotImplementedException("Unhandled situation.");
                    }
                }
                else
                {
                    // Add the value for the first time
                    _localizedValueDict.Add(value.TargetObject, valueNode);
                }

                _currentPurgeValueCount++;
                // Purge values if more than 1000 values has been added since the last purge
                if (_currentPurgeValueCount - _lastPurgeValueCount > 1000)
                {
                    _lastPurgeValueCount = _currentPurgeValueCount;

                    // Trigger a purge
                    PurgeValues();
                }
            }

            /// <summary>
            /// Adds a localized value to the manager.
            /// </summary>
            /// <param name="value"></param>
            /// <exception cref="InvalidOperationException">The method is called on a thread different from the <see cref="Dispatcher"/>'s thread.</exception>
            public void Add(SetterLocalizedValue value)
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                Dispatcher.VerifyAccess();

                _setterLocalizedValueList.AddLast(value);
            }

            /// <summary>
            /// Stops localizing the specified property of the specified <see cref="DependencyObject"/>.
            /// </summary>
            /// <param name="targetObject">The owner of the property</param>
            /// <param name="property"></param>
            /// <exception cref="InvalidOperationException">The method is not called on the UI thread of the specified <see cref="DependencyObject"/>.</exception>
            public void RemoveProperty(DependencyObject targetObject, LocalizedProperty targetProperty)
            {
                if (targetObject == null)
                {
                    throw new ArgumentNullException(nameof(targetObject));
                }
                if (targetProperty == null)
                {
                    throw new ArgumentNullException(nameof(targetProperty));
                }

                Dispatcher.VerifyAccess();

                if (_localizedValueDict.TryGetValue(targetObject, out object oldValue))
                {
                    if (oldValue is LinkedListNode<LocalizedValue> oldValueNode)
                    {
                        if (oldValueNode.Value.TargetProperty.Equals(targetProperty))
                        {
                            // Remove the previous value from both the dictionary and the list
                            _localizedValueDict.Remove(targetObject);
                            _localizedValueList.Remove(oldValueNode);
                        }
                    }
                    else if (oldValue is List<LinkedListNode<LocalizedValue>> oldValueNodeList)
                    {
                        // Remove the previous localized value
                        var oldValueNodeIndex = oldValueNodeList.FindIndex(x => x.Value.TargetProperty.Equals(targetProperty));
                        if (oldValueNodeIndex >= 0)
                        {
                            // Remove the previous value from both the dictionary and the list
                            oldValueNode = oldValueNodeList[oldValueNodeIndex];
                            oldValueNodeList.RemoveAt(oldValueNodeIndex);
                            _localizedValueList.Remove(oldValueNode);
                        }
                    }
                }
            }

            #endregion

            #region RefreshValues

            /// <summary>
            /// Indicates if a refresh has been scheduled but not started yet.
            /// </summary>
            int _refreshScheduled;

            /// <summary>
            /// The last scheduled call to the <see cref="RefreshValues"/> method.
            /// </summary>
            DispatcherOperation _refreshOperation;

            /// <summary>
            /// Updates all localized values.
            /// </summary>
            /// <remarks>
            /// This method is thread-safe.
            /// </remarks>
            public void RefreshValues()
            {
                if (Interlocked.CompareExchange(ref _refreshScheduled, 1, 0) != 0)
                {
                    // A refresh is already being scheduled
                    return;
                }

                Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(StartRefreshValues));
            }

            /// <summary>
            /// Starts a new refresh operation.
            /// </summary>
            /// <remarks>
            /// CAUTION This method must be called on the <see cref="Dispatcher"/>'s thread.
            /// </remarks>
            void StartRefreshValues()
            {
                if (_refreshOperation != null)
                {
                    // The previously scheduled refresh operation must be aborted since values must be refreshed from the start
                    _refreshOperation.Abort();
                    _refreshOperation = null;
                }

                if (_localizedValueList.First != null)
                {
                    // Schedule the first phase of the refresh operation
                    // Start with regular values; the "RefreshValues" will automatically continue with setter values
                    _refreshOperation = Dispatcher.BeginInvoke(DispatcherPriority.Background, new SendOrPostCallback(RefreshValues), _localizedValueList.First);
                }
                else if (_setterLocalizedValueList.First != null)
                {
                    // Schedule the first phase of the refresh operation
                    // There are no regular values so start with setter values
                    _refreshOperation = Dispatcher.BeginInvoke(DispatcherPriority.Background, new SendOrPostCallback(RefreshValues), _setterLocalizedValueList.First);
                }

                // Allow new refresh operations to be scheduled
                Interlocked.Exchange(ref _refreshScheduled, 0);
            }

            /// <summary>
            /// Updates all localized values.
            /// </summary>
            /// <remarks>
            /// CAUTION This method must be called on the <see cref="Dispatcher"/>'s thread.
            /// </remarks>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object)")]
            void RefreshValues(object state)
            {
                if (state is LinkedListNode<LocalizedValue> localizedValueNode)
                {
                    // In order to avoid blocking the UI thread for too long the refresh is split into phases each of which updates at most 1000 values
                    for (var k = 0; k < 1000 && localizedValueNode != null; k++, localizedValueNode = localizedValueNode.Next)
                    {
                        localizedValueNode.Value.UpdateValue();
                    }

                    if (localizedValueNode != null)
                    {
                        _refreshOperation = Dispatcher.BeginInvoke(DispatcherPriority.Background, new SendOrPostCallback(RefreshValues), localizedValueNode);
                    }
                    else if (_setterLocalizedValueList.First != null)
                    {
                        // Update setter values
                        _refreshOperation = Dispatcher.BeginInvoke(DispatcherPriority.Background, new SendOrPostCallback(RefreshValues), _setterLocalizedValueList.First);
                    }
                    else
                    {
                        _refreshOperation = null;
                    }
                }
                else if (state is LinkedListNode<SetterLocalizedValue> setterLocalizedValueNode)
                {
                    // In order to avoid blocking the UI thread for too long the refresh is split into phases each of which updates at most 1000 values
                    for (var k = 0; k < 1000 && setterLocalizedValueNode != null; k++, setterLocalizedValueNode = setterLocalizedValueNode.Next)
                    {
                        setterLocalizedValueNode.Value.UpdateValue();
                    }

                    if (setterLocalizedValueNode != null)
                    {
                        _refreshOperation = Dispatcher.BeginInvoke(DispatcherPriority.Background, new SendOrPostCallback(RefreshValues), setterLocalizedValueNode);
                    }
                    else
                    {
                        _refreshOperation = null;
                    }
                }
                else
                {
                    throw new NotImplementedException($"A localized value of type {state.GetType().FullName} is not handled.");
                }

#if DEPRECATED
                var node = (LinkedListNode<LocalizedValueBase>)state;

                // In order to avoid blocking the UI thread for too long the refresh is split into phases each of which updates at most 1000 values
                for (var k = 0; k < 1000 && node != null; k++, node = node.Next)
                {
                    node.Value.UpdateValue();
                }

                if (node != null)
                {
                    _refreshOperation = Dispatcher.BeginInvoke(DispatcherPriority.Background, new SendOrPostCallback(RefreshValues), node);
                }
                else if (state is LinkedListNode<LocalizedValue> && _setterLocalizedValueList.First != null)
                {
                    // Update setter values
                    _refreshOperation = Dispatcher.BeginInvoke(DispatcherPriority.Background, new SendOrPostCallback(RefreshValues), _setterLocalizedValueList.First);
                }
                else
                {
                    _refreshOperation = null;
                }
#endif
            }

            #endregion

            #region PurgeValues

            /// <summary>
            /// Removes localized values of <see cref="DependencyObject"/>s that has been GC.
            /// </summary>
            /// <remarks>
            /// This method is thread-safe.
            /// </remarks>
            public void PurgeValues()
            {
                if (Interlocked.CompareExchange(ref _purgeInProgress, 1, 0) != 0)
                {
                    // Values are already being purged
                    return;
                }

                Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(StartPurgeValues));
            }

            /// <summary>
            /// Removes localized values of <see cref="DependencyObject"/>s that has been GC.
            /// </summary>
            /// <remarks>
            /// This method is thread-safe.
            /// </remarks>
            void PurgeValues(object sender, EventArgs e)
            {
                if (Interlocked.CompareExchange(ref _purgeInProgress, 1, 0) != 0)
                {
                    // Values are already being purged
                    return;
                }

                Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(StartPurgeValues));
            }

            /// <summary>
            /// Starts purging localized values.
            /// </summary>
            /// <remarks>
            /// CAUTION This method must be called on the <see cref="Dispatcher"/>'s thread.
            /// </remarks>
            void StartPurgeValues()
            {
                if (_localizedValueList.First != null)
                {
                    PurgeValues(_localizedValueList.First);
                }
                else
                {
                    // The purge is complete
                    Interlocked.Exchange(ref _purgeInProgress, 0);
                }
            }

            /// <summary>
            /// Removes localized values of <see cref="DependencyObject"/>s that has been GC.
            /// </summary>
            /// <param name="state"></param>
            /// <remarks>
            /// CAUTION This method must be called on the <see cref="Dispatcher"/>'s thread.
            /// </remarks>
            void PurgeValues(object state)
            {
                var node = (LinkedListNode<LocalizedValue>)state;

                // In order to avoid blocking the UI thread for too long the purge is split into phases each of which inspects at most 1000 values
                for (var k = 0; k < 1000 && node != null; k++)
                {
                    var nextNode = node.Next;
                    if (false == node.Value.IsAlive)
                    {
                        node.List.Remove(node);
                    }
                    node = nextNode;
                }

                if (node != null)
                {
                    Dispatcher.BeginInvoke(DispatcherPriority.Background, new SendOrPostCallback(PurgeValues), node);
                }
                else
                {
                    // The purge is complete
                    Interlocked.Exchange(ref _purgeInProgress, 0);
                }
            }

            #endregion
        }

    }
}