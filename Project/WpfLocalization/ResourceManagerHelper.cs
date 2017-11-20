using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Windows;

namespace WpfLocalization
{
    /// <summary>
    /// Provides methods to work with <see cref="ResourceManager"/> instances.
    /// </summary>
    public static class ResourceManagerHelper
    {
        static DependencyProperty DefaultResourceManagerProperty = DependencyProperty.RegisterAttached(
            "Localization_DefaultResourceManager",
            typeof(ResourceManager),
            typeof(ResourceManagerHelper),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.OverridesInheritanceBehavior)
            );

        /// <summary>
        /// Determines the <see cref="ResourceManager"/> to use for the specified <see cref="DependencyObject"/>.
        /// </summary>
        /// <param name="dependencyObject"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">The method is not called on the UI thread of the specified <see cref="DependencyObject"/>.</exception>
        public static ResourceManager GetResourceManager(DependencyObject dependencyObject)
        {
            if (dependencyObject == null)
            {
                throw new ArgumentNullException(nameof(dependencyObject));
            }

            dependencyObject.VerifyAccess();

            var resourceManager = LocalizationScope.GetResourceManager(dependencyObject);
            if (resourceManager == null)
            {
                var window = Window.GetWindow(dependencyObject);

                if (window != null)
                {
                    var localValue = window.ReadLocalValue(DefaultResourceManagerProperty);
                    if (localValue == DependencyProperty.UnsetValue)
                    {
                        resourceManager = GetDefaultResourceManagerForAssembly(window.GetType().Assembly);
                        window.SetValue(DefaultResourceManagerProperty, resourceManager);
                    }
                    else
                    {
                        resourceManager = localValue as ResourceManager;
                    }
                }
            }

            return resourceManager;
        }

        /// <summary>
        /// Maps a <see cref="Type"/> to its corresponding <see cref="ResourceManager"/>.
        /// </summary>
        static ConcurrentDictionary<Type, ResourceManager> _resourceManagerDict = new ConcurrentDictionary<Type, ResourceManager>();

        /// <summary>
        /// Returns the <see cref="ResourceManager"/> that corresponds to the specified <see cref="Type"/>.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal static ResourceManager GetResourceManager(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            return _resourceManagerDict.GetOrAdd(type, x => new ResourceManager(x));
        }

        /// <summary>
        /// Maps an assembly to its default resource manager.
        /// </summary>
        static ConcurrentDictionary<Assembly, ResourceManager> _defaultResourceManagerDict = new ConcurrentDictionary<Assembly, ResourceManager>();

        /// <summary>
        /// Returns the default resource manager to be used for localization of <see cref="Window"/>s and <see cref="UserControl"/>s
        /// located in the specified <see cref="Assembly"/>.
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public static ResourceManager GetDefaultResourceManagerForAssembly(Assembly assembly)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            if (_defaultResourceManagerDict.TryGetValue(assembly, out ResourceManager result))
            {
                return result;
            }

            var resourceManagerType = assembly.GetType(assembly.GetName().Name + ".Properties.Resources", false) // C#
                ?? assembly.GetType(assembly.GetName().Name + ".Resources", false); // VB.NET

            var resourceManager = resourceManagerType == null ? null : GetResourceManager(resourceManagerType);

            _defaultResourceManagerDict.TryAdd(assembly, resourceManager);

            return resourceManager;
        }
    }
}
