using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Threading;

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

        /// <summary>
        /// The name of the resource.
        /// </summary>
        /// <remarks>
        /// </remarks>
        public string Key { get; set; }

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

        protected LocalizedValueBase() { }

        /// <summary>
        /// Updates the value of the property.
        /// </summary>
        /// <remarks>
        /// CAUTION This method must be called on a UI thread.
        /// </remarks>
        public abstract void UpdateValue();
    }
}
