using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace WpfLocalization
{
    /// <summary>
    /// Provides methods related to design-time support.
    /// </summary>
    static class DesignTimeHelper
    {
        static Tuple<Assembly> _designTimeAssembly;

        static string[] DesignTimeAssembly_IgnoreAssemblyNameList = { "Microsoft", "Newtonsoft", "Interop" };
        static string[] DesignTimeAssembly_IgnoreCompanyNameList = { "Microsoft", "Newtonsoft" };

        /// <summary>
        /// Checks if the specified assembly is a part of WPF designer rather than a part of the application being developed.
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        internal static bool IsWpfDesignerAssembly(Assembly assembly)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            if (assembly.GlobalAssemblyCache)
            {
                return true;
            }

            // The name of the assembly
            var assemblyName = assembly.GetName().Name;

            if (DesignTimeAssembly_IgnoreAssemblyNameList.Any(x => assemblyName.IndexOf(x, StringComparison.OrdinalIgnoreCase) >= 0))
            {
                // Avoid Microsoft and interoperability assemblies loaded by Visual Studio
                return true;
            }

            if (string.Equals(assemblyName, "Blend", StringComparison.OrdinalIgnoreCase))
            {
                // Avoid "Blend.exe" of Expression Blend
                return true;
            }

            var assemblyCompanyAttribute = (AssemblyCompanyAttribute)assembly.GetCustomAttributes(typeof(AssemblyCompanyAttribute), false).FirstOrDefault();
            if (assemblyCompanyAttribute != null && DesignTimeAssembly_IgnoreCompanyNameList.Any(x => assemblyCompanyAttribute.Company.IndexOf(x, StringComparison.OrdinalIgnoreCase) >= 0))
            {
                // Avoid Microsoft assemblies loaded by Visual Studio
                return true;
            }

            if (false == assembly.GetReferencedAssemblies().Any(x => x.Name.Equals("PresentationFramework", StringComparison.OrdinalIgnoreCase)))
            {
                // Ignore assemblies that do not use WPF
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns the design-time assembly.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Due to the way the WPF designer works it is not possible to determine the assembly in which
        /// a <see cref="Window"/> or a <see cref="UserControl"/> is located at design time. A workaround is to
        /// examine all loaded assemblies and return the first assembly which does not seem to belong to Microsoft.
        /// </remarks>
        internal static Assembly GetDesignTimeAssembly()
        {
            if (_designTimeAssembly == null)
            {
                var assemblyList = AppDomain.CurrentDomain.GetAssemblies();

                foreach (var item in assemblyList)
                {
                    if (IsWpfDesignerAssembly(item))
                    {
                        continue;
                    }

                    // Assume that the first non-Microsoft assembly is the one that contains the Window or UserControl
                    _designTimeAssembly = Tuple.Create(item);
                    break;
                }
            }
            return _designTimeAssembly.Item1;
        }

#if DEPRECATED

        /// <summary>
        /// Returns the assembly in which the specified dependency object is used.
        /// </summary>
        /// <param name=""></param>
        /// <param name=""></param>
        /// <returns></returns>
        /// <remarks>
        /// A dependency object can be a <see cref="Window"/> or a <see cref="UserControl"/> (or can be used inside one)
        /// either at design-time or runtime.
        /// </remarks>
        internal static Assembly GetDependencyObjectAssembly(DependencyObject depObj)
        {
            if (depObj == null)
            {
                throw new ArgumentNullException(nameof(depObj));
            }

            if (DesignerProperties.GetIsInDesignMode(depObj))
            {
                // WPF Designer uses an instance of "System.Windows.Controls.UserControl" to represent a user control
                // (not the actual type) and "Microsoft.VisualStudio.DesignTools.WpfDesigner.InstanceBuilders.WindowInstance"
                // to represent a mock window

                if (depObj is UserControl || (depObj is FrameworkElement frameworkElement && frameworkElement.Parent == null))
                {
                    // The element is a user control or a root element (e.g. a mock-up window - WindowInstance)

                    // Check if the object is a mock object or an actual instance. If it is an actual instance then
                    // the user control is used in another user control or a window
                    if (depObj.GetType() == typeof(UserControl) || IsWpfDesignerAssembly(depObj.GetType().Assembly))
                    {
                        // The object is a mock object; therefore, use the design-time assembly 
                        return GetDesignTimeAssembly();
                    }
                    else
                    {
                        // The object is an actual instance
                        return depObj.GetType().Assembly;
                    }
                }
                else
                {
                    throw new NotSupportedException("The 'LocalResourceManager' extension can be used only on <Window> and <UserControl> elements.");
                }
            }
            else
            {
                if (depObj is Window || depObj is UserControl)
                {
                    return depObj.GetType().Assembly;
                }
                else
                {
                    throw new NotSupportedException("The 'LocalResourceManager' extension can be used only on <Window> and <UserControl> elements.");
                }
            }
        }

#endif
    }
}
