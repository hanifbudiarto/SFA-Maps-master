using SalesForceMap.Argumen;
using SalesForceMap.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SqlClient;
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
    /// Interaction logic for Window_KodenotaSelect.xaml
    /// </summary>
    public partial class Window_KodenotaSelect : Window
    {
        public event EventHandler<KodenotaPilihArgs> OnKodenotaPilihChanged;

        ObservableCollection<Model_Kodenota> list_daftar_kodenota = new ObservableCollection<Model_Kodenota>();
        string TGLSEKARANG = "";
        string TGLBESOK = "";
        string KODESALES = "";
        public Window_KodenotaSelect(string tglsekarang,string tglbesok,string kodesales)
        {
            InitializeComponent();

            this.TGLSEKARANG = tglsekarang;
            this.TGLBESOK = tglbesok;
            this.KODESALES = kodesales;

            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Owner = Application.Current.MainWindow;

            list_daftar.ItemsSource = list_daftar_kodenota;


            if(TGLSEKARANG.Equals("")==false && TGLBESOK.Equals("")==false && KODESALES.Equals("") == false)
            {
                TampilDaftarKodenota();
            }else
            {
                MessageBoxResult result= MessageBox.Show("Could not process your request. Make sure you have selected a sales and a travel date", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                if (result == MessageBoxResult.OK)
                {
                    Close();
                }
            }
        }

        private void TampilDaftarKodenota()
        {
            Model_Login m = new Model_Login();
            bool stat = false;

            bool ada = false;
            list_daftar_kodenota.Clear();

            grid_progress.Visibility = Visibility.Visible;

            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.ProgressChanged += delegate
            {

            };
            worker.DoWork += delegate
            {
                try
                {
                    using (SqlConnection connection = new SqlConnection(Setting_variabel.mykoneksi_sql))
                    {
                        connection.Open();

                        String sqlQuery = "select m.KodeNota,m.TglBerangkat,m.isClose,m.SudahKembali, " + 
                                "(select COUNT(1) from DetailPerjalanan dp join " + 
                                "(select kode = min(kode) from pelanggan " + 
                                "group by cif) p on dp.cust = p.kode where dp.kodenota = m.kodenota and " + 
                                "(dp.Checkin is not null and dp.Checkin > '2000-01-01')) as JmlTerkunjungi, " + 
                                "(select COUNT(1) from DetailPerjalanan dp join " + 
                                "(select kode = min(kode) from pelanggan " + 
                                "group by cif) p on dp.cust = p.kode where dp.kodenota = m.kodenota) as JmlKunjungan " + 
                                "from masterperjalanan m " + 
                                "where m.tglberangkat >= '" + TGLSEKARANG + "' and " + 
                                "m.tglberangkat < '" + TGLBESOK + "' and " + 
                                "m.Sales = '" + KODESALES + "' order by m.TglBerangkat desc";

                        using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                        {
                            SqlDataReader reader = command.ExecuteReader();

                            while (reader.Read())
                            {

                                Model_Kodenota item = new Model_Kodenota();
                                item.Kodenota = reader.GetString(0);
                                try
                                {
                                    item.TglBerangkat = "" + reader.GetDateTime(1);
                                }
                                catch (Exception e) { }

                                try
                                {
                                    item.IsClose = reader.GetBoolean(2);
                                }
                                catch (Exception e) { }

                                try
                                {
                                    item.IsSudahKembali = reader.GetBoolean(3);
                                }
                                catch (Exception e) { }

                                item.JmlTerkunjungi = reader.GetInt32(4);
                                item.JmlKunjungan=reader.GetInt32(5);


                                App.Current.Dispatcher.Invoke((Action)delegate // <--- HERE
                                {
                                    list_daftar_kodenota.Add(item);
                                });


                                ada = true;
                            }

                            stat = true;
                            reader.Close();
                        }

                        m.Keterangan = "Unable to process your request";

                        connection.Close();
                    }
                }
                catch (Exception e)
                {
                    m.Keterangan = "Unable to process your request: " + e.Message;
                    Console.WriteLine("" + e.Message + " " + e.StackTrace);
                }
            };
            //worker.ProgressChanged += worker_ProgressChanged;
            worker.RunWorkerCompleted += delegate
            {
                grid_progress.Visibility = Visibility.Collapsed;

                m.IsSukses = stat;

                if (m.IsSukses == true)
                {
                    if (ada == false)
                    {
                        MessageBox.Show("Could not found travel code for sales \"" + KODESALES + "\" at "+TGLSEKARANG+" ",
                                        "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else
                {
                    MessageBox.Show("" + m.Keterangan, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }

            };
            worker.RunWorkerAsync();


        }

        private void list_daftar_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            list_daftar.SelectedItem = null;
        }

        private void btn_cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btn_ok_Click(object sender, RoutedEventArgs e)
        {
            //ini buttonselect        
            Button button = sender as Button;
            Model_Kodenota item = button.DataContext as Model_Kodenota;
            Console.WriteLine("KLIK VIEW: " + item.Kodenota );

            if (OnKodenotaPilihChanged != null)
                OnKodenotaPilihChanged.Invoke(this, new KodenotaPilihArgs(item.Kodenota,item.IsKeterangan,item.JmlTerkunjungi,item.JmlKunjungan));
            Close();
        }
    }
}
