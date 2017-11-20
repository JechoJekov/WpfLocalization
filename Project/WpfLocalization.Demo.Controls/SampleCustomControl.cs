using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfLocalization.Demo.Controls
{
    /// <summary>
    /// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
    ///
    /// Step 1a) Using this custom control in a XAML file that exists in the current project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:WpfLocalization.Demo.Controls"
    ///
    ///
    /// Step 1b) Using this custom control in a XAML file that exists in a different project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:WpfLocalization.Demo.Controls;assembly=WpfLocalization.Demo.Controls"
    ///
    /// You will also need to add a project reference from the project where the XAML file lives
    /// to this project and Rebuild to avoid compilation errors:
    ///
    ///     Right click on the target project in the Solution Explorer and
    ///     "Add Reference"->"Projects"->[Select this project]
    ///
    ///
    /// Step 2)
    /// Go ahead and use your control in the XAML file.
    ///
    ///     <MyNamespace:CustomControl1/>
    ///
    /// </summary>
    public class SampleCustomControl : Control
    {
        /// <summary>
        /// First name.
        /// </summary>
        public string FirstName
        {
            get { return (string)GetValue(FirstNameProperty); }
            set { SetValue(FirstNameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FirstName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FirstNameProperty = DependencyProperty.Register(
            "FirstName",
            typeof(string),
            typeof(SampleCustomControl),
            new FrameworkPropertyMetadata(
                string.Empty,
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender
                )
            );

        /// <summary>
        /// Last name.
        /// </summary>
        public string LastName
        {
            get { return (string)GetValue(LastNameProperty); }
            set { SetValue(LastNameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LastName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LastNameProperty = DependencyProperty.Register(
            "LastName",
            typeof(string),
            typeof(SampleCustomControl),
            new FrameworkPropertyMetadata(
                string.Empty,
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender
                )
            );

        static SampleCustomControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SampleCustomControl), new FrameworkPropertyMetadata(typeof(SampleCustomControl)));
        }

        public SampleCustomControl()
        {
            // Set the resources manager in the constructor
            LocalizationScope.SetResourceManager(this, Properties.Resources.ResourceManager);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            var labelWelcomeMessage = this.Template.FindName("labelWelcomeMessage", this) as TextBlock;

            // Localize the value in code-behind
            labelWelcomeMessage.Property(TextBlock.TextProperty).ResourceFormat(
                "Text_Welcome",
                new Binding(nameof(FirstName))
                {
                    RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent),
                },
                new Binding(nameof(LastName))
                {
                    RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent),
                });
        }
    }
}
