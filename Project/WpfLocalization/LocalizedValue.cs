using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Threading;
using WpfLocalization.Converters;

namespace WpfLocalization
{
    /// <summary>
    /// Represents a localized value.
    /// </summary>
    class LocalizedValue : IServiceProvider, IProvideValueTarget
    {
        #region Constants

        /// <summary>
        /// The error message to display in place of a localized value when no resource manager was found.
        /// </summary>
        const string ErrorMessage_ResourceManagerNotFound = "[- Resource File Not Found -]";

        /// <summary>
        /// The error message to display in place of a localized value when the specified resource key was not found.
        /// </summary>
        const string ErrorMessage_ResourceKeyNotFound = "[{0}]";

        #endregion

        #region TargetObject

        /// <summary>
        /// The object whose property is localized.
        /// </summary>
        /// <remarks>
        /// CAUTION This value is <c>null</c> if the object has been GC.
        /// </remarks>
        public DependencyObject TargetObject => _targetObject.Target as DependencyObject;

        /// <summary>
        /// Gets a value indicating if the owner of the property still exists.
        /// </summary>
        public bool IsAlive => _targetObject.IsAlive;

        /// <summary>
        /// Gets a value indicating if the owner of the property is used in design mode.
        /// </summary>
        public bool IsInDesignMode
        {
            get
            {
                return _targetObject.Target is DependencyObject targetObject && DesignerProperties.GetIsInDesignMode(targetObject);
            }
        }

        /// <summary>
        /// The owner of the property.
        /// </summary>
        WeakReference _targetObject { get; }

        #endregion

        /// <summary>
        /// The localized property.
        /// </summary>
        public LocalizedProperty TargetProperty { get; }

        Tuple<DependencyObject, LocalizedProperty> _dictKey;

        /// <summary>
        /// A key used for dictionary lookups.
        /// </summary>
        internal Tuple<DependencyObject, LocalizedProperty> DictKey
        {
            get
            {
                return _dictKey ?? (_dictKey = new Tuple<DependencyObject, LocalizedProperty>(TargetObject, TargetProperty));
            }
        }

        /// <summary>
        /// The name of the resource.
        /// </summary>
        /// <remarks>
        /// </remarks>
        public string Key { get; set; }

        /// <summary>
        /// The format string to use in conjunction with <see cref="Binding"/> or <see cref="Bindings"/>.
        /// </summary>
        /// <remarks>
        /// </remarks>
        public string StringFormat { get; set; }

        #region Binding

        /// <summary>
        /// The binding to pass as argument to the format string.
        /// </summary>
        public BindingBase Binding { get; set; }

        /// <summary>
        /// The list of binding to pass as arguments to the format string.
        /// </summary>
        public Collection<BindingBase> Bindings { get; set; }

        /// <summary>
        /// The binding used to obtain the value.
        /// </summary>
        MultiBinding _finalBinding;
        BindingExpressionBase _finalBindingExpression;

        #endregion

        #region Converter

        /// <summary>
        /// The converter to use to convert the value before it is assigned to the property.
        /// </summary>
        /// <remarks>
        /// The converter is ignored on data-bound values.
        /// </remarks>
        public IValueConverter Converter { get; set; }

        /// <summary>
        /// The parameter to pass to the converter.
        /// </summary>
        /// <remarks>
        /// The converter is ignored on data-bound values.
        /// </remarks>
        public object ConverterParameter { get; set; }

        #endregion

        public LocalizedValue(DependencyObject targetObject, LocalizedProperty targetProperty)
        {
            if (targetObject == null)
            {
                throw new ArgumentNullException(nameof(targetObject));
            }
            this.TargetProperty = targetProperty ?? throw new ArgumentNullException(nameof(targetProperty));
            this._targetObject = new WeakReference(targetObject);
        }

        /// <summary>
        /// Updates the value of the property.
        /// </summary>
        public void UpdateValue()
        {
            var targetObject = this.TargetObject;
            if (targetObject == null)
            {
                // The object has been GC
                return;
            }

            if (targetObject.Dispatcher.CheckAccess())
            {
                GetOrUpdateValue(targetObject, true);
            }
            else
            {
                targetObject.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new SendOrPostCallback(x => GetOrUpdateValue((DependencyObject)x, true)), targetObject);
            }
        }

        /// <summary>
        /// Evaluates the value of the property and returns the value.
        /// </summary>
        /// <param name="targetObject"></param>
        /// <returns></returns>
        /// <remarks>
        /// CAUTION This method must be called on the thread of the <see cref="TargetObject"/>'s <see cref="Dispatcher"/>
        /// </remarks>
        internal object GetValue(DependencyObject targetObject)
        {
            if (targetObject == null)
            {
                throw new ArgumentNullException(nameof(targetObject));
            }

            return GetOrUpdateValue(targetObject, false);
        }

        /// <summary>
        /// Evaluates the value of the property, optionally updates the property and returns the value.
        /// </summary>
        /// <param name="update">
        /// <c>true</c> to update the property's value; otherwise, <c>false</c>.
        /// </param>
        /// <returns></returns>
        /// <remarks>
        /// CAUTION This method must be called on the thread of the <see cref="TargetObject"/>'s <see cref="Dispatcher"/>
        /// </remarks>
        object GetOrUpdateValue(DependencyObject targetObject, bool update)
        {
            Debug.Assert(targetObject != null);

