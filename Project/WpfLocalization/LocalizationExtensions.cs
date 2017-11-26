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
        /// Returns an object that can be used to localize the specified property of the specified <see cref="DependencyObject"/>.
        /// </summary>
        /// <param name="depObj">The owner of the property.</param>
        /// <param name="property">The property to localize.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"><paramref name="depObj"/> or <see cref="property"/> or null.</exception>
        public static LocalizableProperty Property(this DependencyObject depObj, DependencyProperty property)
        {
            if (depObj == null)
            {
                throw new ArgumentNullException(nameof(depObj));
            }
            if (property == null)
            {
                throw new ArgumentNullException(nameof(property));
            }

            return new LocalizableProperty(depObj, new LocalizedDepProperty(property));
        }

        /// <summary>
        /// Returns an object that can be used to localize the specified property of the specified <see cref="DependencyObject"/>.
        /// </summary>
        /// <param name="depObj">The owner of the property.</param>
        /// <param name="property">The property to localize.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"><paramref name="depObj"/> or <see cref="property"/> or null.</exception>
        public static LocalizableProperty Property(this DependencyObject depObj, PropertyInfo property)
        {
            if (depObj == null)
            {
                throw new ArgumentNullException(nameof(depObj));
            }
            if (property == null)
            {
                throw new ArgumentNullException(nameof(property));
            }

            return new LocalizableProperty(depObj, new LocalizedNonDepProperty(property));
        }

        /// <summary>
        /// Returns an object that can be used to localize the specified property of the specified <see cref="DependencyObject"/>.
        /// </summary>
        /// <param name="depObj">The owner of the property.</param>
        /// <param name="propertyName">The name of the property to localize.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"><paramref name="depObj"/> is null or <see cref="propertyName"/> or null or empty.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// A dependency or a non-dependency property named <paramref name="propertyName"/> was not found.
        /// </exception>
        public static LocalizableProperty Property(this DependencyObject depObj, string propertyName)
        {
            if (depObj == null)
            {
                throw new ArgumentNullException(nameof(depObj));
            }
            if (string.IsNullOrEmpty(propertyName))
            {
                throw new ArgumentNullException(nameof(propertyName));
            }

            if (DependencyPropertyDescriptor.FromName(propertyName, depObj.GetType(), depObj.GetType()) is DependencyPropertyDescriptor depProperty)
            {
                return new LocalizableProperty(depObj, new LocalizedDepProperty(depProperty.DependencyProperty));
            }
            else if (depObj.GetType().GetProperty(propertyName) is PropertyInfo propertyInfo)
            {
                return new LocalizableProperty(depObj, new LocalizedNonDepProperty(propertyInfo));
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(propertyName), propertyName, "Property not found.");
            }
        }

        #endregion

        #region Set value

        /// <summary>
        /// Localizes the specified property with a resource value.
        /// </summary>
        /// <param name="property"></param>
        /// <param name="key"></param>
        public static void Resource(this LocalizableProperty property, string key)
        {
            var localizedValue = new LocalizedValue(property.TargetObject, property.TargetProperty)
            {
                Key = key,
            };

            LocalizationManager.Add(localizedValue);
        }

        /// <summary>
        /// Localizes the specified property with a formatting resource string and bindings.
        /// </summary>
        /// <param name="property"></param>
        /// <param name="key"></param>
        /// <param name="bindings"></param>
        public static void ResourceFormat(this LocalizableProperty property, string key, params BindingBase[] bindings)
        {
            ResourceFormat(property, key, (IList<BindingBase>)bindings);
        }

        /// <summary>
        /// Localizes the specified property with a formatting resource string and bindings.
        /// </summary>
        /// <param name="property"></param>
        /// <param name="key"></param>
        /// <param name="bindings"></param>
        public static void ResourceFormat(this LocalizableProperty property, string key, IList<BindingBase> bindings)
        {
            if (bindings == null || bindings.Count == 0)
            {
                Resource(property, key);
            }
            else
            {
                var localizedValue = new LocalizedValue(property.TargetObject, property.TargetProperty)
                {
                    Key = key,
                };

                if (bindings.Count == 1)
                {
                    localizedValue.Binding = bindings[0];
                }
                else
                {
                    localizedValue.Bindings = new Collection<BindingBase>(bindings);
                }

                LocalizationManager.Add(localizedValue);
            }
        }

        /// <summary>
        /// Localizes the specified property with a formatting string and bindings.
        /// </summary>
        /// <param name="property"></param>
        /// <param name="stringFormat"></param>
        /// <param name="bindings"></param>
        public static void Format(this LocalizableProperty property, string stringFormat, params BindingBase[] bindings)
        {
            Format(property, stringFormat, (IList<BindingBase>)bindings);
        }

        /// <summary>
        /// Localizes the specified property with a formatting string and bindings.
        /// </summary>
        /// <param name="property"></param>
        /// <param name="stringFormat"></param>
        /// <param name="bindings"></param>
        public static void Format(this LocalizableProperty property, string stringFormat, IList<BindingBase> bindings)
        {
            var localizedValue = new LocalizedValue(property.TargetObject, property.TargetProperty)
            {
                StringFormat = stringFormat,
            };

            if (bindings == null || bindings.Count == 0)
            {
                // Do nothing
            }
            else if (bindings.Count == 1)
            {
                localizedValue.Binding = bindings[0];
            }
            else
            {
                localizedValue.Bindings = new Collection<BindingBase>(bindings);
            }

            LocalizationManager.Add(localizedValue);
        }

        #region Callback

        /// <summary>
        /// Localizes the specified property by using a callback to obtain a localized value.
        /// </summary>
        /// <param name="property"></param>
        /// <param name="callbackParameter"></param>
        public static void Callback(this LocalizableProperty property, LocalizationCallback callback, object callbackParameter)
        {
            var localizedValue = new LocalizedValue(property.TargetObject, property.TargetProperty)
            {
                Callback = callback,
                CallbackParameter = callbackParameter,
            };

            LocalizationManager.Add(localizedValue);
        }

        /// <summary>
        /// Localizes the specified property by using a callback to obtain a localized value.
        /// </summary>
        /// <param name="property"></param>
        /// <param name="callbackParameter"></param>
        public static void CallbackFormat(this LocalizableProperty property, LocalizationCallback callback, object callbackParameter, string stringFormat)
        {
            CallbackFormat(property, callback, callbackParameter, stringFormat, null);
        }

        /// <summary>
        /// Localizes the specified property by using a callback to obtain a localized value.
        /// </summary>
        /// <param name="property"></param>
        /// <param name="callbackParameter"></param>
        public static void CallbackFormat(this LocalizableProperty property, LocalizationCallback callback, object callbackParameter, string stringFormat, BindingBase binding)
        {
            var localizedValue = new LocalizedValue(property.TargetObject, property.TargetProperty)
            {
                StringFormat = stringFormat,
                Binding = binding,
                Callback = callback,
                CallbackParameter = callbackParameter,
            };

            LocalizationManager.Add(localizedValue);
        }

        /// <summary>
        /// Localizes the specified property by using a callback to obtain a localized value.
        /// </summary>
        /// <param name="property"></param>
        /// <param name="callbackParameter"></param>
        public static void CallbackResourceFormat(this LocalizableProperty property, LocalizationCallback callback, object callbackParameter, string resourceKey)
        {
            CallbackResourceFormat(property, callback, callbackParameter, resourceKey, null);
        }

        /// <summary>
        /// Localizes the specified property by using a callback to obtain a localized value.
        /// </summary>
        /// <param name="property"></param>
        /// <param name="callbackParameter"></param>
        public static void CallbackResourceFormat(this LocalizableProperty property, LocalizationCallback callback, object callbackParameter, string resourceKey, BindingBase binding)
        {
            var localizedValue = new LocalizedValue(property.TargetObject, property.TargetProperty)
            {
                Key = resourceKey,
                Binding = binding,
                Callback = callback,
                CallbackParameter = callbackParameter,
            };

            LocalizationManager.Add(localizedValue);
        }

        #endregion

        #endregion

        /// <summary>
        /// Removes the localization from the specified property.
        /// </summary>
        /// <param name="property"></param>
        /// <remarks>
        /// This method stops localizing the specified value (the value will no longer be updated
        /// when the culture changes). However, this method does not remove the value that is already set. If there
        /// was a localized binding and the some of the data bound values change the property will still be updated.
        /// </remarks>
        public static void Remove(this LocalizableProperty property)
        {
            LocalizationManager.RemoveProperty(property.TargetObject, property.TargetProperty);
        }
    }
}
