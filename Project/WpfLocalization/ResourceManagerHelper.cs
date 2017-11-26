using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace WpfLocalization
{
    /// <summary>
    /// Provides methods to work with <see cref="ResourceManager"/> instances.
    /// </summary>
    public static class ResourceManagerHelper
    {
        #region GetResourceManager for DependencyObject 

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
                if (DesignerProperties.GetIsInDesignMode(dependencyObject))
                {
                    // Window.GetWindow returns "null" at design time
                    return GetDefaultResourceManagerForAssembly(DesignTimeHelper.GetDesignTimeAssembly());
                }
                else
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
            }

            return resourceManager;
        }

        #endregion

        #region GetResourceManager for type 

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

            return _resourceManagerDict.GetOrAdd(type, x => CreateResourceManager(x));
        }

        /// <summary>
        /// Creates a resource manager associated with the specified type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        static ResourceManager CreateResourceManager(Type type)
        {
            Debug.Assert(type != null);

            if (type.Namespace.EndsWith("My.Resources"))
            {
                // The resource file is located in "My Projects" of a VB.NET assembly. "ResourceManager" is unable to use
                // the type to determine the name of the resources.
                var resourceBaseName = type.Namespace.Substring(0, type.Namespace.Length - "My.Resources".Length) + type.Name;
                return new ResourceManager(resourceBaseName, type.Assembly);
            }
            else
            {
                return new ResourceManager(type);
            }
        }

        /// <summary>
        /// Returns the <see cref="ResourceManager"/> that corresponds to the specified type.
        /// </summary>
        /// <param name="typeName">Full or partial type name.</param>
        /// <returns>
        /// The type of the <see cref="ResourceManager"/> that corresponds to the specified type or
        /// <c>null</c> if no such type was found.
        /// </returns>
        /// <remarks>
        /// If <paramref name="typeName"/> is partial (only resource file name without extension) then
        /// the method looks for the type in the "Properties" (C#) / "My Project" (VB.NET) folder.
        /// </remarks>
        internal static ResourceManager GetResourceManager(Assembly assembly, string typeName)
        {
            var type = GetResourceManagerType(assembly, typeName);
            return type == null ? null : GetResourceManager(type);
        }

        /// <summary>
        /// Returns the type of a <see cref="ResourceManager"/>.
        /// </summary>
        /// <param name="typeName">Full or partial type name.</param>
        /// <returns>
        /// The type of the <see cref="ResourceManager"/> that corresponds to the specified name or
        /// <c>null</c> if no such type was found.
        /// </returns>
        /// <remarks>
        /// If <paramref name="typeName"/> is partial (only resource file name without extension) then
        /// the method looks for the type in the "Properties" (C#) / "My Project" (VB.NET) folder.
        /// </remarks>
        static Type GetResourceManagerType(Assembly assembly, string typeName)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }
            if (string.IsNullOrEmpty(typeName))
            {
                throw new ArgumentNullException(nameof(typeName));
            }

            if (typeName.IndexOf('.') < 0)
            {
                // Partial name
                return assembly.GetType(assembly.GetName().Name + ".Properties." + typeName, false) // C#
                   ?? assembly.GetType(assembly.GetName().Name + ".My.Resources." + typeName, false); // VB.NET
            }
            else
            {
                // Full name
                return assembly.GetType(typeName, false);
            }
        }

        #endregion

        #region GetDefaultResourceManagerForAssembly

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

            var resourceManager = GetResourceManager(assembly, "Resources");

            // The type should be remembered even if it is null, which means "no default resources found".
            _defaultResourceManagerDict.TryAdd(assembly, resourceManager);

            return resourceManager;
        }

        #endregion
    }
}
