﻿using System;
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

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Loc")]
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
        public LocalizationCallback Callback { get; set; }

        /// <summary>
        /// The parameter to pass to the callback.
        /// </summary>
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
        public IValueConverter Converter { get; set; }

        /// <summary>
        /// The parameter to pass to the converter.
        /// </summary>
        public object ConverterParameter { get; set; }

        #endregion

        public LocExtension() { }

        public LocExtension(string key)
        {
            this.Key = key;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "UserControl")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "ResourceDictionary")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object)")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "DependencyObject")]
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }

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

                LocalizableProperty localizableProperty;

                if (service.TargetProperty is DependencyProperty depProperty)
                {
                    localizableProperty = new LocalizableDepProperty(depProperty);
                }
                else if (service.TargetProperty is PropertyInfo propertyInfo)
                {
                    localizableProperty = new LocalizableNonDepProperty(propertyInfo);
                }
                else
                {
                    throw new NotSupportedException($"The extension supports only dependency and non-dependency properties of dependency objects. Properties of type {service.TargetProperty.GetType()} are not supported.");
                }

                var options = new LocalizationOptions()
                {
                    Key = Key,
                    StringFormat = StringFormat,
                    Binding = Binding,
                    Bindings = _bindings,
                    Callback = Callback,
                    CallbackParameter = CallbackParameter,
                    Converter = Converter,
                    ConverterParameter = ConverterParameter,
                };

                var localizedValue = LocalizedValue.Create(new DependencyObjectProperty(depObj, localizableProperty), options);

                LocalizationManager.Add(localizedValue);

                if (localizedValue.BindingExpression != null)
                {
                    // The value uses bindings
                    return localizedValue.BindingExpression;
                }
                else
                {
                    // The value does not use bindings

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

                        return localizableProperty.DefaultValue;
                    }
                    else
                    {
                        return localizedValue.ProduceValue();
                    }
                }
            }
            else if (service.TargetObject is Setter)
            {
                var rootObjectProvider = (IRootObjectProvider)serviceProvider.GetService(typeof(IRootObjectProvider));

                // IRootObjectProvider is never null
                // IRootObjectProvider.RootObject is null when the style is located in a separate resource dictionary file
                // or the application's resource dictionary in App.xaml
                // Otherwise, the value can be Window or UserControl depending on the where the style is declared.
                // Therefore, it can be assumed that once root object (if any) is disposed the localized value
                // can be disposed as well.
                var rootObject = rootObjectProvider?.RootObject as DependencyObject;

                if (rootObject == null)
                {
                    // There is no way to access the resource manager (apart from the resource manager of the entry assembly).
                    // In order to avoid misunderstanding and bugs localizing setters is not supported unless they are delcared inside a Window or a UserControl
                    throw new NotSupportedException("Setters are supported only inside a Window or a UserControl. They are not supported inside a ResourceDictionary file or App.xaml.");
                }

                var options = new LocalizationOptions()
                {
                    Key = Key,
                    StringFormat = StringFormat,
                    Binding = Binding,
                    Bindings = _bindings,
                    Callback = Callback,
                    CallbackParameter = CallbackParameter,
                    Converter = Converter,
                    ConverterParameter = ConverterParameter,
                };

                var localizedValueTuple = LocalizedSetterValue.Create(rootObject, options);

                LocalizationManager.Add(localizedValueTuple.Item1);

                return localizedValueTuple.Item2.ProvideValue(serviceProvider);
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
