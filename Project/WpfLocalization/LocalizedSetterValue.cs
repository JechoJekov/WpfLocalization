using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;
using WpfLocalization.Converters;

namespace WpfLocalization
{
    /// <summary>
    /// Represents a localized value intended to be used with setters.
    /// </summary>
    /// <remarks>
    /// Implementing <see cref="IValueConverter"/> is necessary since this is the only way to obtain the type
    /// of the target property (see LocExtension.ProvideValue for more information).
    /// </remarks>
    class LocalizedSetterValue : LocalizedValueBase, IValueConverter, IMultiValueConverter, INotifyPropertyChanged
    {
        #region RootObject

        /// <summary>
        /// The root object.
        /// </summary>
        /// <remarks>
        /// CAUTION This value is <c>null</c> if the object has been GC.
        /// </remarks>
        public DependencyObject RootObject => _rootObject.Target as DependencyObject;

        /// <summary>
        /// Returns the <see cref="Dispatcher"/> of the root object.
        /// </summary>
        /// <remarks>
        /// CAUTION This value is <c>null</c> if the root object has been GC.
        /// </remarks>
        public override Dispatcher Dispatcher => RootObject?.Dispatcher;

        /// <summary>
        /// Returns a value indicating if the value can be purged.
        /// </summary>
        public override bool CanPurge()
        {
            // The instance can be purged if there is a root object that has been GC
            return _rootObject != null && false == _rootObject.IsAlive;
        }

        /// <summary>
        /// Gets a value indicating if the owner of the property is used in design mode.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public bool IsInDesignMode
        {
            get
            {
                return _rootObject !=null && _rootObject.Target is DependencyObject rootObject && DesignerProperties.GetIsInDesignMode(rootObject);
            }
        }

        /// <summary>
        /// The owner of the property.
        /// </summary>
        WeakReference _rootObject { get; }

        #endregion

        public LocalizedSetterValue(DependencyObject rootObject)
        {
            if (rootObject == null)
            {
                throw new ArgumentNullException(nameof(rootObject));
            }

            this._rootObject = new WeakReference(rootObject);
        }

        /// <summary>
        /// Returns the localized value.
        /// </summary>
        /// <returns></returns>
        object ProduceValue(Type targetPropertyType, bool dataBound, object dataBoundValueOrValues)
        {
            Debug.Assert(targetPropertyType != null);

            var rootObject = this.RootObject;
            if (rootObject == null)
            {
                // The object has been GC
                return TypeUtils.GetDefaultValue(targetPropertyType);
            }

            var dispatcher = rootObject.Dispatcher;

            var cultureInfo = LocalizationScope.GetCulture(rootObject) ?? dispatcher.Thread.CurrentCulture;
            var uiCultureInfo = LocalizationScope.GetUICulture(rootObject) ?? dispatcher.Thread.CurrentUICulture;
            var resourceManager = ResourceManagerHelper.GetResourceManager(rootObject);

            return base.ProduceValue(
                null,
                targetPropertyType,
                resourceManager,
                cultureInfo,
                uiCultureInfo,
                dataBound,
                dataBoundValueOrValues
                );
        }

        /// <summary>
        /// Updates the value of the property.
        /// </summary>
        /// <remarks>
        /// CAUTION This method must be called on a UI thread.
        /// </remarks>
        public override void UpdateValue()
        {
            // Unlike "LocalizedValue.UpdateValue" a "BindingExpression" cannot be used
            // since "BindingBase.ProvideValue" returns "BindingBase" instead of "BindingExpressionBase".
            // Therefore, the only option is to use "INotifyPropertyChanged"

            FireValueChanged();
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// The localized value.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public object Value
        {
            get
            {
                // The actual value is returned by the "IValueConverter.Convert" method and this value is ignored
                // Implementing "IValueConverter" is necessary since this is the only way to obtain the type
                // of the target property (see LocExtension.ProvideValue for more information).
                return null;
            }
        }

        void FireValueChanged()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
        }

        #endregion

        #region IValueConverter

        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // The single binding is used only if there is no binding specified in the extension. In such case
            // the binding is pointed to an empty value.
            return ProduceValue(targetType, false, null);
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        object IMultiValueConverter.Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            // The last value is internal and is used to trigger an update of the bound value. Normally it should be
            // removed from the array, however, since it can only be used in string.Format(...) for performance
            // reasons the array is not updated (the value is always null).

            Debug.Assert(values.Length >= 2);

            if (values.Length == 2)
            {
                // Ignore the internal second value
                return ProduceValue(targetType, true, values[0]);
            }
            else
            {
                // There are more than 1 bindings so the values can be used only in String.Format. Therefore, removing
                // the extra internal value (the last one in the array) is not necessary.
                return ProduceValue(targetType, true, values);
            }

        }

        object[] IMultiValueConverter.ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region Static methods

        /// <summary>
        /// Creates a new localized value.
        /// </summary>
        /// <returns></returns>
        public static Tuple<LocalizedSetterValue, BindingBase> Create(
            DependencyObject rootObject,
            LocalizationOptions options
            )
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            var localizedValue = new LocalizedSetterValue(rootObject)
            {
                Key = options.Key,
                StringFormat = options.StringFormat,
                Callback = options.Callback,
                CallbackParameter = options.CallbackParameter,
                Converter = options.Converter,
                ConverterParameter = options.ConverterParameter,
            };

            BindingBase finalBinding;

            if (options.Binding != null || options.Bindings?.Count > 0)
            {
                // Create a binding
                var localizedBinding = new MultiBinding()
                {
                    Mode = BindingMode.OneWay,
                };
                if (options.Binding != null)
                {
                    localizedBinding.Bindings.Add(options.Binding);
                }
                if (options.Bindings?.Count > 0)
                {
                    foreach (var item in options.Bindings)
                    {
                        localizedBinding.Bindings.Add(item);
                    }
                }

                // Add a binding that can be used to update the value
                localizedBinding.Bindings.Add(new Binding()
                {
                    Mode = BindingMode.OneWay,
                    Source = localizedValue,
                    Path = new PropertyPath(nameof(Value)),
                });

                localizedBinding.Converter = localizedValue;

                finalBinding = localizedBinding;
            }
            else
            {
                // Create a binding
                var localizedBinding = new Binding()
                {
                    Mode = BindingMode.OneWay,
                    Source = localizedValue,
                    Path = new PropertyPath(nameof(Value)),
                };

                localizedBinding.Converter = localizedValue;

                finalBinding = localizedBinding;
            }

            return Tuple.Create(localizedValue, finalBinding);
        }

        #endregion
    }
}
