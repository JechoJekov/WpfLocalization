using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace WpfLocalization
{
    /// <summary>
    /// Contains the options used to localize a property.
    /// </summary>
    public class LocalizationOptions
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
        public BindingBase Binding { get; set; }

        /// <summary>
        /// The list of binding to pass as arguments to the format string.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public ICollection<BindingBase> Bindings { get; set; }

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

        public LocalizationOptions() { }

        public LocalizationOptions(string key)
        {
            this.Key = key;
        }
    }
}