            var dispatcher = targetObject.Dispatcher;

            var cultureInfo = LocalizationScope.GetCulture(targetObject) ?? dispatcher.Thread.CurrentCulture;
            var uiCultureInfo = LocalizationScope.GetUICulture(targetObject) ?? dispatcher.Thread.CurrentUICulture;

            #region Resource value

            object resourceValue;

            if (string.IsNullOrEmpty(Key))
            {
                // Only the culture or formatting is localized
                resourceValue = null;
            }
            else
            {
                var resourceManager = ResourceManagerHelper.GetResourceManager(targetObject);

                if (resourceManager == null)
                {
                    if (TargetProperty.PropertyType.IsAssignableFrom(typeof(string)))
                    {
                        if (update)
                        {
                            TargetProperty.SetValue(targetObject, ErrorMessage_ResourceManagerNotFound);
                        }
                    }
                    else
                    {
                        // Do nothing
                    }

                    return TypeUtils.GetValueOrDefaultValue(TargetProperty.PropertyType, ErrorMessage_ResourceManagerNotFound);
                }
                else
                {
                    resourceValue = resourceManager.GetObject(Key, uiCultureInfo);
                    if (resourceValue == null)
                    {
                        var errorMessage = string.Format(ErrorMessage_ResourceKeyNotFound, Key);

                        if (TargetProperty.PropertyType.IsAssignableFrom(typeof(string)))
                        {
                            if (update)
                            {
                                TargetProperty.SetValue(targetObject, errorMessage);
                            }
                        }
                        else
                        {
                            // Do nothing
                        }

                        return TypeUtils.GetValueOrDefaultValue(TargetProperty.PropertyType, errorMessage);
                    }
                }
            }

            #endregion

            #region Binding

            if (Binding != null || Bindings?.Count > 0)
            {
                // Use the binding to obtain the value and then use the resource value or format string to format it

                var stringFormat = (resourceValue as string).NullIfEmpty() ?? StringFormat.NullIfEmpty() ?? "{0}";

                // A binding cannot be changed once "BindingBase.ProvideValue" is invoked therefore, it must be recreated if the
                // formatting string or the culture has changed
                if (_finalBinding == null || _finalBinding.StringFormat != stringFormat || _finalBinding.ConverterCulture != cultureInfo)
                {
                    Debug.Assert(TargetProperty is LocalizedDepProperty, "Bindings are supported only on dependency properties.");

                    // Prepare the binding
                    var finalBinding = new MultiBinding()
                    {
                        Mode = BindingMode.OneWay,
                        StringFormat = stringFormat,
                        ConverterCulture = cultureInfo,
                    };

                    if (Binding != null)
                    {
                        finalBinding.Bindings.Add(Binding);
                    }
                    else
                    {
                        foreach (var item in Bindings)
                        {
                            finalBinding.Bindings.Add(item);
                        }
                    }

                    this._finalBinding = finalBinding;
                    this._finalBindingExpression = (BindingExpressionBase)_finalBinding.ProvideValue(this);

                    if (update)
                    {
                        // Set the value
                        TargetProperty.SetValue(targetObject, _finalBindingExpression);
                    }
                }
                else
                {
                    if (update)
                    {
                        _finalBindingExpression.UpdateTarget();
                    }
                }

                return _finalBindingExpression;
            }
            #endregion
            else
            {
                object targetValue;

                if (Converter != null)
                {
                    targetValue = Converter.Convert(resourceValue, TargetProperty.PropertyType, ConverterParameter, cultureInfo);
                }
                else if (false == TargetProperty.PropertyType.IsAssignableFrom(resourceValue.GetType()))
                {
                    // Use an appropriate converter
                    targetValue = DefaultValueConverter.Instance.Convert(resourceValue, TargetProperty.PropertyType, null, cultureInfo);
                }
                else
                {
                    targetValue = resourceValue;
                }

                if (update)
                {
                    TargetProperty.SetValue(targetObject, targetValue);
                }

                return targetValue;
            }
        }

        #region IServiceProvider Members

        /// <summary>
        /// Gets the service object of the specified type.
        /// </summary>
        /// <param name="serviceType">An object that specifies the type of service object to get.</param>
        /// <returns>
        /// A service object of type <paramref name="serviceType"/>.
        /// -or-
        /// null if there is no service object of type <paramref name="serviceType"/>.
        /// </returns>
        object IServiceProvider.GetService(Type serviceType)
        {
            if (serviceType == typeof(IProvideValueTarget))
            {
                return this;
            }
            else
            {
                return null;
            }
        }

        #endregion

        #region IProvideValueTarget Members

        /// <summary>
        /// Gets the target object being reported.
        /// </summary>
        /// <value></value>
        /// <returns>
        /// The target object being reported.
        /// </returns>
        object IProvideValueTarget.TargetObject
        {
            get
            {
                return TargetObject;
            }
        }

        object IProvideValueTarget.TargetProperty
        {
            get
            {
                return ((LocalizedDepProperty)TargetProperty).Property;
            }
        }

        #endregion
    }
}
