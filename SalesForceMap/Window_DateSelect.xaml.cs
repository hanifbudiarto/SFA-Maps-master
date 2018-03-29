using SalesForceMap.Argumen;
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
using System.Windows.Shapes;

namespace SalesForceMap
{
    /// <summary>
    /// Interaction logic for Window_DateSelect.xaml
    /// </summary>
    public partial class Window_DateSelect : Window
    {
        public event EventHandler<DatePilihArgs> OnDatePilihChanged;
        

        public Window_DateSelect()
        {
            InitializeComponent();

            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Owner = Application.Current.MainWindow;
        }

        string TglSekarang = "";
        string TglBesok = "";
        private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            var picker = sender as DatePicker;
            DateTime? date = picker.SelectedDate;

            if (date == null)
            {
                //this.Title = "No date";
            }
            else
            {
                //this.Title = date.Value.ToShortDateString();

                DateTime dt = date.Value;
                DateTime dt2 = dt.AddDays(1);
                string format = "yyyy-MM-dd";
                Console.WriteLine("dt1:" + dt.ToString(format));
                Console.WriteLine("dt2:" + dt2.ToString(format));

                TglSekarang = dt.ToString(format);
                TglBesok = dt2.ToString(format);


            }
        }

        private void btn_ok_Click(object sender, RoutedEventArgs e)
        {
            if (OnDatePilihChanged != null)
                OnDatePilihChanged.Invoke(this, new DatePilihArgs(TglSekarang, TglBesok));


            Close();
        }

        private void btn_cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
