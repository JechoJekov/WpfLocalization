using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace WpfLocalization
{
    /// <summary>
    /// Provide extension methods that provide support for code-behind localization.
    /// </summary>
    public static class LocalizationExtensions
    {
        #region Property

        /// <summary>
        /// Returns an object that can be used to localize the specified objectProperty of the specified <see cref="DependencyObject"/>.
        /// </summary>
        /// <param name="obj">The owner of the objectProperty.</param>
        /// <param name="dependencyProperty">The objectProperty to localize.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"><paramref name="obj"/> or <paramref name="dependencyProperty"/> or null.</exception>
        public static DependencyObjectProperty Property(this DependencyObject obj, DependencyProperty dependencyProperty)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }
            if (dependencyProperty == null)
            {
                throw new ArgumentNullException(nameof(dependencyProperty));
            }

            return new DependencyObjectProperty(obj, new LocalizableDepProperty(dependencyProperty));
        }

        /// <summary>
        /// Returns an object that can be used to localize the specified objectProperty of the specified <see cref="DependencyObject"/>.
        /// </summary>
        /// <param name="obj">The owner of the objectProperty.</param>
        /// <param name="propertyInfo">The objectProperty to localize.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"><paramref name="obj"/> or <paramref name="propertyInfo"/> or null.</exception>
        public static DependencyObjectProperty Property(this DependencyObject obj, PropertyInfo propertyInfo)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }
            if (propertyInfo == null)
            {
                throw new ArgumentNullException(nameof(propertyInfo));
            }

            return new DependencyObjectProperty(obj, new LocalizableNonDepProperty(propertyInfo));
        }

        /// <summary>
        /// Returns an object that can be used to localize the specified objectProperty of the specified <see cref="DependencyObject"/>.
        /// </summary>
        /// <param name="obj">The owner of the objectProperty.</param>
        /// <param name="propertyName">The name of the objectProperty to localize.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"><paramref name="obj"/> is null or <see cref="propertyName"/> or null or empty.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// A dependency or a non-dependency objectProperty named <paramref name="propertyName"/> was not found.
        /// </exception>
        public static DependencyObjectProperty Property(this DependencyObject obj, string propertyName)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }
            if (string.IsNullOrEmpty(propertyName))
            {
                throw new ArgumentNullException(nameof(propertyName));
            }

            if (DependencyPropertyDescriptor.FromName(propertyName, obj.GetType(), obj.GetType()) is DependencyPropertyDescriptor depProperty)
            {
                return new DependencyObjectProperty(obj, new LocalizableDepProperty(depProperty.DependencyProperty));
            }
            else if (obj.GetType().GetProperty(propertyName) is PropertyInfo propertyInfo)
            {
                return new DependencyObjectProperty(obj, new LocalizableNonDepProperty(propertyInfo));
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(propertyName), propertyName, "Property not found.");
            }
        }

        #endregion

        #region Set value

        /// <summary>
        /// Localizes the specified objectProperty.
        /// </summary>
        /// <param name="objectProperty">The objectProperty to localize.</param>
        /// <param name="options">The options describing how the objectProperty is to be localized.</param>
        /// <exception cref="ArgumentException">A binding is specified and the objectProperty is a non-dependency objectProperty.</exception>
        public static void Localize(this DependencyObjectProperty objectProperty, LocalizationOptions options)
        {
            var localizedValue = LocalizedValue.Create(objectProperty, options);

            LocalizationManager.Add(localizedValue);

            // Set the value initially
            if (localizedValue.BindingExpression != null)
            {
                // The value uses bindings
                localizedValue.TargetProperty.SetValue(localizedValue.TargetObject, localizedValue.BindingExpression);
            }
            else
            {
                // The value does not use bindings
                localizedValue.TargetProperty.SetValue(localizedValue.TargetObject, localizedValue.ProduceValue());
            }
        }

        #region Resource

        /// <summary>
        /// Localizes the specified objectProperty with a resource value.
        /// </summary>
        /// <param name="objectProperty"></param>
        /// <param name="key"></param>
        public static void Resource(this DependencyObjectProperty objectProperty, string key)
        {
            Resource(objectProperty, key, null);
        }

        /// <summary>
        /// Localizes the specified objectProperty with a resource value.
        /// </summary>
        /// <param name="objectProperty"></param>
        /// <param name="key"></param>
        public static void Resource(this DependencyObjectProperty objectProperty, string key, IValueConverter converter)
        {
            Localize(objectProperty, new LocalizationOptions() { Key = key, Converter = converter, });
        }

        #endregion

        #region Callback

        /// <summary>
        /// Localizes the specified objectProperty by using the specified callback to obtain a localized value.
        /// </summary>
        /// <param name="objectProperty"></param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1719:ParameterNamesShouldNotMatchMemberNames", MessageId = "1#")]
        public static void Callback(this DependencyObjectProperty objectProperty, LocalizationCallback callback)
        {
            CallbackOptions(objectProperty, callback);
        }

        /// <summary>
        /// Localizes the specified objectProperty by using the specified callback to obtain a value and then formats the value using the specified
        /// string.
        /// </summary>
        /// <param name="objectProperty"></param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "string")]
        public static void CallbackFormat(this DependencyObjectProperty objectProperty, LocalizationCallback callback, string stringFormat)
        {
            CallbackOptions(objectProperty, callback, stringFormat: stringFormat);
        }

        /// <summary>
        /// Localizes the specified objectProperty by using the specified callback to obtain a value and then formats the value using the specified
        /// string resource.
        /// </summary>
        /// <param name="objectProperty"></param>
        public static void CallbackResourceFormat(this DependencyObjectProperty objectProperty, LocalizationCallback callback, string resourceKey)
        {
            CallbackOptions(objectProperty, callback, resourceKey: resourceKey);
        }

        static void CallbackOptions(this DependencyObjectProperty objectProperty, LocalizationCallback callback, string stringFormat = null, string resourceKey = null)
        {
            Localize(objectProperty, new LocalizationOptions() { Callback = callback, StringFormat = stringFormat, Key = resourceKey });
        }

        #endregion

        #region Binding

        /// <summary>
        /// Localizes the specified objectProperty by using the specified binding to obtain a value and then formats the value using the specified
        /// string.
        /// </summary>
        /// <param name="objectProperty"></param>
        /// <param name="callbackParameter"></param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "string")]
        public static void BindingFormat(this DependencyObjectProperty objectProperty, BindingBase binding, string stringFormat)
        {
            BindingOptions(objectProperty, binding, null, stringFormat: stringFormat);
        }

        /// <summary>
        /// Localizes the specified objectProperty by using the specified binding to obtain a value and then formats the value using the specified
        /// string resource.
        /// </summary>
        /// <param name="objectProperty"></param>
        public static void BindingResourceFormat(this DependencyObjectProperty objectProperty, BindingBase binding, string resourceKey)
        {
            BindingOptions(objectProperty, binding, null, resourceKey: resourceKey);
        }

        /// <summary>
        /// Localizes the specified objectProperty by using the specified binding to obtain a value and then formats the value using the specified
        /// string.
        /// </summary>
        /// <param name="objectProperty"></param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "string")]
        public static void BindingFormat(this DependencyObjectProperty objectProperty, ICollection<BindingBase> bindings, string stringFormat)
        {
            BindingOptions(objectProperty, null, bindings, stringFormat: stringFormat);
        }

        /// <summary>
        /// Localizes the specified objectProperty by using the specified binding to obtain a value and then formats the value using the specified
        /// string resource.
        /// </summary>
        /// <param name="objectProperty"></param>
        public static void BindingResourceFormat(this DependencyObjectProperty objectProperty, ICollection<BindingBase> bindings, string resourceKey)
        {
            BindingOptions(objectProperty, null, bindings, resourceKey: resourceKey);
        }

        /// <summary>
        /// Localizes the specified objectProperty by using the specified binding to obtain a value and then formats the value using the specified
        /// string.
        /// </summary>
        /// <param name="objectProperty"></param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "string")]
        public static void BindingFormat(this DependencyObjectProperty objectProperty, string stringFormat, params BindingBase[] bindings)
        {
            BindingOptions(objectProperty, null, bindings, stringFormat: stringFormat);
        }

        /// <summary>
        /// Localizes the specified objectProperty by using the specified binding to obtain a value and then formats the value using the specified
        /// string resource.
        /// </summary>
        /// <param name="objectProperty"></param>
        public static void BindingResourceFormat(this DependencyObjectProperty objectProperty, string resourceKey, params BindingBase[] bindings)
        {
            BindingOptions(objectProperty, null, bindings, resourceKey: resourceKey);
        }

        static void BindingOptions(this DependencyObjectProperty objectProperty, BindingBase binding, ICollection<BindingBase> bindings, string stringFormat = null, string resourceKey = null)
        {
            Localize(objectProperty, new LocalizationOptions() { Binding = binding, Bindings = bindings, StringFormat = stringFormat, Key = resourceKey });
        }

        #endregion

        #endregion

        #region Remove

        /// <summary>
        /// Removes the localization from the specified objectProperty.
        /// </summary>
        /// <param name="objectProperty"></param>
        /// <remarks>
        /// This method stops localizing the specified value (the value will no longer be updated
        /// when the culture changes). However, this method does not remove the value that is already set. If there
        /// was a localized binding and the some of the data bound values change the objectProperty will still be updated.
        /// </remarks>
        public static void Remove(this DependencyObjectProperty objectProperty)
        {
            LocalizationManager.RemoveProperty(objectProperty.TargetObject, objectProperty.TargetProperty);
        }

        #endregion

        #region Resources

        /// <summary>
        /// Returns the specified resource based on the current culture of the specified object.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="resourceKey"></param>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// This method respects the resources and culture set via the <see cref="LocalizationScope.ResourceManager"/> 
        /// and <see cref="LocalizationScope.UICulture"/> attached properties when retrieving the resource.
        /// </para>
        /// <para>
        /// This method is thread-safe.
        /// </para>
        /// </remarks>
        public static object GetResource(this DependencyObject obj, string resourceKey)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            var dispatcher = obj.Dispatcher;
            var uiCultureInfo = LocalizationScope.GetUICulture(obj) ?? dispatcher.Thread.CurrentUICulture;

            var resourceManager = ResourceManagerHelper.GetResourceManager(obj);
            if (resourceManager == null)
            {
                return null;
            }
            else
            {
                return resourceManager.GetObject(resourceKey, uiCultureInfo);
            }
        }

        /// <summary>
        /// Returns the specified resource based on the current culture of the specified object.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="resourceKey"></param>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// This method respects the resources and culture set via the <see cref="LocalizationScope.ResourceManager"/> 
        /// and <see cref="LocalizationScope.UICulture"/> attached properties when retrieving the resource.
        /// </para>
        /// <para>
        /// This method is thread-safe.
        /// </para>
        /// </remarks>
        public static T GetResource<T>(this DependencyObject obj, string resourceKey) where T : class
        {
            return (T)GetResource(obj, resourceKey);
        }

        #endregion
    }
}
