using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace WpfLocalization
{
    /// <summary>
    /// Represents a property that can be localized.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
    public struct LocalizableProperty
    {
        /// <summary>
        /// The owner of the property.
        /// </summary>
        public DependencyObject TargetObject { get; }

        /// <summary>
        /// The property.
        /// </summary>
        public LocalizedProperty TargetProperty { get; }

        public LocalizableProperty(DependencyObject targetObject, LocalizedProperty targetProperty)
        {
            this.TargetObject = targetObject ?? throw new ArgumentNullException(nameof(targetObject));
            this.TargetProperty = targetProperty ?? throw new ArgumentNullException(nameof(targetProperty));
        }
    }
}
