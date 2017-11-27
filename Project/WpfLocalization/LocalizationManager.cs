using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace WpfLocalization
{
    /// <summary>
    /// Manages all localized values.
    /// </summary>
    public static partial class LocalizationManager
    {
        #region Culture properties

        /// <summary>
        /// Provides binding support for the static *Culture properties of this type.
        /// </summary>
        /// <exception cref="InvalidOperationException">The property is not accessed from a UI thread.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2211:NonConstantFieldsShouldNotBeVisible")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static LocalizationManagerCulture Cultures = new LocalizationManagerCulture();

        /// <summary>
        /// Gets or sets <see cref="Thread.CurrentCulture"/> of the current UI thread.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <see cref="Thread.CurrentCulture"/> is used to format dates and numbers.
        /// </para>
        /// <para>
        /// CAUTION This method will set the culture only on the current UI thread. To set the culture of all UI threads (in case the application uses more than one)
        /// use <see cref="GlobalCulture"/>.
        /// </para>
        /// <para>
        /// To bind to this property use the <see cref="Cultures"/> instance.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">The current thread is not a UI thread.</exception>
        public static CultureInfo CurrentCulture
        {
            get
            {
                WpfUtils.VerifyIsUIThread();

                return Thread.CurrentThread.CurrentCulture;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                PrivateSetCurrentCultures(value, null);
            }
        }

        /// <summary>
        /// Gets or sets <see cref="Thread.CurrentUICulture"/> of the current UI thread.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <see cref="Thread.CurrentUICulture"/> specifies the culture of the resources (e.g. text and images).
        /// </para>
        /// <para>
        /// CAUTION This method will set the culture only on the current UI thread. To set the culture of all UI threads (in case the application uses more than one)
        /// use <see cref="GlobalUICulture"/>.
        /// </para>
        /// <para>
        /// To bind to this property use the <see cref="Cultures"/> instance.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">The current thread is not a UI thread.</exception>
        public static CultureInfo CurrentUICulture
        {
            get
            {
                WpfUtils.VerifyIsUIThread();

                return Thread.CurrentThread.CurrentUICulture;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                PrivateSetCurrentCultures(null, value);
            }
        }

        static CultureInfo _globalCulture;

        /// <summary>
        /// Gets or sets <see cref="Thread.CurrentCulture"/> of *all* UI threads that at least one localized property.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This property is most useful for applications that utilize multiple UI threads. For applications that have only a single UI thread
        /// the <see cref="CurrentCulture"/> provides a bit better performance. However, this method can be called from any thread and not just a UI thread
        /// which can be useful for single-threaded applications as well.
        /// </para>
        /// <para>
        /// The initial value of this property is the culture of the primary UI thread (<see cref="Application.Current"/>).
        /// </para>
        /// <para>
        /// While the setter sets the culture of *all* UI threads the getter returns either the culture of the primary UI thread (the initial value)
        /// or the last value set . There is no guarantee that the <see cref="CultureInfo"/> by the getter is the actual culture of every
        /// UI thread as each UI thread has its own culture setting.
        /// </para>
        /// <para>
        /// The setter sets the culture of all UI threads that controls at least one localized value. Threads that do not use localization
        /// are unknown to <see cref="LocalizationManager"/>.
        /// </para>
        /// <para>
        /// The value of this property is used by the <see cref="SpawnUIThread"/> method to initialize the culture of the newly spawned UI thread.
        /// </para>
        /// <para>
        /// <see cref="Thread.CurrentCulture"/> is used to format dates and numbers.
        /// </para>
        /// <para>
        /// To bind to this property use the <see cref="Cultures"/> instance.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">The getter is accessed in a non-WPF application.</exception>
        public static CultureInfo GlobalCulture
        {
            get
            {
                if (_globalCulture == null)
                {
                    var application = Application.Current;
                    if (application == null)
                    {
                        throw new InvalidOperationException("This property can be accessed only in a WPF application.");
                    }
                    _globalCulture = application.Dispatcher.Thread.CurrentCulture;
                }
                return _globalCulture;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                PrivateSetGlobalCultures(value, null);
            }
        }

        static CultureInfo _globalUICulture;

        /// <summary>
        /// Gets or sets <see cref="Thread.CurrentUICulture"/> of *all* UI threads that at least one localized property.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This property is most useful for applications that utilize multiple UI threads. For applications that have only a single UI thread
        /// the <see cref="CurrentUICulture"/> provides a bit better performance. However, this method can be called from any thread and not just a UI thread
        /// which can be useful for single-threaded applications as well.
        /// </para>
        /// <para>
        /// The initial value of this property is the culture of the primary UI thread (<see cref="Application.Current"/>).
        /// </para>
        /// <para>
        /// While the setter sets the culture of *all* UI threads the getter returns either the culture of the primary UI thread (the initial value)
        /// or the last value set . There is no guarantee that the <see cref="CultureInfo"/> by the getter is the actual culture of every
        /// UI thread as each UI thread has its own culture setting.
        /// </para>
        /// <para>
        /// The setter sets the culture of all UI threads that controls at least one localized value. Threads that do not use localization
        /// are unknown to <see cref="LocalizationManager"/>.
        /// </para>
        /// <para>
        /// The value of this property is used by the <see cref="SpawnUIThread"/> method to initialize the culture of the newly spawned UI thread.
        /// </para>
        /// <para>
        /// <see cref="Thread.CurrentUICulture"/> specifies the culture of the resources (e.g. text and images).
        /// </para>
        /// <para>
        /// To bind to this property use the <see cref="Cultures"/> instance.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">The getter is accessed in a non-WPF application.</exception>
        public static CultureInfo GlobalUICulture
        {
            get
            {
                if (_globalUICulture == null)
                {
                    var application = Application.Current;
                    if (application == null)
                    {
                        throw new InvalidOperationException("This property can be accessed only in a WPF application.");
                    }
                    _globalUICulture = application.Dispatcher.Thread.CurrentUICulture;
                }
                return _globalUICulture;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                PrivateSetGlobalCultures(null, value);
            }
        }

        #region Private methods

        static void PrivateSetCurrentCultures(CultureInfo culture, CultureInfo uiCulture)
        {
            var thread = Thread.CurrentThread;
            var dispatcher = Dispatcher.FromThread(thread);
            if (dispatcher == null)
            {
                throw new InvalidOperationException("This method can be called only from a UI thread.");
            }

            // This method should work even if there are no localized values controlled by the current UI thread
            var cultureChanged = false;
            if (culture != null && thread.CurrentCulture != culture)
            {
                thread.CurrentCulture = culture;
                cultureChanged = true;
            }
            var uiCultureChanged = false;
            if (uiCulture != null && thread.CurrentUICulture != uiCulture)
            {
                thread.CurrentUICulture = uiCulture;
                uiCultureChanged = true;
            }
            // This method should work even if there are no localized values controlled by the current UI thread
            GetDispatcherHandler(dispatcher)?.RefreshValues();

            if (cultureChanged)
            {
                FireCultureChangedAsync(dispatcher, nameof(CurrentCulture), () => _currentCultureChanged);
            }
            if (uiCultureChanged)
            {
                FireCultureChangedAsync(dispatcher, nameof(CurrentUICulture), () => _currentUICultureChanged);
            }
        }

        static void PrivateSetGlobalCultures(CultureInfo culture, CultureInfo uiCulture)
        {
            lock (_dispatcherHandlerList)
            {
                // The fields should be set inside the lock to ensure that the field's value correspond to the culture actually applied
                // to the UI threads. Setting the fields outside of the lock makes the following scenario possible:
                // 1. Thread1 sets culture = "fr-FR" (value stored in the field)
                // 2. Thread2 sets culture = "de-DE" (value stored in the field)
                // 3. Thread2 obtains a lock and sets the culture of the individual threads
                // 4. Thread1 obtains a lock and sets the culture of the individual threads
                // As a result the culture stored in the field will be different than the actual culture of the threads.
                var globalCultureChanged = false;
                if (culture != null && _globalCulture != culture)
                {
                    _globalCulture = culture;
                    globalCultureChanged = true;
                }
                var globalUICultureChanged = false;
                if (uiCulture != null && _globalUICulture != uiCulture)
                {
                    _globalUICulture = uiCulture;
                    globalUICultureChanged = true;
                }

                foreach (var item in _dispatcherHandlerList)
                {
                    var thread = item.Dispatcher.Thread;
                    var cultureChanged = false;
                    if (culture != null && thread.CurrentCulture != culture)
                    {
                        thread.CurrentCulture = culture;
                        cultureChanged = true;
                    }
                    var uiCultureChanged = false;
                    if (uiCulture != null && thread.CurrentUICulture != uiCulture)
                    {
                        thread.CurrentUICulture = uiCulture;
                        uiCultureChanged = true;
                    }
                    if (cultureChanged || uiCultureChanged)
                    {
                        item.RefreshValues();
                    }
                    if (cultureChanged)
                    {
                        FireCultureChangedAsync(item.Dispatcher, nameof(CurrentCulture), () => _currentCultureChanged);
                    }
                    if (uiCultureChanged)
                    {
                        FireCultureChangedAsync(item.Dispatcher, nameof(CurrentUICulture), () => _currentUICultureChanged);
                    }
                    if (globalCultureChanged)
                    {
                        FireCultureChangedAsync(item.Dispatcher, nameof(GlobalCulture), () => _globalCultureChanged);
                    }
                    if (globalUICultureChanged)
                    {
                        FireCultureChangedAsync(item.Dispatcher, nameof(GlobalUICulture), () => _globalUICultureChanged);
                    }
                }
            }
        }

        #endregion

        #endregion

        #region Culture events

        [ThreadStatic]
        static EventHandler _currentCultureChanged;

        /// <summary>
        /// Occurs when the <see cref="CurrentCulture"/> of the current UI thread changes.
        /// </summary>
        /// <remarks>
        /// CAUTION This even is fired only on a UI thread that controls at least one localized value.
        /// </remarks>
        /// <exception cref="InvalidOperationException">The event is accessed from a non-UI thread.</exception>
        public static event EventHandler CurrentCultureChanged
        {
            add
            {
                WpfUtils.VerifyIsUIThread();

                _currentCultureChanged += value;
            }
            remove
            {
                WpfUtils.VerifyIsUIThread();

                _currentCultureChanged -= value;
            }
        }

        [ThreadStatic]
        static EventHandler _currentUICultureChanged;

        /// <summary>
        /// Occurs when the <see cref="CurrentUICulture"/> of the current UI thread changes.
        /// </summary>
        /// <remarks>
        /// CAUTION This even is fired only on a UI thread that controls at least one localized value.
        /// </remarks>
        /// <exception cref="InvalidOperationException">The event is accessed from a non-UI thread.</exception>
        public static event EventHandler CurrentUICultureChanged
        {
            add
            {
                WpfUtils.VerifyIsUIThread();

                _currentUICultureChanged += value;
            }
            remove
            {
                WpfUtils.VerifyIsUIThread();

                _currentUICultureChanged -= value;
            }
        }

        [ThreadStatic]
        static EventHandler _globalCultureChanged;

        /// <summary>
        /// Occurs when the <see cref="GlobalCulture"/> of the current UI thread changes.
        /// </summary>
        /// <remarks>
        /// CAUTION This even is fired only on a UI thread that controls at least one localized value.
        /// </remarks>
        /// <exception cref="InvalidOperationException">The event is accessed from a non-UI thread.</exception>
        public static event EventHandler GlobalCultureChanged
        {
            add
            {
                WpfUtils.VerifyIsUIThread();

                _globalCultureChanged += value;
            }
            remove
            {
                WpfUtils.VerifyIsUIThread();

                _globalCultureChanged -= value;
            }
        }

        [ThreadStatic]
        static EventHandler _globalUICultureChanged;

        /// <summary>
        /// Occurs when the <see cref="GlobalUICulture"/> of the current UI thread changes.
        /// </summary>
        /// <remarks>
        /// CAUTION This even is fired only on a UI thread that controls at least one localized value.
        /// </remarks>
        /// <exception cref="InvalidOperationException">The event is accessed from a non-UI thread.</exception>
        public static event EventHandler GlobalUICultureChanged
        {
            add
            {
                WpfUtils.VerifyIsUIThread();

                _globalUICultureChanged += value;
            }
            remove
            {
                WpfUtils.VerifyIsUIThread();

                _globalUICultureChanged -= value;
            }
        }

        #endregion

        #region Culture properties & events

        /// <summary>
        /// Called when a culture property changes.
        /// </summary>
        /// <param name="dispatcher">The dispatcher to raised the event on.</param>
        /// <param name="propertyName">The name of the changed property.</param>
        /// <param name="eventHandler">A function that returns the event handler.</param>
        /// <remarks>
        /// <paramref name="eventHandler"/> is a function as the actual value must be obtained on the UI thread since the field is [ThreadStatic].
        /// </remarks>
        static void FireCultureChangedAsync(Dispatcher dispatcher, string propertyName, Func<EventHandler> eventHandler)
        {
            Debug.Assert(dispatcher != null);

            dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                // Fire the binding event
                Cultures.FirePropertyChanged(propertyName);

                // Fire the event
                eventHandler()?.Invoke(null, EventArgs.Empty);
            }));
        }

        #endregion

        #region Multiple threads

        /// <summary>
        /// Starts a new UI thread and sets <see cref="Thread.CurrentCulture"/> and <see cref="Thread.CurrentUICulture"/>
        /// to <see cref="GlobalCulture"/> and <see cref="GlobalUICulture"/> correspondingly.
        /// </summary>
        /// <param name="start">The main method of the thread.</param>
        /// <param name="threadName">The name of the thread.</param>
        /// <returns>The newly created thread.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        public static Thread SpawnUIThread(ThreadStart start, string threadName = null)
        {
            var thread = new Thread(start)
            {
                CurrentCulture = GlobalCulture,
                CurrentUICulture = GlobalUICulture,
                Name = threadName,
            };
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            return thread;
        }

        /// <summary>
        /// Starts a new UI thread and sets <see cref="Thread.CurrentCulture"/> and <see cref="Thread.CurrentUICulture"/>
        /// to <see cref="GlobalCulture"/> and <see cref="GlobalUICulture"/> correspondingly.
        /// </summary>
        /// <param name="start">The main method of the thread.</param>
        /// <param name="threadName">The name of the thread.</param>
        /// <returns>The newly created thread.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        public static Thread SpawnUIThread(ParameterizedThreadStart start, object state, string threadName = null)
        {
            var thread = new Thread(start)
            {
                CurrentCulture = GlobalCulture,
                CurrentUICulture = GlobalUICulture,
                Name = threadName,
            };
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start(state);
            return thread;
        }

        #endregion

        #region Values

        /// <summary>
        /// Adds a localized value to the manager.
        /// </summary>
        /// <param name="value"></param>
        /// <exception cref="InvalidOperationException">The method is not called on the UI thread of <see cref="LocalizedValueBase.TargetObject"/>.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "DependencyObject")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object)")]
        internal static void Add(LocalizedValue value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            var dispatcher = value.Dispatcher;

            if (dispatcher == null)
            {
                // The dependency object has been GC
                return;
            }

            if (false == dispatcher.CheckAccess())
            {
                // COMMENT This restriction can be lifted if the DispatcherHandler.Add method becomes thread-safe
                throw new InvalidOperationException($"This method must be called on the UI thread of the {nameof(DependencyObject)} whose value is localized.");
            }

            GetOrCreateDispatcherHander(dispatcher).Add(value);
        }

        /// <summary>
        /// Adds a localized value to the manager.
        /// </summary>
        /// <param name="value"></param>
        /// <exception cref="InvalidOperationException">The method is not called on the UI thread of <see cref="LocalizedValueBase.TargetObject"/>.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "DependencyObject")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object)")]
        internal static void Add(SetterLocalizedValue value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            var dispatcher = value.Dispatcher;

            if (dispatcher == null)
            {
                // The dependency object has been GC
                return;
            }

            if (false == dispatcher.CheckAccess())
            {
                // COMMENT This restriction can be lifted if the DispatcherHandler.Add method becomes thread-safe
                throw new InvalidOperationException($"This method must be called on the UI thread of the {nameof(DependencyObject)} whose value is localized.");
            }

            GetOrCreateDispatcherHander(dispatcher).Add(value);
        }

        /// <summary>
        /// Stops localizing the specified property of the specified <see cref="DependencyObject"/>.
        /// </summary>
        /// <param name="targetObject">The owner of the property</param>
        /// <param name="property"></param>
        /// <exception cref="InvalidOperationException">The method is not called on the UI thread of the specified <see cref="DependencyObject"/>.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "DependencyObject")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object)")]
        internal static void RemoveProperty(DependencyObject targetObject, LocalizedProperty targetProperty)
        {
            if (targetObject == null)
            {
                throw new ArgumentNullException(nameof(targetObject));
            }
            if (targetProperty == null)
            {
                throw new ArgumentNullException(nameof(targetProperty));
            }

            if (false == targetObject.Dispatcher.CheckAccess())
            {
                // COMMENT This restriction can be lifted if the DispatcherHandler.Add method becomes thread-safe
                throw new InvalidOperationException($"This method must be called on the UI thread of the {nameof(DependencyObject)} whose value is localized.");
            }

            GetDispatcherHandler(targetObject.Dispatcher)?.RemoveProperty(targetObject, targetProperty);
        }

        /// <summary>
        /// Updates the localized values on all UI threads.
        /// </summary>
        /// <remarks>
        /// This method should be called if the culture of a UI thread is set by using the <see cref="Thread.CurrentCulture"/> or the
        /// <see cref="Thread.CurrentUICulture"/> properties instead of the culture properties exposed by this type.
        /// </remarks>
        /// <seealso cref="CurrentCulture"/>
        /// <seealso cref="CurrentUICulture"/>
        /// <seealso cref="GlobalCulture"/>
        /// <seealso cref="GlobalUICulture"/>
        public static void RefreshValues()
        {
            lock (_dispatcherHandlerList)
            {
                foreach (var item in _dispatcherHandlerList)
                {
                    item.RefreshValues();
                }
            }
        }

        /// <summary>
        /// Updates the localized values on the specified UI thread.
        /// </summary>
        /// <param name="dispatcher"></param>
        internal static void RefreshValues(Dispatcher dispatcher)
        {
            Debug.Assert(dispatcher != null);

            GetDispatcherHandler(dispatcher)?.RefreshValues();
        }

        /// <summary>
        /// Removes localized values of controls that has been garbage collected.
        /// </summary>
        /// <remarks>
        /// Localized values are purged automatically so normally there is no need to call this method. However, in extreme cases when thousands of
        /// controls are localized are subsequently disposed of rapidly (e.g. within minute and two), for example, when used in a data grid, this
        /// method can be used to manually purge the localized values of garbage collected controls. For maximum effect is recommended to
        /// force a garbage collection prior to calling this method.
        /// </remarks>
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public static void PurgeValues()
        {
            lock (_dispatcherHandlerList)
            {
                foreach (var item in _dispatcherHandlerList)
                {
                    item.PurgeValues();
                }
            }
        }

        #endregion

        #region DispatcherHandler

        /// <summary>
        /// Contains the handler of the dispatcher of the last added localized value.
        /// </summary>
        static DispatcherHandler _lastDispatcherHandler;

        static List<DispatcherHandler> _dispatcherHandlerList = new List<DispatcherHandler>();

        /// <summary>
        /// Returns the <see cref="DispatcherHandler"/> for the specified <see cref="Dispatcher"/> or <c>null</c>
        /// if no <see cref="DispatcherHandler"/> exists for the specified <see cref="Dispatcher"/>.
        /// </summary>
        /// <param name="dispatcher"></param>
        /// <returns></returns>
        static DispatcherHandler GetDispatcherHandler(Dispatcher dispatcher)
        {
            Debug.Assert(dispatcher != null);

            var lastDispatcherHandler = _lastDispatcherHandler;

            if (lastDispatcherHandler != null && dispatcher == lastDispatcherHandler.Dispatcher)
            {
                return lastDispatcherHandler;
            }
            else
            {
                lock (_dispatcherHandlerList)
                {
                    return _dispatcherHandlerList.Find(x => x.Dispatcher == dispatcher);
                }
            }
        }

        /// <summary>
        /// Returns a <see cref="DispatcherHandler"/> for the specified <see cref="Dispatcher"/>.
        /// If a <see cref="DispatcherHandler"/> does not exists yet then one is created.
        /// </summary>
        /// <param name="dispatcher"></param>
        /// <returns></returns>
        static DispatcherHandler GetOrCreateDispatcherHander(Dispatcher dispatcher)
        {
            Debug.Assert(dispatcher != null);

            var lastDispatcherHandler = _lastDispatcherHandler;

            if (lastDispatcherHandler != null && dispatcher == lastDispatcherHandler.Dispatcher)
            {
                return lastDispatcherHandler;
            }
            else
            {
                lock (_dispatcherHandlerList)
                {
                    var dispatcherHandler = _dispatcherHandlerList.Find(x => x.Dispatcher == dispatcher);
                    if (dispatcherHandler == null)
                    {
                        dispatcherHandler = new DispatcherHandler(dispatcher);
                        _dispatcherHandlerList.Add(dispatcherHandler);
                        dispatcher.ShutdownFinished += Dispatcher_ShutdownFinished;
                    }
                    _lastDispatcherHandler = dispatcherHandler;

                    return dispatcherHandler;
                }
            }
        }

        static void Dispatcher_ShutdownFinished(object sender, EventArgs e)
        {
            lock (_dispatcherHandlerList)
            {
                _dispatcherHandlerList.RemoveAll(x => x.Dispatcher == sender);
                if (_lastDispatcherHandler != null && _lastDispatcherHandler.Dispatcher == sender)
                {
                    _lastDispatcherHandler = null;
                }
            }
        }

        #endregion
    }
}
