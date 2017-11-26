using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Threading;
using System.Xaml;

namespace WpfLocalization
{
    /// <summary>
    /// Represents a method to be called to obtain a localized value.
    /// </summary>
    /// <param name="culture"></param>
    /// <param name="uiCulture"></param>
    /// <param name="parameter"></param>
    /// <returns></returns>
    public delegate object LocalizationCallback(CultureInfo culture, CultureInfo uiCulture, object parameter, object dataBindingValue);

    [ContentProperty("Bindings")]
    [MarkupExtensionReturnType(typeof(object))]
    public class LocExtension : MarkupExtension
    {
        /// <summary>
        /// The name of the resource.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// The format string to use in conjunction with <see cref="Binding"/> or <see cref="Bindings"/>.
        /// </summary>
        public string StringFormat { get; set; }

        #region Callback

        /// <summary>
        /// A method to call to obtain the value or transform a value returned by the data binding.
        /// </summary>
        /// <remarks>
        /// CAUTION Specifying both a <see cref="Callback"/> and a <see cref="Converter"/> is not supported.
        /// If such case the <see cref="Converter"/> is ignored.
        /// </remarks>
        public LocalizationCallback Callback { get; set; }

        /// <summary>
        /// The parameter to pass to the callback.
        /// </summary>
        /// <remarks>
        /// CAUTION Specifying both a <see cref="Callback"/> and a <see cref="Converter"/> is not supported.
        /// If such case the <see cref="Converter"/> is ignored.
        /// </remarks>
        public object CallbackParameter { get; set; }

        #endregion

        #region Binding

        /// <summary>
        /// The binding to pass as argument to the format string.
        /// </summary>
        [DefaultValue(null)]
        public BindingBase Binding { get; set; }

        Collection<BindingBase> _bindings;

        /// <summary>
        /// The list of binding to pass as arguments to the format string.
        /// </summary>
        public Collection<BindingBase> Bindings
        {
            get
            {
                if (_bindings == null)
                {
                    _bindings = new Collection<BindingBase>();
                }

                return _bindings;
            }
        }

        #endregion

        #region Converter

        /// <summary>
        /// The converter to use to convert the value before it is assigned to the property.
        /// </summary>
        /// <remarks>
        /// CAUTION Specifying both a <see cref="Callback"/> and a <see cref="Converter"/> is not supported.
        /// If such case the <see cref="Converter"/> is ignored.
        /// </remarks>
        public IValueConverter Converter { get; set; }

        /// <summary>
        /// The parameter to pass to the converter.
        /// </summary>
        /// <remarks>
        /// CAUTION Specifying both a <see cref="Callback"/> and a <see cref="Converter"/> is not supported.
        /// If such case the <see cref="Converter"/> is ignored.
        /// </remarks>
        public object ConverterParameter { get; set; }

        #endregion

        public LocExtension() { }

        public LocExtension(string key)
        {
            this.Key = key;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            // Other useful services:
            // - IDestinationTypeProvider
            // - IRootObjectProvider

            if (!(serviceProvider.GetService(typeof(IProvideValueTarget)) is IProvideValueTarget service))
            {
                return null;
            }

            if (service.TargetObject is DependencyObject depObj)
            {
                if (Binding != null || _bindings?.Count > 0)
                {
                    if (false == (service.TargetProperty is DependencyProperty))
                    {
                        // The what bindings are implemented in WPF provides no way to obtain the value
                        // produced by the binging. The only way is to update the property directly. Therefore,
                        // the extension cannot support bindings on non-dependency properties (same as WPF).
                        throw new NotSupportedException("Bindings are supported only on dependency properties.");
                    }
                }

                LocalizedProperty localizedProperty;

                if (service.TargetProperty is DependencyProperty depProperty)
                {
                    localizedProperty = new LocalizedDepProperty(depProperty);
                }
                else if (service.TargetProperty is PropertyInfo propertyInfo)
                {
                    localizedProperty = new LocalizedNonDepProperty(propertyInfo);
                }
                else
                {
                    throw new NotSupportedException($"The default property provider supports only dependency and non-dependency properties of dependency objects. Properties of type {service.TargetProperty.GetType()} are not supported.");
                }

                var localizedValue = new LocalizedValue(depObj, localizedProperty)
                {
                    Key = this.Key,
                    StringFormat = this.StringFormat,
                    Callback = this.Callback,
                    CallbackParameter = this.CallbackParameter,
                    Binding = this.Binding,
                    Bindings = this._bindings,
                    Converter = this.Converter,
                    ConverterParameter = this.ConverterParameter,
                };

                LocalizationManager.Add(localizedValue);

                if (localizedValue.IsInDesignMode)
                {
                    // At design time VS designer does not set the parent of any control
                    // before its properties are set. For this reason the correct values
                    // of inherited attached properties cannot be obtained.
                    // Therefore, to display the correct localized value it must be updated
                    // later after the parent of the control has been set.

                    depObj.Dispatcher.BeginInvoke(
                        DispatcherPriority.ApplicationIdle,
                        new SendOrPostCallback(x => ((LocalizedValue)x).UpdateValue()),
                        localizedValue
                        );

                    return localizedProperty.DefaultValue;
                }
                else
                {
                    return localizedValue.GetValue(depObj);
                }
            }
            else if (service.TargetObject is Setter)
            {
                if (Binding != null || _bindings?.Count > 0)
                {
                    throw new NotSupportedException("Bindings are not supported in style setters.");
                }

                var rootObjectProvider = (IRootObjectProvider)serviceProvider.GetService(typeof(IRootObjectProvider));

                Debug.Assert(rootObjectProvider != null);

                var rootObject = rootObjectProvider.RootObject as DependencyObject;

                Debug.Assert(rootObject != null);

                var destinationTypeProvider = (IDestinationTypeProvider)serviceProvider.GetService(typeof(IDestinationTypeProvider));

#if DEPRECATED // Unfortunately "GetDestinationType" throws NullReferenceException therefore, this approach is not applicable for retrieving the property's type
                Debug.Assert(destinationTypeProvider != null);

                var targetPropertyType = destinationTypeProvider.GetDestinationType();

                var localizedValue = new SetterLocalizedValue(rootObject, targetPropertyType)
                {
                    Key = this.Key,
                    Callback = this.Callback,
                    CallbackParameter = this.CallbackParameter,
                    Converter = this.Converter,
                    ConverterParameter = this.ConverterParameter,
                };
#endif

                var localizedValue = new SetterLocalizedValue(rootObject)
                {
                    Key = this.Key,
                    Callback = this.Callback,
                    CallbackParameter = this.CallbackParameter,
                    Converter = this.Converter,
                    ConverterParameter = this.ConverterParameter,
                };

                var binding = new Binding(nameof(SetterLocalizedValue.Value))
                {
                    Source = localizedValue,
                    Mode = BindingMode.OneWay,
                    Converter = localizedValue,
                };

                LocalizationManager.Add(localizedValue);

                return binding.ProvideValue(serviceProvider);
            }
            else if (service.TargetProperty is DependencyProperty || service.TargetProperty is PropertyInfo)
            {
                // The extension is used in a template and will be evaluated once the template is instantiated
                return this;
            }
            else
            {
                throw new NotSupportedException($"The localization extension can be used only with {nameof(DependencyObject)}s.");
            }
        }
    }
}
