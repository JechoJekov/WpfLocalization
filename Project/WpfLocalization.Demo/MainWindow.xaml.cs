using System;
using System.Collections.Generic;
using System.Globalization;
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

namespace WpfLocalization.Demo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        object BirthDateInfoCallback(CultureInfo culture, CultureInfo uiCulture, object parameter, object dataBindingValue)
        {
            // Calculate the number of days left until the user's next birthday

            var birthDate = (DateTime)dataBindingValue;
            var now = DateTime.Now;
            birthDate = new DateTime(now.Year, birthDate.Month, birthDate.Day);
            if (birthDate < now)
            {
                birthDate = birthDate.AddYears(1);
            }
            return (int)(birthDate - now).TotalDays;
        }
    }
}
