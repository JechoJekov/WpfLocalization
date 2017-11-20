using System;
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
        ///     <Grid LocalizationScope.ResourceManager={ResourceManager Type={x:Type properties:MyResources}}>
        ///     ...
        ///     </Grid>
        /// ...
        /// </Window>
        /// ]]>
        /// </code>
        /// </example>
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
        ///     <Grid LocalizationScope.ResourceManager={ResourceManager AssemblyName=MyAssembly, ResourcePath=MyAssembly.Resources.MyResources}>
        ///     ...
        ///     </Grid>
        /// ...
        /// </Window>
        /// ]]>
        /// </code>
        /// </example>
        public string AssemblyName { get; set; }

        /// <summary>
        /// The full name of the resource file including namespace and file name (e.g. "MyApplication.Properties.MyResources").
        /// </summary>
        /// <remarks>
        /// A resource file can be specified by either type or an assembly name and a resource file name.
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <Window>
        /// ...
        ///     <Grid LocalizationScope.ResourceManager={ResourceManager AssemblyName=MyAssembly, ResourceFile=MyAssembly.Resources.MyResources}>
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

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
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
                    type = assembly.GetType(ResourceFile, false);

                    if (type == null)
                    {
                        throw new InvalidOperationException($"Resource file named {ResourceFile} was not found in {AssemblyName}. Make sure the full name of the resource file is specified (including namespace and file name).");
                    }
                }
            }

            return ResourceManagerHelper.GetResourceManager(type);
        }
    }
}
