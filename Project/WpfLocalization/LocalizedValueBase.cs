using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;
using WpfLocalization.Converters;

namespace WpfLocalization
{
    /// <summary>
    /// Base type for localized values.
    /// </summary>
    abstract class LocalizedValueBase
    {
        #region Constants

        /// <summary>
        /// The error message to display in place of a localized value when no resource manager was found.
        /// </summary>
        protected const string ErrorMessage_ResourceManagerNotFound = "[- Resource File Not Found -]";

        /// <summary>
        /// The error message to display in place of a localized value when the specified resource key was not found.
        /// </summary>
        protected const string ErrorMessage_ResourceKeyNotFound = "[{0}]";

        #endregion

        #region Basic properties

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

        #endregion

        #region Callback

        /// <summary>
        /// A method to call to obtain the value.
        /// </summary>
        public LocalizationCallback Callback { get; set; }

        /// <summary>
        /// The parameter to pass to the callback.
        /// </summary>
        public object CallbackParameter { get; set; }

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

        /// <summary>
        /// Returns the <see cref="Dispatcher"/> of the owner of the localized property.
        /// </summary>
        /// <remarks>
        /// CAUTION This value is <c>null</c> if the owner has been GC.
        /// </remarks>
        public abstract Dispatcher Dispatcher { get; }

        /// <summary>
        /// Returns a value indicating if the value can be purged.
        /// </summary>
        public abstract bool CanPurge();

        protected LocalizedValueBase() { }

