using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace WpfLocalization.Demo.Data
{
    public class StyleDemoData : INotifyPropertyChanged
    {
        double _number;

        public double Number
        {
            get
            {
                return _number;
            }
            set
            {
                _number = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Number)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsPositiveNumber)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsNegativeNumber)));
            }
        }

        public bool IsPositiveNumber
        {
            get
            {
                return Number > 0;
            }
        }

        public bool IsNegativeNumber
        {
            get
            {
                return Number < 0;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
