using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace SalesForceMap.Model
{
    class Model_Kodenota : INotifyPropertyChanged
    {
        public string Kodenota { get; set; }
        public string TglBerangkat { get; set; }
        public bool IsClose { get; set; }
        public bool IsSudahKembali { get; set; }
        public int JmlTerkunjungi { get; set; }
        public int JmlKunjungan { get; set; }

        public string IsKeterangan
        {
            get
            {
                if(IsClose==true || IsSudahKembali == true)
                {
                    return "Travel is closed";
                }else
                {
                    return "Travel in progress";
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