        /// <summary>
        /// Updates the value of the property.
        /// </summary>
        /// <remarks>
        /// CAUTION This method must be called on a UI thread.
        /// </remarks>
        public abstract void UpdateValue();

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "StringFormat")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object)")]
        protected object ProduceValue(
            LocalizableProperty targetProperty,
            Type propertyType,
            ResourceManager resourceManager,
            CultureInfo culture,
            CultureInfo uiCulture,
            bool dataBound,
            object dataBoundValueOrValues
            )
        {
            if (propertyType == null)
            {
                throw new ArgumentNullException(nameof(propertyType));
            }
            if (resourceManager == null)
            {
                throw new ArgumentNullException(nameof(resourceManager));
            }
            if (culture == null)
            {
                throw new ArgumentNullException(nameof(culture));
            }
            if (uiCulture == null)
            {
                throw new ArgumentNullException(nameof(uiCulture));
            }

            /* Flow:
             * 1. Obtain value
             * 1.1. Binding -> Get binding value
             * 1.2. Multi-binding -> Get all values
             * 1.3. Resource -> Get resource value
             * 2. Convert value
             * 2.1. If Callback -> invoke (with data-bound value only since the resource value (if any) will be used as "StringFormat")
             * 2.2. If Converter -> invoke
             * 2.3. If NO Bindings AND NO Converter AND NO Callback AND NO StringFormat AND Different-Type -> Use default converter
             * 3. Format value
             * 3.1. If StringFormat -> Format
             * 3.2. If NO StringFormat AND (Bindings OR Callback) AND Resource is string -> Use resource for formatting
             */

            #region Obtain value

            object resourceValue;

            if (string.IsNullOrEmpty(this.Key))
            {
                // No resource value specified
                // Even if no data bindings are specified it is possible that a value is produced by the callback.
                // At the very least the StringFormat should be used even if "null" is passed
                /*
                if (false == dataBound)
                {
                    // No data bindings specified so return the default value
                    // return TypeUtils.GetDefaultValue(propertyType);
                    // Do NOT throw an exception in order to avoid design-time errors
                    //throw new InvalidOperationException("At least a resource or a binding must be specified.");
                }
                */
                resourceValue = null;
            }
            else
            {
                if (resourceManager == null)
                {
                    // Resource manager not found - return an error message if possible
                    return propertyType.IsAssignableFrom(typeof(string)) ? ErrorMessage_ResourceManagerNotFound
                        : targetProperty != null ? targetProperty.DefaultValue : TypeUtils.GetDefaultValue(propertyType);
                }
                else
                {
                    resourceValue = resourceManager.GetObject(this.Key, uiCulture);
                    if (resourceValue == null)
                    {
                        // Resource value not found - return an error message if possible
                        return propertyType.IsAssignableFrom(typeof(string)) ? string.Format(ErrorMessage_ResourceKeyNotFound, Key)
                            : targetProperty != null ? targetProperty.DefaultValue : TypeUtils.GetDefaultValue(propertyType);
                    }
                }
            }

            #endregion

            if (dataBound && resourceValue != null && false == resourceValue is string)
            {
                throw new NotSupportedException("Bindings can be combined only with string resources (since the string resource is used as 'StringFormat').");
            }
            if (Callback != null && resourceValue != null && false == resourceValue is string)
            {
                throw new NotSupportedException("A callback can be combined only with string resources (since the string resource is used as 'StringFormat').");
            }

            // List of data bound values (multi-bindings only)
            var dataBoundValueList = dataBoundValueOrValues as object[];

            // Indicates if the value is multi-binding
            var isMultiBinding = dataBoundValueList?.Length > 1;

            if (isMultiBinding)
            {
                // Converter is not supported for multi-bindings

                if (Callback != null)
                {
                    throw new NotSupportedException("Callback is not supported for multi-bindings.");
                }
                if (Converter != null)
                {
                    throw new NotSupportedException("Converter is not supported for multi-bindings.");
                }

                // Format the value

                var stringFormat = StringFormat.NullIfEmpty() ?? (resourceValue as string).NullIfEmpty();

                if (stringFormat == null)
                {
                    throw new InvalidOperationException("Either 'StringFormat' or a resource must be specified when multi-binding is used.");
                }

                return string.Format(culture, stringFormat, dataBoundValueList);
            }
            else if (dataBound)
            {
                // There is a single data-bound value
                var dataValue = dataBoundValueList == null ? dataBoundValueOrValues : dataBoundValueList[0];

                if (Callback != null)
                {
                    // Pass the data-bound value to the callback
                    dataValue = Callback(culture, uiCulture, CallbackParameter, dataValue);
                }
                if (Converter != null)
                {
                    dataValue = Converter.Convert(dataValue, propertyType, ConverterParameter, culture);
                }

                // Format the value
                var stringFormat = StringFormat.NullIfEmpty() ?? (resourceValue as string).NullIfEmpty();
                return stringFormat == null ? dataValue : string.Format(culture, stringFormat, dataValue);
            }
            else if (Callback != null)
            {
                // The callback will produce the value
                var dataValue = Callback(culture, uiCulture, CallbackParameter, null);

                if (Converter != null)
                {
                    dataValue = Converter.Convert(dataValue, propertyType, ConverterParameter, culture);
                }

                // Format the value
                var stringFormat = StringFormat.NullIfEmpty() ?? (resourceValue as string).NullIfEmpty();
                return stringFormat == null ? dataValue : string.Format(culture, stringFormat, dataValue);
            }
            else if (resourceValue != null)
            {
                // There is a resource value only
                var dataValue = resourceValue;

                if (Converter != null)
                {
                    dataValue = Converter.Convert(dataValue, propertyType, ConverterParameter, culture);
                }
                else if (false == propertyType.IsAssignableFrom(dataValue.GetType()))
                {
                    // Use the default converter to convert the resource value to the type of the property
                    dataValue = DefaultValueConverter.Instance.Convert(dataValue, propertyType, null, culture);
                }

                if (string.IsNullOrEmpty(StringFormat))
                {
                    return dataValue;
                }
                else
                {
                    // Format the value
                    return string.Format(culture, StringFormat, dataValue);
                }
            }
            else
            {
                // There is no binding, callback, or a resource value

                if (string.IsNullOrEmpty(StringFormat))
                {
                    return targetProperty != null ? targetProperty.DefaultValue : TypeUtils.GetDefaultValue(propertyType);
                }
                else
                {
                    // Format the value
                    return StringFormat;
                }
            }
        }
    }
}
