using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace SalesForceMap.Model
{
    class Model_Sales : INotifyPropertyChanged
    {
        public string KodeSales { get; set; }
        public string NamaSales { get; set; }

        public bool _IsVisible;
        public bool IsVisible {
            get
            {
                return _IsVisible;
            }
            set
            {
                if (_IsVisible != value)
                {
                    _IsVisible = value;
                    OnPropertyChanged("IsVisible");
                }
            }
        }

        public double _Latitude;
        public double Latitude
        {
            get
            {
                return _Latitude;
            }
            set
            {
                if (_Latitude != value)
                {
                    _Latitude = value;
                    OnPropertyChanged("Latitude");
                }
            }
        }

        public double _Longitude;
        public double Longitude
        {
            get
            {
                return _Longitude;
            }
            set
            {
                if (_Longitude != value)
                {
                    _Longitude = value;
                    OnPropertyChanged("Longitude");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
