using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
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
    /// <remarks>
    /// CAUTION This extension can only be used when setting <see cref="LocalizationScope.ResourceManager"/> attached property 
    /// on a <see cref="Window"/> or a <see cref="UserControl"/>. It cannot be used in the content of a window or a user control.
    /// </remarks>
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
        public string ResourceFile { get; set; }

        public LocalResourceManagerExtension() { }

        public LocalResourceManagerExtension(string resourceFile)
        {
            this.ResourceFile = resourceFile;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (!(serviceProvider.GetService(typeof(IProvideValueTarget)) is IProvideValueTarget service))
            {
                return null;
            }

            Assembly assembly;

            if (service.TargetObject is DependencyObject depObj && DesignerProperties.GetIsInDesignMode(depObj))
            {
                // WPF Designer uses an instance of "System.Windows.Controls.UserControl" to represent a user control
                // (not the actual type) and "Microsoft.VisualStudio.DesignTools.WpfDesigner.InstanceBuilders.WindowInstance"
                // to represent a mock window

                if (depObj is UserControl || (depObj is FrameworkElement frameworkElement && frameworkElement.Parent == null))
                {
                    // The element is a user control or a root element (e.g. a mock-up window - WindowInstance)

                    // Check if the object is a mock object or an actual instance. If it is an actual instance then
                    // the user control is used in another user control or a window
                    if (depObj.GetType() == typeof(UserControl) || DesignTimeHelper.IsWpfDesignerAssembly(depObj.GetType().Assembly))
                    {
                        // The object is a mock object; therefore, use the design-time assembly 
                        assembly = DesignTimeHelper.GetDesignTimeAssembly();
                    }
                    else
                    {
                        // The object is an actual instance
                        assembly = depObj.GetType().Assembly;
                    }
                }
                else
                {
                    throw new NotSupportedException("The 'LocalResourceManager' extension can be used only on <Window> and <UserControl> elements.");
                }
            }
            else if (service.TargetObject is Window || service.TargetObject is UserControl)
            {
                assembly = service.TargetObject.GetType().Assembly;
            }
            else
            {
                throw new NotSupportedException("The 'LocalResourceManager' extension can be used only on <Window> and <UserControl> elements.");
            }

            if (string.IsNullOrEmpty(ResourceFile))
            {
                return ResourceManagerHelper.GetDefaultResourceManagerForAssembly(assembly)
                    ?? throw new InvalidOperationException($"Default resource file was not found in '{assembly.GetName().Name}'.");
            }
            else
            {
                return ResourceManagerHelper.GetResourceManager(assembly, ResourceFile) 
                    ?? throw new InvalidOperationException($"Resource file named '{ResourceFile}' was not found in '{assembly.GetName().Name}'.");
            }
        }
    }
}
