using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace WpfLocalization
{
    /// <summary>
    /// Loads resources located in the same assembly as the <see cref="Window"/> or <see cref="UserControl"/> on which the extension is used.
    /// </summary>
    /// <example>
    /// This example uses the default resource file of the assembly in which the user control is located.
    /// <code>
    /// <![CDATA[
    /// <UserControl ... LocalizationManager.ResourceManager="{LocalResourceManager}" ...>
    /// ...
    /// </UserControl>
    /// ]]>
    /// </code>
    /// </example>
    /// <example>
    /// <code>
    /// This example uses the resource file named "AltResources" in the assembly in which the user control is located.
    /// <![CDATA[
    /// <UserControl ... LocalizationManager.ResourceManager="{LocalResourceManager AltResources}" ...>
    /// ...
    /// </UserControl>
    /// ]]>
    /// </code>
    /// </example>
    [MarkupExtensionReturnType(typeof(ResourceManager))]
    public class LocalResourceManagerExtension : MarkupExtension
    {
        public string Name { get; set; }

        public LocalResourceManagerExtension() { }

        public LocalResourceManagerExtension(string name)
        {
            this.Name = name;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (!(serviceProvider.GetService(typeof(IProvideValueTarget)) is IProvideValueTarget service))
            {
                return null;
            }

            var assembly = service.TargetObject.GetType().Assembly;

            ResourceManager resourceManager;

            if (string.IsNullOrEmpty(Name))
            {
                return ResourceManagerHelper.GetDefaultResourceManagerForAssembly(assembly);
            }
            else
            {
                Type type;

                if (Name.IndexOf('.') < 0)
                {
                    // Partial name
                    type = assembly.GetType(assembly.GetName().Name + ".Properties." + Name, false) // C#
                       ?? assembly.GetType(assembly.GetName().Name + "." + Name, false); // VB.NET
                }
                else
                {
                    // Full name
                    type = assembly.GetType(Name, false);
                }

                resourceManager = type == null ? null : ResourceManagerHelper.GetResourceManager(type);
            }

            return resourceManager;
        }
    }
}
