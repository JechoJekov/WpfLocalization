﻿using System;
using System.Collections.Generic;

using System.Text;
using System.Windows.Markup;
using System.Resources;
using System.Reflection;

namespace WpfLocalization
{
    /// <summary>
    /// Loads the resources specified by a <see cref="Type"/> or an assembly and resource name.
    /// </summary>
    [MarkupExtensionReturnType(typeof(ResourceManager))]
    public class ResourceManagerExtension : MarkupExtension
    {
        /// <summary>
        /// The type of the code behind of the resource file (e.g. "{x:Type properties:MyResources}" where "properties" is a registered namespace).
        /// </summary>
        /// <remarks>
        /// A resource file can be specified by either type or an assembly name and a resource file name.
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <Window ... xmlns:properties="clr-namespace:MyApplication.Properties;assembly=MyApplication" ...>
        /// ...
        ///     <Grid LocalizationScope.ResourceManager="{ResourceManager Type={x:Type properties:MyResources}"}>
        ///     ...
        ///     </Grid>
        /// ...
        /// </Window>
        /// ]]>
        /// </code>
        /// </example>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods")]
        public Type Type { get; set; }

        /// <summary>
        /// The name of the assembly that contains the resources.
        /// </summary>
        /// <remarks>
        /// A resource file can be specified by either type or an assembly name and a resource file name.
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <Window>
        /// ...
        ///     <Grid LocalizationScope.ResourceManager="{ResourceManager AssemblyName=MyAssembly, ResourcePath=MyAssembly.Resources.MyResources}">
        ///     ...
        ///     </Grid>
        /// ...
        /// </Window>
        /// ]]>
        /// </code>
        /// </example>
        public string AssemblyName { get; set; }

        /// <summary>
        /// The full or partial name of the resource file including namespace and file name (e.g. "MyApplication.Properties.MyResources").
        /// If partial name is specified then the file is expected to be located in the "Properties" (C#) or "My Project" (VB.NET) folder.
        /// </summary>
        /// <remarks>
        /// A resource file can be specified by either type or an assembly name and a resource file name.
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <Window>
        /// ...
        ///     <Grid LocalizationScope.ResourceManager="{ResourceManager AssemblyName=MyAssembly, ResourceFile=MyAssembly.Resources.MyResources}">
        ///     ...
        ///     </Grid>
        /// ...
        /// </Window>
        /// ]]>
        /// </code>
        /// </example>
        public string ResourceFile { get; set; }

        public ResourceManagerExtension() { }

        public ResourceManagerExtension(Type type)
        {
            Type = type;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object,System.Object,System.Object)")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object,System.Object)")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object)")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "AssemblyName")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "ResourceFile")]
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }

            var type = Type;

            if (type == null)
            {
                if (string.IsNullOrEmpty(AssemblyName) || string.IsNullOrEmpty(ResourceFile))
                {
                    throw new InvalidOperationException($"Either {nameof(Type)} or {nameof(AssemblyName)} and {nameof(ResourceFile)} must be specified.");
                }
                else
                {
                    var assembly = AppDomain.CurrentDomain.Load(AssemblyName);
                    return ResourceManagerHelper.GetResourceManager(assembly, ResourceFile)
                        ?? throw new InvalidOperationException($"Resource file named '{ResourceFile}' was not found in '{AssemblyName}'. Make sure the full name of the resource file is specified (including namespace and file name).");
                }
            }
            else
            {
                return ResourceManagerHelper.GetResourceManager(type)
                    ?? throw new InvalidOperationException($"Resource file corresponding to type '{type.FullName}' was not found.");
            }
        }
    }
}
