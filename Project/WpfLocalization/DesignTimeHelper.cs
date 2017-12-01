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
    }
}
