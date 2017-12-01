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
    class LocalizedValue : LocalizedValueBase, IMultiValueConverter
    {
        #region TargetObject

        /// <summary>
        /// The object whose property is localized.
        /// </summary>
        /// <remarks>
        /// CAUTION This value is <c>null</c> if the object has been GC.
        /// </remarks>
        public DependencyObject TargetObject => _targetObject.Target as DependencyObject;

        /// <summary>
        /// Returns the <see cref="Dispatcher"/> of the owner of the localized property.
        /// </summary>
        /// <remarks>
        /// CAUTION This value is <c>null</c> if the owner has been GC.
        /// </remarks>
        public override Dispatcher Dispatcher => TargetObject?.Dispatcher;

        /// <summary>
        /// Returns a value indicating if the value can be purged.
        /// </summary>
        public override bool CanPurge()
        {
            return false == _targetObject.IsAlive;
        }

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
        public LocalizableProperty TargetProperty { get; }

        /// <summary>
        /// The binding created for the value (if any).
        /// </summary>
        internal BindingExpressionBase BindingExpression { get; private set; }

        public LocalizedValue(DependencyObject targetObject, LocalizableProperty targetProperty)
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
        /// <remarks>
        /// CAUTION This method must be called on a UI thread.
        /// </remarks>
        public override void UpdateValue()
        {
            var targetObject = this.TargetObject;
            if (targetObject == null)
            {
                // The object has been GC
                return;
            }

            if (BindingExpression != null)
            {
                BindingExpression.UpdateTarget();
            }
            else
            {
                var value = ProduceValue();
                TargetProperty.SetValue(targetObject, value);
            }
        }

        /// <summary>
        /// Determines the value of the property based on the current culture.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// This method is intended to be used by the <see cref="LocExtension"/> type to be able to return
        /// the value of the property when the extension is evaluated by WPF.
        /// </remarks>
        internal object ProduceValue()
        {
            return ProduceValue(false, null);
        }

        object ProduceValue(bool dataBound, object dataBoundValueOrValues)
        {
            var targetObject = this.TargetObject;
            if (targetObject == null)
            {
                // The object has been GC
                return TargetProperty.DefaultValue;
            }

            var dispatcher = targetObject.Dispatcher;
            var culture = LocalizationScope.GetCulture(targetObject) ?? dispatcher.Thread.CurrentCulture;
            var uiCulture = LocalizationScope.GetUICulture(targetObject) ?? dispatcher.Thread.CurrentUICulture;
            var resourceManager = ResourceManagerHelper.GetResourceManager(targetObject);

            return base.ProduceValue(
                TargetProperty,
                TargetProperty.PropertyType,
                resourceManager,
                culture,
                uiCulture,
                dataBound,
                dataBoundValueOrValues
                );
        }

        #region IValueConverter & IMultiValueConverter

        object IMultiValueConverter.Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return ProduceValue(true, values);
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
        /// <param name="targetObject"></param>
        /// <param name="targetProperty"></param>
        /// <param name="binding"></param>
        /// <param name="bindings"></param>
        /// <returns></returns>
        public static LocalizedValue Create(
            DependencyObjectProperty objectProperty,
            LocalizationOptions options
            )
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            var localizedValue = new LocalizedValue(objectProperty.TargetObject, objectProperty.TargetProperty)
            {
                Key = options.Key,
                StringFormat = options.StringFormat,
                Callback = options.Callback,
                CallbackParameter = options.CallbackParameter,
                Converter = options.Converter,
                ConverterParameter = options.ConverterParameter,
            };

            if (options.Binding != null || options.Bindings?.Count > 0)
            {
                if (false == (objectProperty.TargetProperty.PropertyObject is DependencyProperty))
                {
                    // The what bindings are implemented in WPF provides no way to obtain the value
                    // produced by the binging. The only way is to update the property directly. Therefore,
                    // the extension cannot support bindings on non-dependency properties (same as WPF).
                    throw new ArgumentException("Bindings are supported only on dependency properties.", nameof(options));
                }

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

                localizedBinding.Converter = localizedValue;
                localizedValue.BindingExpression = (BindingExpressionBase)localizedBinding.ProvideValue(objectProperty);
            }

            return localizedValue;
        }

        #endregion
    }
}
