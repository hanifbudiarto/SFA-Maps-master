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
    /// Interaction logic for Window_SalesSelect.xaml
    /// </summary>
    public partial class Window_SalesSelect : Window
    {
        public event EventHandler<SalesPilihArgs> OnSalesPilihChanged;

        ObservableCollection<Model_Sales> list_daftar_sales = new ObservableCollection<Model_Sales>();
        public Window_SalesSelect()
        {
            InitializeComponent();

            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Owner = Application.Current.MainWindow;

            list_daftar.ItemsSource = list_daftar_sales;
        }

        private void btn_ok_Click(object sender, RoutedEventArgs e)
        {
            //ini buttonselect        
            Button button = sender as Button;
            Model_Sales item = button.DataContext as Model_Sales;
            Console.WriteLine("KLIK VIEW: " + item.KodeSales + " - " + item.NamaSales);

            if (OnSalesPilihChanged != null)
                OnSalesPilihChanged.Invoke(this, new SalesPilihArgs(item.KodeSales, item.NamaSales));


            Close();
        }

        private void btn_cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void list_daftar_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            list_daftar.SelectedItem = null;
        }

        private void textbox_sales_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                eksek_search_id();
            }
        }

        private void eksek_search_id()
        {
            string input = textbox_sales.Text;
            input = input.Trim();
            if (input.Equals("") == false)
            {
                if (input.Length > 2)
                {
                    TampilDaftarSearch(input);
                }
                else
                {
                   MessageBox.Show("You must enter min 3 character",
                                "Information",MessageBoxButton.OK,MessageBoxImage.Information);
                }
                textbox_sales.Text = "";
            }
        }

        private void TampilDaftarSearch(string input)
        {
            Model_Login m = new Model_Login();
            bool stat = false;

            bool ada = false;
            list_daftar_sales.Clear();

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

                        String sqlQuery = "select Kode,Nama from SalesPerson where username like '%"+input+"%' or nama like '%" + input + "%' or Kode like '%" + input + "%' and Aktif=1";
                        using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                        {
                            SqlDataReader reader = command.ExecuteReader();

                            while (reader.Read())
                            {

                                Model_Sales item = new Model_Sales();
                                item.KodeSales = reader.GetString(0);
                                item.NamaSales = reader.GetString(1);


                                App.Current.Dispatcher.Invoke((Action)delegate // <--- HERE
                                {
                                    list_daftar_sales.Add(item);
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
                        MessageBox.Show("Could not found \"" + input + "\"",
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
    }
}
