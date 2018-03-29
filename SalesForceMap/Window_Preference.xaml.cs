using System;
using System.Collections.Generic;
using System.IO;
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
    /// Interaction logic for Window_Preference.xaml
    /// </summary>
    public partial class Window_Preference : Window
    {
        string lokasiFileINI = "";
        string namaFileINI = "SFA_Settings.ini";

        public Window_Preference()
        {
            InitializeComponent();

            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Owner = Application.Current.MainWindow;

            textbox_alamatip.Text = Setting_variabel.CONS_ALAMATIP;
            textbox_instance.Text = Setting_variabel.CONS_INSTANCE;
            textbox_dbname.Text = Setting_variabel.CONS_NAMADB;
        }

        private void SimpanFileINI(string lokasiFile)
        {
            try
            {
                //StreamWriter writetext = new StreamWriter(lokasiFile);

                StringBuilder strbuild = new StringBuilder();
                strbuild.Append("[SETTING]\r\n");
                strbuild.Append("[IPSERVER]=\"" + Setting_variabel.CONS_ALAMATIP + "\"\r\n");
                strbuild.Append("[INSTANCE]=\"" + Setting_variabel.CONS_INSTANCE + "\"\r\n");
                strbuild.Append("[DBNAME]=\"" + Setting_variabel.CONS_NAMADB + "\"\r\n");
                strbuild.Append("[ENDSETTING]\r\n");

               // writetext.WriteLine("" + strbuild.ToString());
                //writetext.Close();


                if (!File.Exists(lokasiFile))
                {
                    string createText = strbuild.ToString();
                    File.WriteAllText(lokasiFile, createText);
                }
                else
                {
                    //hapus
                    try
                    {
                        File.Delete(lokasiFile);
                    }
                    catch (Exception e) { }

                    //simpan
                    string createText = strbuild.ToString();
                    File.WriteAllText(lokasiFile, createText);
                }
            }
            catch(Exception e)
            {
                Console.WriteLine("" + e.Message + " " + e.StackTrace );
            }


            try
            {
                FileInfo fi = new FileInfo(lokasiFile);
                fi.Attributes = FileAttributes.Hidden;
            }
            catch (Exception e) { }

        }

        private void btn_ok_Click(object sender, RoutedEventArgs e)
        {
            Setting_variabel.CONS_ALAMATIP = textbox_alamatip.Text;
            Setting_variabel.CONS_INSTANCE = textbox_instance.Text;
            Setting_variabel.CONS_NAMADB = textbox_dbname.Text;

            lokasiFileINI = Setting_variabel.settingPath;// System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            lokasiFileINI = lokasiFileINI.Replace("file:\\", "");

            SimpanFileINI(lokasiFileINI + "\\" + namaFileINI);


            Close();
        }

        private void btn_cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
