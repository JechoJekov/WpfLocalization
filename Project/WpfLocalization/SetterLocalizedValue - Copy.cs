using System;
using System.Collections.Generic;
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
    class SetterLocalizedValue : LocalizedValueBase, INotifyPropertyChanged, IValueConverter
    {
        /// <summary>
        /// Returns the <see cref="Dispatcher"/> of the owner of the localized property.
        /// </summary>
        /// <remarks>
        /// CAUTION This value is <c>null</c> if the owner has been GC.
        /// </remarks>
        public override Dispatcher Dispatcher => Dispatcher.FromThread(Thread.CurrentThread);

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// The localized value.
        /// </summary>
        public object Value
        {
            get
            {
                return GetValue();
            }
        }

        void FireValueChanged()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
        }

        IValueConverter _finalConverter;
        object _finalConverterParameter;

        DependencyObject _rootDependencyObject;
        Type _targetPropertyType;

        public SetterLocalizedValue(DependencyObject rootDependencyObject, Type targetPropertyType)
        {
            this._rootDependencyObject = rootDependencyObject ?? throw new ArgumentNullException(nameof(rootDependencyObject));
            this._targetPropertyType = targetPropertyType ?? throw new ArgumentNullException(nameof(targetPropertyType));
        }

        /// <summary>
        /// Returns the localized value.
        /// </summary>
        /// <returns></returns>
        object GetValue()
        {
            var dispatcher = Dispatcher.FromThread(Thread.CurrentThread);

            Debug.Assert(dispatcher != null);

            var cultureInfo = dispatcher.Thread.CurrentCulture;
            var uiCultureInfo = dispatcher.Thread.CurrentUICulture;

            #region Converter

            if (_finalConverter == null)
            {
                if (Callback != null)
                {
                    _finalConverter = new CallbackValueConverter(this);
#if GOOD_DESIGN_MORE_MEMORY
                    _finalConverter = new CallbackValueConverter(Callback, Converter, ConverterParameter);
#endif
                    _finalConverterParameter = CallbackParameter;
                }
                else if (Converter != null)
                {
                    _finalConverter = Converter;
                    _finalConverterParameter = ConverterParameter;
                }
            }

            if (_finalConverterParameter is CallbackValueConverter callbackValueConverter)
            {
                callbackValueConverter.UICulture = uiCultureInfo;
            }

            #endregion

            #region Resource value

            object resourceValue;

            if (string.IsNullOrEmpty(Key))
            {
                // Only the culture or formatting is localized
                resourceValue = null;
            }
            else
            {
                var resourceManager = ResourceManagerHelper.GetResourceManager(_rootDependencyObject);

                if (resourceManager == null)
                {
                    if (_targetPropertyType.IsAssignableFrom(typeof(string)))
                    {
                        return ErrorMessage_ResourceManagerNotFound;
                    }
                    else
                    {
                        return TypeUtils.GetDefaultValue(_targetPropertyType);
                    }
                }
                else
                {
                    resourceValue = resourceManager.GetObject(Key, uiCultureInfo);
                    if (resourceValue == null)
                    {
                        var errorMessage = string.Format(ErrorMessage_ResourceKeyNotFound, Key);

                        if (_targetPropertyType.IsAssignableFrom(typeof(string)))
                        {
                            return errorMessage;
                        }
                        else
                        {
                            return TypeUtils.GetDefaultValue(_targetPropertyType);
                        }
                    }
                }
            }

            #endregion

            object targetValue;

            if (_finalConverter != null)
            {
                targetValue = _finalConverter.Convert(resourceValue, _targetPropertyType, _finalConverterParameter, cultureInfo);
            }
            else if (false == _targetPropertyType.IsAssignableFrom(resourceValue.GetType()))
            {
                // Use an appropriate converter
                targetValue = DefaultValueConverter.Instance.Convert(resourceValue, _targetPropertyType, null, cultureInfo);
            }
            else
            {
                targetValue = resourceValue;
            }

            return targetValue;
        }

        public override void UpdateValue()
        {
            FireValueChanged();
        }

        #region IValueConverter

        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return GetValue(targetType);
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
