using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace SalesForceMap.Model
{
    class Model_Perjalanan : INotifyPropertyChanged
    {
        public string Kodenota { get; set; }
        public string KodePelanggan { get; set; }
        public string NamaPelanggan { get; set; }
        public DateTime Checkin { get; set; }
        public DateTime Checkout { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }


        public System.Windows.Visibility IsVisibleKunjungan
        {
            get
            {

                if (Index == 0)
                {
                    return System.Windows.Visibility.Collapsed;
                }
                else
                {
                    return System.Windows.Visibility.Visible;
                }
           
            }
            set
            {
                if (IsVisibleKunjungan != value)
                {
                    IsVisibleKunjungan = value;
                    OnPropertyChanged("IsVisibleKunjungan");
                    OnPropertyChanged("Keterangan");
                    OnPropertyChanged("Index");
                    OnPropertyChanged("ColorStatus");
                    OnPropertyChanged("Kunjunganke");
                    OnPropertyChanged("StrCheckin");
                }
            }
        }

        public string _StrCheckin;
        public string StrCheckin
        {
            get
            {
                if (Index == 0)
                {
                    return "n/a";
                }
                else
                {
                    if (Checkin != null)
                    {
                        return Checkin.ToString("HH:mm");
                    }
                    else
                    {
                        return "n/a";
                    }
                }
            }

            set
            {
                if (_StrCheckin != value)
                {
                    _StrCheckin = value;
                    OnPropertyChanged("Keterangan");
                    OnPropertyChanged("Index");
                    OnPropertyChanged("ColorStatus");
                    OnPropertyChanged("IsVisibleKunjungan");
                    OnPropertyChanged("Kunjunganke");
                    OnPropertyChanged("StrCheckin");
                }
            }
        }


        public string _Kunjunganke;
        public string Kunjunganke
        {
            get
            {
                if (Index == 0)
                {
                    return "";
                }
                else
                {
                    return "Kunjungan ke-"+Index;
                }
            }
            set
            {
                if (_Kunjunganke != value)
                {
                    _Kunjunganke = value;
                    OnPropertyChanged("Keterangan");
                    OnPropertyChanged("Index");
                    OnPropertyChanged("ColorStatus");
                    OnPropertyChanged("IsVisibleKunjungan");
                    OnPropertyChanged("Kunjunganke");
                    OnPropertyChanged("StrCheckin");
                }
            }

        }
        public string _ColorStatus;
        public string ColorStatus
        {
            get
            {
                if (Index == 0)
                {
                    return "#FF888888";
                }else
                {
                    return "#FF45A470";
                }
            }
            set
            {
                if (_ColorStatus != value)
                {
                    _ColorStatus = value;
                    OnPropertyChanged("Keterangan");
                    OnPropertyChanged("Index");
                    OnPropertyChanged("ColorStatus");
                    OnPropertyChanged("IsVisibleKunjungan");
                    OnPropertyChanged("Kunjunganke");
                    OnPropertyChanged("StrCheckin");
                }
            }
        }

        public int _Index = 0;
        public int Index {
            get
            {
                return _Index;
            }
            set
            {
                if (_Index != value)
                {
                    _Index = value;
                    OnPropertyChanged("Keterangan");
                    OnPropertyChanged("Index");
                    OnPropertyChanged("ColorStatus");
                    OnPropertyChanged("IsVisibleKunjungan");
                    OnPropertyChanged("Kunjunganke");
                    OnPropertyChanged("StrCheckin");
                }
            }
        }

        public string _Keterangan = "";
        public string Keterangan
        {
            get
            {
                return _Keterangan;
            }
            set
            {
                if (_Keterangan != value)
                {
                    _Keterangan = value;
                    OnPropertyChanged("Keterangan");
                    OnPropertyChanged("Index");
                    OnPropertyChanged("ColorStatus");
                    OnPropertyChanged("IsVisibleKunjungan");
                    OnPropertyChanged("Kunjunganke");
                    OnPropertyChanged("StrCheckin");
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
