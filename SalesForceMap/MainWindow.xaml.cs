using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsPresentation;
using System.Globalization;
using System.IO;
using SalesForceMap.Marker;
using System.Windows.Threading;
using System.Threading;
using System.Diagnostics;
using System.ComponentModel;
using System.Net.NetworkInformation;
using System.Data.SqlClient;
using SalesForceMap.Model;
using static SalesForceMap.Window_DateSelect;
using SalesForceMap.Argumen;
using System.Collections.ObjectModel;
using System.Windows.Media.Animation;
using System.Text.RegularExpressions;
using System.Timers;

namespace SalesForceMap
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string lokasiFileINI = "";
        string namaFileINI = "SFA_Settings.ini";

        //select * from masterperjalanan where tglberangkat>='2014-12-10' and tglberangkat<'2014-12-11'
        //select dp.kodenota,dp.kilometer,dp.checkin,dp.checkout,case when (CAST(dp.Latitude as float)=0 or dp.Latitude is null or dp.Latitude='') then p.Latitude else dp.Latitude end as Latitude,case when (CAST(dp.Longitude as float)=0 or dp.Longitude is null or dp.Longitude='') then p.Longitude else dp.Longitude end as Longitude from DetailPerjalanan dp, pelanggan p where kodenota='00004/01/NJ/1604/AE00001' and dp.cust=p.kode

        List<Window_DateSelect> list_date_select = new List<Window_DateSelect>();
        List<Window_SalesSelect> list_sales_select = new List<Window_SalesSelect>();
        List<Window_KodenotaSelect> list_kodenota_select = new List<Window_KodenotaSelect>();
        List<Window_Preference> list_preference = new List<Window_Preference>();

        PointLatLng start;
        PointLatLng end;

        // marker
        GMapMarker currentMarker;

        // zones list
        List<GMapMarker> Circles = new List<GMapMarker>();

        ObservableCollection<Model_Perjalanan> list_daftar_perjalanan = new ObservableCollection<Model_Perjalanan>();
        ObservableCollection<Model_Sales> list_daftar_sales = new ObservableCollection<Model_Sales>();



        private void bacaFileINI(string lokasiFile)
        {
            var lines = File.ReadLines(lokasiFile);
            StringBuilder strb = new StringBuilder();
            foreach (var line in lines)
            {
                Console.WriteLine("" + line);
                strb.Append(line);
            }

            string data = strb.ToString();

            bool sesuaiFormat = false;
            try
            {
                sesuaiFormat = Regex.IsMatch(data, "[\\w\\W\\s]*[\\[]SETTING[\\]][\\w\\W\\s]*[\\[]ENDSETTING[\\]][\\w\\W\\s]*");
                if (sesuaiFormat)
                {
                    try
                    {
                        Regex RegexObj = new Regex("[\\w\\W\\s]*[\\[]SETTING[\\]][\\w\\W\\s]*[\\[]IPSERVER[\\]][\\s]*[\\=][\\s]*[\\\"]([\\w\\W\\s]*)[\\\"][\\w\\W\\s]*[\\[]INSTANCE[\\]][\\s]*[\\=][\\s]*[\\\"]([\\w\\W\\s]*)[\\\"][\\w\\W\\s]*[\\[]DBNAME[\\]][\\s]*[\\=][\\s]*[\\\"]([\\w\\W\\s]*)[\\\"][\\w\\W\\s]*[\\[]ENDSETTING[\\]][\\w\\W\\s]*");
                        string ipserver = RegexObj.Match(data).Groups[1].Value;
                        string instance = RegexObj.Match(data).Groups[2].Value;
                        string namadb = RegexObj.Match(data).Groups[3].Value;
                        Setting_variabel.CONS_ALAMATIP = ipserver;
                        Setting_variabel.CONS_INSTANCE = instance;
                        Setting_variabel.CONS_NAMADB = namadb;
                    }
                    catch (ArgumentException ex)
                    {
                    }
                }
            }
            catch (ArgumentException ex)
            {
            }
        }

        private void SimpanFileINI(string lokasiFile)
        {
            StreamWriter writetext = new StreamWriter(lokasiFile);

            StringBuilder strbuild = new StringBuilder();
            strbuild.Append("[SETTING]\r\n");
            strbuild.Append("[IPSERVER]=\"" + Setting_variabel.CONS_ALAMATIP + "\"\r\n");
            strbuild.Append("[INSTANCE]=\"" + Setting_variabel.CONS_INSTANCE + "\"\r\n");
            strbuild.Append("[DBNAME]=\"" + Setting_variabel.CONS_NAMADB + "\"\r\n");
            strbuild.Append("[ENDSETTING]\r\n");

            writetext.WriteLine("" + strbuild.ToString());
            writetext.Close();


            try
            {
                FileInfo fi = new FileInfo(lokasiFile);
                fi.Attributes = FileAttributes.Hidden;
            }
            catch (Exception e) { }
        }

        System.Timers.Timer timer_refresh;
        public MainWindow()
        {
            InitializeComponent();
            InisialisasiMap();


            /////
            lokasiFileINI = Setting_variabel.settingPath;// System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            lokasiFileINI = lokasiFileINI.Replace("file:\\", "");

            Console.WriteLine(lokasiFileINI);

            //cek exists
            if (File.Exists(lokasiFileINI + "\\" + namaFileINI))
            {
                Console.WriteLine("file exists");
                bacaFileINI(lokasiFileINI + "\\" + namaFileINI);
            }
            else
            {
                Console.WriteLine("file not exists");
                SimpanFileINI(lokasiFileINI + "\\" + namaFileINI);
                bacaFileINI(lokasiFileINI + "\\" + namaFileINI);
            }

            /////

            list_daftar.ItemsSource = list_daftar_perjalanan;
            //listview_daftar_sales.ItemsSource = list_daftar_sales;


            checkbox_sales.IsChecked = Setting_variabel.IS_VISIBLE_SALES_MARKER;
            checkbox_cust.IsChecked = Setting_variabel.IS_VISIBLE_CUST_MARKER;


            //ProsesAmbilGPSSales(TipeTampilSales.All);

            timer_refresh = new System.Timers.Timer(1*60000);
            timer_refresh.Elapsed += new ElapsedEventHandler(_timer_refresh_Elapsed);
            timer_refresh.Enabled = true;
        }

        void _timer_refresh_Elapsed(object sender, ElapsedEventArgs e)
        {
            App.Current.Dispatcher.Invoke((Action)delegate // <--- HERE
            {
                Console.WriteLine("ProsesAmbilGPSSales(TIPETAMPILSALES_TMP)");
                ProsesAmbilGPSSales(TIPETAMPILSALES_TMP);
            });
          
        }

        private void Login()
        {
            string username = textbox_email.Text.Trim();
            string password = textbox_password.Password;

            if (username.Equals("") == false && password.Equals("") == false)
            {
                CekLogin(username, password);

            }
            else
            {
                MessageBox.Show("You must enter username and pasword", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void button_signin_Click(object sender, RoutedEventArgs e)
        {
            Login();
        }

        private void textbox_password_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Login();
            }
        }

        private void textbox_email_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Login();
            }
        }

        private void textbox_password_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(textbox_password.Password))
            {
                textbox_password.Tag = "";
            }
            else
            {
                textbox_password.Tag = "Password";
            }
            //textbox_password.Tag = (!string.IsNullOrEmpty(textbox_password.Password)).ToString();
        }

        public static bool PingNetwork(string hostNameOrAddress)
        {
            bool pingStatus = false;

            using (Ping p = new Ping())
            {
                byte[] buffer = Encoding.ASCII.GetBytes("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
                int timeout = 4444; // 4s

                try
                {
                    PingReply reply = p.Send(hostNameOrAddress, timeout, buffer);
                    pingStatus = (reply.Status == IPStatus.Success);
                }
                catch (Exception)
                {
                    pingStatus = false;
                }
            }

            return pingStatus;
        }

        private void InisialisasiMap()
        {
            // set cache mode only if no internet avaible
            if (!PingNetwork("pingtest.com"))
            {
                MainMap.Manager.Mode = AccessMode.CacheOnly;
                MessageBox.Show("No internet connection available, going to CacheOnly mode.", "SFA Tracking", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            // config map
            MainMap.MapProvider = GMapProviders.OpenStreetMap;
            MainMap.Position = new PointLatLng(-7.2721965, 112.7425929);

            // MainMap.ScaleMode = ScaleModes.Dynamic;

            MainMap.Zoom = 6;

            // map events
            MainMap.OnPositionChanged += new PositionChanged(MainMap_OnCurrentPositionChanged);
            MainMap.OnTileLoadComplete += new TileLoadComplete(MainMap_OnTileLoadComplete);
            MainMap.OnTileLoadStart += new TileLoadStart(MainMap_OnTileLoadStart);
            MainMap.OnMapTypeChanged += new MapTypeChanged(MainMap_OnMapTypeChanged);
            MainMap.MouseMove += new System.Windows.Input.MouseEventHandler(MainMap_MouseMove);
            MainMap.MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(MainMap_MouseLeftButtonDown);
            MainMap.MouseEnter += new MouseEventHandler(MainMap_MouseEnter);

            // get map types
            comboBoxMapType.ItemsSource = GMapProviders.List;
            comboBoxMapType.DisplayMemberPath = "Name";
            comboBoxMapType.SelectedItem = MainMap.MapProvider;

            // acccess mode
            comboBoxMode.ItemsSource = Enum.GetValues(typeof(AccessMode));
            comboBoxMode.SelectedItem = MainMap.Manager.Mode;

            // get cache modes
            checkBoxCacheRoute.IsChecked = MainMap.Manager.UseRouteCache;
            checkBoxGeoCache.IsChecked = MainMap.Manager.UseGeocoderCache;

            // setup zoom min/max
            sliderZoom.Maximum = MainMap.MaxZoom;
            sliderZoom.Minimum = MainMap.MinZoom;

            // get position
            textBoxLat.Text = MainMap.Position.Lat.ToString(CultureInfo.InvariantCulture);
            textBoxLng.Text = MainMap.Position.Lng.ToString(CultureInfo.InvariantCulture);

            // get marker state
            checkBoxCurrentMarker.IsChecked = true;

            // can drag map
            checkBoxDragMap.IsChecked = MainMap.CanDragMap;

#if DEBUG
            checkBoxDebug.IsChecked = true;
#endif

            //validator.Window = this;

            // set current marker
            currentMarker = new GMapMarker(MainMap.Position);
            {
                currentMarker.Shape = new CustomMarkerRed(this, currentMarker, "custom position marker");
                currentMarker.Offset = new System.Windows.Point(-15, -15);
                currentMarker.ZIndex = int.MaxValue;
                //MainMap.Markers.Add(currentMarker);
            }

            MainMap.ShowTileGridLines = false;
            MainMap.CanDragMap = true;

            if (false)
            {
                // add my city location for demo
                GeoCoderStatusCode status = GeoCoderStatusCode.Unknow;

                PointLatLng? city = GMapProviders.GoogleMap.GetPoint("Lithuania, Vilnius", out status);
                if (city != null && status == GeoCoderStatusCode.G_GEO_SUCCESS)
                {
                    GMapMarker it = new GMapMarker(city.Value);
                    {
                        it.ZIndex = 55;
                        it.Shape = new CustomMarkerDemo(this, it, "Welcome to Lithuania! ;}");
                    }
                    MainMap.Markers.Add(it);

                    #region -- add some markers and zone around them --
                    //if(false)
                    {
                        List<PointAndInfo> objects = new List<PointAndInfo>();
                        {
                            string area = "Antakalnis";
                            PointLatLng? pos = GMapProviders.GoogleMap.GetPoint("Lithuania, Vilnius, " + area, out status);
                            if (pos != null && status == GeoCoderStatusCode.G_GEO_SUCCESS)
                            {
                                objects.Add(new PointAndInfo(pos.Value, area));
                            }
                        }
                        {
                            string area = "Senamiestis";
                            PointLatLng? pos = GMapProviders.GoogleMap.GetPoint("Lithuania, Vilnius, " + area, out status);
                            if (pos != null && status == GeoCoderStatusCode.G_GEO_SUCCESS)
                            {
                                objects.Add(new PointAndInfo(pos.Value, area));
                            }
                        }
                        {
                            string area = "Pilaite";
                            PointLatLng? pos = GMapProviders.GoogleMap.GetPoint("Lithuania, Vilnius, " + area, out status);
                            if (pos != null && status == GeoCoderStatusCode.G_GEO_SUCCESS)
                            {
                                objects.Add(new PointAndInfo(pos.Value, area));
                            }
                        }
                        AddDemoZone(8.8, city.Value, objects);
                    }
                    #endregion
                }

                if (MainMap.Markers.Count > 1)
                {
                    MainMap.ZoomAndCenterMarkers(null);
                }
            }

            // perfromance test
            timer.Interval = TimeSpan.FromMilliseconds(44);
            timer.Tick += new EventHandler(timer_Tick);

        }


        void MainMap_MouseEnter(object sender, MouseEventArgs e)
        {
            MainMap.Focus();
        }

        #region -- performance test--
        public RenderTargetBitmap ToImageSource(FrameworkElement obj)
        {
            // Save current canvas transform
            Transform transform = obj.LayoutTransform;
            obj.LayoutTransform = null;

            // fix margin offset as well
            Thickness margin = obj.Margin;
            obj.Margin = new Thickness(0, 0, margin.Right - margin.Left, margin.Bottom - margin.Top);

            // Get the size of canvas
            System.Windows.Size size = new System.Windows.Size(obj.Width, obj.Height);

            // force control to Update
            obj.Measure(size);
            obj.Arrange(new Rect(size));

            RenderTargetBitmap bmp = new RenderTargetBitmap((int)size.Width, (int)size.Height, 96, 96, PixelFormats.Pbgra32);
            bmp.Render(obj);

            if (bmp.CanFreeze)
            {
                bmp.Freeze();
            }

            // return values as they were before
            obj.LayoutTransform = transform;
            obj.Margin = margin;

            return bmp;
        }

        double NextDouble(Random rng, double min, double max)
        {
            return min + (rng.NextDouble() * (max - min));
        }

        Random r = new Random();

        int tt = 0;
        void timer_Tick(object sender, EventArgs e)
        {
            var pos = new PointLatLng(NextDouble(r, MainMap.ViewArea.Top, MainMap.ViewArea.Bottom), NextDouble(r, MainMap.ViewArea.Left, MainMap.ViewArea.Right));
            GMapMarker m = new GMapMarker(pos);
            {
                var s = new Test((tt++).ToString());

                var image = new Image();
                {
                    RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.LowQuality);
                    image.Stretch = Stretch.None;
                    image.Opacity = s.Opacity;

                    image.MouseEnter += new System.Windows.Input.MouseEventHandler(image_MouseEnter);
                    image.MouseLeave += new System.Windows.Input.MouseEventHandler(image_MouseLeave);

                    image.Source = ToImageSource(s);
                }

                m.Shape = image;

                m.Offset = new System.Windows.Point(-s.Width, -s.Height);
            }
            MainMap.Markers.Add(m);

            if (tt >= 333)
            {
                timer.Stop();
                tt = 0;
            }
        }

        void image_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Image img = sender as Image;
            img.RenderTransform = null;
        }

        void image_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Image img = sender as Image;
            img.RenderTransform = new ScaleTransform(1.2, 1.2, 12.5, 12.5);
        }

        DispatcherTimer timer = new DispatcherTimer();
        #endregion



        // add objects and zone around them
        void AddDemoZone(double areaRadius, PointLatLng center, List<PointAndInfo> objects)
        {
            var objectsInArea = from p in objects
                                where MainMap.MapProvider.Projection.GetDistance(center, p.Point) <= areaRadius
                                select new
                                {
                                    Obj = p,
                                    Dist = MainMap.MapProvider.Projection.GetDistance(center, p.Point)
                                };
            if (objectsInArea.Any())
            {
                var maxDistObject = (from p in objectsInArea
                                     orderby p.Dist descending
                                     select p).First();

                // add objects to zone
                foreach (var o in objectsInArea)
                {
                    GMapMarker it = new GMapMarker(o.Obj.Point);
                    {
                        it.ZIndex = 55;
                        var s = new CustomMarkerDemo(this, it, o.Obj.Info + ", distance from center: " + o.Dist + "km.");
                        it.Shape = s;
                    }

                    MainMap.Markers.Add(it);
                }

                // add zone circle
                //if(false)
                {
                    GMapMarker it = new GMapMarker(center);
                    it.ZIndex = -1;

                    Circle c = new Circle();
                    c.Center = center;
                    c.Bound = maxDistObject.Obj.Point;
                    c.Tag = it;
                    c.IsHitTestVisible = false;

                    UpdateCircle(c);
                    Circles.Add(it);

                    it.Shape = c;
                    MainMap.Markers.Add(it);
                }
            }
        }

        // calculates circle radius
        void UpdateCircle(Circle c)
        {
            var pxCenter = MainMap.FromLatLngToLocal(c.Center);
            var pxBounds = MainMap.FromLatLngToLocal(c.Bound);

            double a = (double)(pxBounds.X - pxCenter.X);
            double b = (double)(pxBounds.Y - pxCenter.Y);
            var pxCircleRadius = Math.Sqrt(a * a + b * b);

            c.Width = 55 + pxCircleRadius * 2;
            c.Height = 55 + pxCircleRadius * 2;
            (c.Tag as GMapMarker).Offset = new System.Windows.Point(-c.Width / 2, -c.Height / 2);
        }

        void MainMap_OnMapTypeChanged(GMapProvider type)
        {
            sliderZoom.Minimum = MainMap.MinZoom;
            sliderZoom.Maximum = MainMap.MaxZoom;
        }

        void MainMap_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            System.Windows.Point p = e.GetPosition(MainMap);
            currentMarker.Position = MainMap.FromLocalToLatLng((int)p.X, (int)p.Y);
        }

        // move current marker with left holding
        void MainMap_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                System.Windows.Point p = e.GetPosition(MainMap);
                currentMarker.Position = MainMap.FromLocalToLatLng((int)p.X, (int)p.Y);
            }
        }

        // zoo max & center markers
        private void button13_Click(object sender, RoutedEventArgs e)
        {
            MainMap.ZoomAndCenterMarkers(null);

            /*
            PointAnimation panMap = new PointAnimation();
            panMap.Duration = TimeSpan.FromSeconds(1);
            panMap.From = new Point(MainMap.Position.Lat, MainMap.Position.Lng);
            panMap.To = new Point(0, 0);
            Storyboard.SetTarget(panMap, MainMap);
            Storyboard.SetTargetProperty(panMap, new PropertyPath(GMapControl.MapPointProperty));

            Storyboard panMapStoryBoard = new Storyboard();
            panMapStoryBoard.Children.Add(panMap);
            panMapStoryBoard.Begin(this);
             */
        }

        // tile louading starts
        void MainMap_OnTileLoadStart()
        {


            Dispatcher.BeginInvoke(new Action(delegate
            {
                // Do your work
                progressBar1.Visibility = Visibility.Visible;
            }));


            //System.Windows.Forms.MethodInvoker m = delegate ()
            //{
            //    progressBar1.Visibility = Visibility.Visible;
            //};

            //try
            //{
            //    this.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, m);
            //}
            //catch
            //{
            //}

        }

        // tile loading stops
        void MainMap_OnTileLoadComplete(long ElapsedMilliseconds)
        {
            MainMap.ElapsedMilliseconds = ElapsedMilliseconds;

            Dispatcher.BeginInvoke(new Action(delegate
            {
                // Do your work
                progressBar1.Visibility = Visibility.Hidden;
                groupBox3.Header = "loading, last in " + MainMap.ElapsedMilliseconds + "ms";
            }));

            //System.Windows.Forms.MethodInvoker m = delegate ()
            //{
            //    progressBar1.Visibility = Visibility.Hidden;
            //    groupBox3.Header = "loading, last in " + MainMap.ElapsedMilliseconds + "ms";
            //};

            //try
            //{
            //    this.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, m);
            //}
            //catch
            //{
            //}
        }

        // current location changed
        void MainMap_OnCurrentPositionChanged(PointLatLng point)
        {
            //mapgroup.Header = "gmap: " + point;
        }

        // reload
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            MainMap.ReloadMap();
        }

        // enable current marker
        private void checkBoxCurrentMarker_Checked(object sender, RoutedEventArgs e)
        {
            if (currentMarker != null)
            {
                MainMap.Markers.Add(currentMarker);
            }
        }

        // disable current marker
        private void checkBoxCurrentMarker_Unchecked(object sender, RoutedEventArgs e)
        {
            if (currentMarker != null)
            {
                MainMap.Markers.Remove(currentMarker);
            }
        }

        // enable map dragging
        private void checkBoxDragMap_Checked(object sender, RoutedEventArgs e)
        {
            MainMap.CanDragMap = true;
        }

        // disable map dragging
        private void checkBoxDragMap_Unchecked(object sender, RoutedEventArgs e)
        {
            MainMap.CanDragMap = false;
        }

        // goto!
        private void button2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                double lat = double.Parse(textBoxLat.Text, CultureInfo.InvariantCulture);
                double lng = double.Parse(textBoxLng.Text, CultureInfo.InvariantCulture);

                MainMap.Position = new PointLatLng(lat, lng);
            }
            catch (Exception ex)
            {
                MessageBox.Show("incorrect coordinate format: " + ex.Message);
            }
        }

        // goto by geocoder
        private void textBoxGeo_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                GeoCoderStatusCode status = MainMap.SetPositionByKeywords(textBoxGeo.Text);
                if (status != GeoCoderStatusCode.G_GEO_SUCCESS)
                {
                    MessageBox.Show("Geocoder can't find: '" + textBoxGeo.Text + "', reason: " + status.ToString(), "GMap.NET", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
                else
                {
                    currentMarker.Position = MainMap.Position;
                }
            }
        }

        // zoom changed
        private void sliderZoom_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // updates circles on map
            foreach (var c in Circles)
            {
                UpdateCircle(c.Shape as Circle);
            }
        }

        // zoom up
        private void czuZoomUp_Click(object sender, RoutedEventArgs e)
        {
            MainMap.Zoom = ((int)MainMap.Zoom) + 1;
        }

        // zoom down
        private void czuZoomDown_Click(object sender, RoutedEventArgs e)
        {
            MainMap.Zoom = ((int)(MainMap.Zoom + 0.99)) - 1;
        }

        // prefetch
        private void button3_Click(object sender, RoutedEventArgs e)
        {
            RectLatLng area = MainMap.SelectedArea;
            if (!area.IsEmpty)
            {
                for (int i = (int)MainMap.Zoom; i <= MainMap.MaxZoom; i++)
                {
                    MessageBoxResult res = MessageBox.Show("Ready ripp at Zoom = " + i + " ?", "GMap.NET", MessageBoxButton.YesNoCancel);

                    if (res == MessageBoxResult.Yes)
                    {
                        GMap.NET.WindowsPresentation.TilePrefetcher obj = new GMap.NET.WindowsPresentation.TilePrefetcher();
                        obj.Owner = this;
                        obj.ShowCompleteMessage = true;
                        obj.Start(area, i, MainMap.MapProvider, 100);
                    }
                    else if (res == MessageBoxResult.No)
                    {
                        continue;
                    }
                    else if (res == MessageBoxResult.Cancel)
                    {
                        break;
                    }
                }
            }
            else
            {
                MessageBox.Show("Select map area holding ALT", "GMap.NET", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        // access mode
        private void comboBoxMode_DropDownClosed(object sender, EventArgs e)
        {
            MainMap.Manager.Mode = (AccessMode)comboBoxMode.SelectedItem;
            MainMap.ReloadMap();
        }

        // clear cache
        private void button4_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are You sure?", "Clear GMap.NET cache?", MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.OK)
            {
                try
                {
                    MainMap.Manager.PrimaryCache.DeleteOlderThan(DateTime.Now, null);
                    MessageBox.Show("Done. Cache is clear.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        // export
        private void button6_Click(object sender, RoutedEventArgs e)
        {
            MainMap.ShowExportDialog();
        }

        // import
        private void button5_Click(object sender, RoutedEventArgs e)
        {
            MainMap.ShowImportDialog();
        }

        // use route cache
        private void checkBoxCacheRoute_Checked(object sender, RoutedEventArgs e)
        {
            MainMap.Manager.UseRouteCache = checkBoxCacheRoute.IsChecked.Value;
        }

        // use geocoding cahce
        private void checkBoxGeoCache_Checked(object sender, RoutedEventArgs e)
        {
            MainMap.Manager.UseGeocoderCache = checkBoxGeoCache.IsChecked.Value;
            MainMap.Manager.UsePlacemarkCache = MainMap.Manager.UseGeocoderCache;
        }

        // save currnt view
        private void button7_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ImageSource img = MainMap.ToImageSource();
                PngBitmapEncoder en = new PngBitmapEncoder();
                en.Frames.Add(BitmapFrame.Create(img as BitmapSource));

                Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
                dlg.FileName = "GMap.NET Image"; // Default file name
                dlg.DefaultExt = ".png"; // Default file extension
                dlg.Filter = "Image (.png)|*.png"; // Filter files by extension
                dlg.AddExtension = true;
                dlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

                // Show save file dialog box
                bool? result = dlg.ShowDialog();

                // Process save file dialog box results
                if (result == true)
                {
                    // Save document
                    string filename = dlg.FileName;

                    using (System.IO.Stream st = System.IO.File.OpenWrite(filename))
                    {
                        en.Save(st);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        // clear all markers
        private void button10_Click(object sender, RoutedEventArgs e)
        {
            var clear = MainMap.Markers.Where(p => p != null && p != currentMarker);
            if (clear != null)
            {
                for (int i = 0; i < clear.Count(); i++)
                {
                    MainMap.Markers.Remove(clear.ElementAt(i));
                    i--;
                }
            }

            if (radioButtonPerformance.IsChecked == true)
            {
                tt = 0;
                if (!timer.IsEnabled)
                {
                    timer.Start();
                }
            }
        }

        // add marker
        private void button8_Click(object sender, RoutedEventArgs e)
        {
            GMapMarker m = new GMapMarker(currentMarker.Position);
            {
                Placemark? p = null;
                if (checkBoxPlace.IsChecked.Value)
                {
                    GeoCoderStatusCode status;
                    var plret = GMapProviders.GoogleMap.GetPlacemark(currentMarker.Position, out status);
                    if (status == GeoCoderStatusCode.G_GEO_SUCCESS && plret != null)
                    {
                        p = plret;
                    }
                }

                string ToolTipText;
                if (p != null)
                {
                    ToolTipText = p.Value.Address;
                }
                else
                {
                    ToolTipText = currentMarker.Position.ToString();
                }

                m.Shape = new CustomMarkerDemo(this, m, ToolTipText);
                m.ZIndex = 55;
            }
            MainMap.Markers.Add(m);
        }

        // sets route start
        private void button11_Click(object sender, RoutedEventArgs e)
        {
            start = currentMarker.Position;
        }

        // sets route end
        private void button9_Click(object sender, RoutedEventArgs e)
        {
            end = currentMarker.Position;
        }

        // adds route
        private void button12_Click(object sender, RoutedEventArgs e)
        {
            RoutingProvider rp = MainMap.MapProvider as RoutingProvider;
            if (rp == null)
            {
                rp = GMapProviders.OpenStreetMap; // use OpenStreetMap if provider does not implement routing
            }

            MapRoute route = rp.GetRoute(start, end, false, false, (int)MainMap.Zoom);
            if (route != null)
            {
                GMapMarker m1 = new GMapMarker(start);
                m1.Shape = new CustomMarkerDemo(this, m1, "Start: " + route.Name);

                GMapMarker m2 = new GMapMarker(end);
                m2.Shape = new CustomMarkerDemo(this, m2, "End: " + start.ToString());

                GMapRoute mRoute = new GMapRoute(route.Points);
                {
                    mRoute.ZIndex = -1;
                }

                MainMap.Markers.Add(m1);
                MainMap.Markers.Add(m2);
                MainMap.Markers.Add(mRoute);

                MainMap.ZoomAndCenterMarkers(null);
            }
        }

        // enables tile grid view
        private void checkBox1_Checked(object sender, RoutedEventArgs e)
        {
            MainMap.ShowTileGridLines = true;
        }

        // disables tile grid view
        private void checkBox1_Unchecked(object sender, RoutedEventArgs e)
        {
        }

        private void Window_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            int offset = 22;

            if (MainMap.IsFocused)
            {
                if (e.Key == Key.Left)
                {
                    MainMap.Offset(-offset, 0);
                }
                else if (e.Key == Key.Right)
                {
                    MainMap.Offset(offset, 0);
                }
                else if (e.Key == Key.Up)
                {
                    MainMap.Offset(0, -offset);
                }
                else if (e.Key == Key.Down)
                {
                    MainMap.Offset(0, offset);
                }
                else if (e.Key == Key.Add)
                {
                    czuZoomUp_Click(null, null);
                }
                else if (e.Key == Key.Subtract)
                {
                    czuZoomDown_Click(null, null);
                }
            }
        }

        // set real time demo
        private void realTimeChanged(object sender, RoutedEventArgs e)
        {
            MainMap.Markers.Clear();

            // start performance test
            if (radioButtonPerformance.IsChecked == true)
            {
                timer.Start();
            }
            else
            {
                // stop performance test
                timer.Stop();
            }

            // start realtime transport tracking demo            
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.A)
            {
                MainMap.Bearing--;
            }
            else if (e.Key == Key.Z)
            {
                MainMap.Bearing++;
            }
        }

        string TGLSEKARANG = "";
        string TGLBESOK = "";
        string KODESALES = "";
        string NAMASALES = "";
        string KODENOTA = "";
        private void btn_selectdate_Click(object sender, RoutedEventArgs e)
        {
            //tampil
            if (list_date_select.Count == 0)
            {
                //tambah
                Window_DateSelect mx = new Window_DateSelect();
                mx.Closed += delegate
                {
                    list_date_select.Remove(mx);
                    GC.Collect();
                };

                mx.OnDatePilihChanged += delegate (object senders, DatePilihArgs es)
                {
                    if (es != null)
                    {
                        Console.WriteLine("TGL SEKARANG=" + es.TglSekarang);
                        Console.WriteLine("TGL BESOK=" + es.TglBesok);

                        TGLSEKARANG = es.TglSekarang;
                        TGLBESOK = es.TglBesok;

                        if (TGLSEKARANG.Equals("") == false)
                        {
                            label_tanggal.Content = TGLSEKARANG;
                        } else
                        {
                            label_tanggal.Content = "No date selected";
                        }
                    } else
                    {
                        label_tanggal.Content = "No date selected";
                    }
                };

                list_date_select.Add(mx);

                //jalankan
                //list_date_select[list_date_select.Count - 1].Topmost = true;
                list_date_select[list_date_select.Count - 1].ShowDialog();
            }
        }


        private void btn_selectsales_Click(object sender, RoutedEventArgs e)
        {
            //tampil
            if (list_sales_select.Count == 0)
            {
                //tambah
                Window_SalesSelect mx = new Window_SalesSelect();
                mx.Closed += delegate
                {
                    list_sales_select.Remove(mx);
                    GC.Collect();
                };

                mx.OnSalesPilihChanged += delegate (object senders, SalesPilihArgs es)
                {
                    if (es != null)
                    {
                        Console.WriteLine("KODESALES=" + es.KodeSales);
                        Console.WriteLine("NAMASALES=" + es.NamaSales);

                        KODESALES = es.KodeSales;
                        NAMASALES = es.NamaSales;

                        if (KODESALES.Equals("") == false)
                        {
                            label_sales.Content = NAMASALES + " (" + KODESALES + ")";
                        }
                        else
                        {
                            label_sales.Content = "No sales selected";
                        }
                    }
                    else
                    {
                        label_sales.Content = "No sales selected";
                    }
                };

                list_sales_select.Add(mx);

                //jalankan
                //list_date_select[list_date_select.Count - 1].Topmost = true;
                list_sales_select[list_sales_select.Count - 1].ShowDialog();
            }
        }

        private void Proses()
        {
            if (TGLSEKARANG.Equals("") == false && TGLBESOK.Equals("") == false && KODESALES.Equals("") == false)
            {
                //ambil kodenota
                //tampil
                if (list_kodenota_select.Count == 0)
                {
                    //tambah
                    Window_KodenotaSelect mx = new Window_KodenotaSelect(TGLSEKARANG, TGLBESOK, KODESALES);
                    mx.Closed += delegate
                    {
                        list_kodenota_select.Remove(mx);
                        GC.Collect();
                    };

                    mx.OnKodenotaPilihChanged += delegate (object senders, KodenotaPilihArgs es)
                    {
                        if (es != null)
                        {
                            Console.WriteLine("KODENOTA=" + es.Kodenota);
                            KODENOTA = es.Kodenota;

                            if (KODENOTA.Equals("") == false)
                            {
                                //tampilkan detail info
                                panel_detail.Visibility = Visibility.Visible;

                                label_detail_sales.Content = "" + NAMASALES;
                                label_kodenota.Content = "" + KODENOTA;
                                label_checkin.Content = "" + es.JmlTerkunjungi + "/" + es.JmlKunjungan;
                                label_status.Content = "" + es.Keterangan;

                                //proses sesungguhnya
                                ProsesAmbilGPSPerjalanan();
                            }
                        }
                    };

                    list_kodenota_select.Add(mx);

                    //jalankan
                    //list_date_select[list_date_select.Count - 1].Topmost = true;
                    list_kodenota_select[list_kodenota_select.Count - 1].ShowDialog();
                }
            } else
            {
                MessageBox.Show("Could not process your request. Make sure you have selected a sales and a travel date", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ProsesRefreshMarkerGPS()
        {
            try
            {
                //sukses tampilkan
                try
                {
                    MainMap.Markers.Clear();
                }
                catch (Exception e)
                {
                    Console.WriteLine("" + e.Message + " " + e.StackTrace);
                }

                int indexZ = 0;
                if (Setting_variabel.IS_VISIBLE_SALES_MARKER == true)
                {
                    //marker sales
                    for (int i = 0; i < list_daftar_sales.Count; i++)
                    {
                        PointLatLng pos = new PointLatLng(list_daftar_sales[i].Latitude, list_daftar_sales[i].Longitude);
                        GMapMarker it = new GMapMarker(pos);
                        {
                            it.ZIndex = (indexZ + 1);
                            indexZ++;
                            it.Shape = new CustomMarkerDemoSales(this, it, "" + list_daftar_sales[i].NamaSales);
                        }
                        MainMap.Markers.Add(it);
                    }
                }

                if (Setting_variabel.IS_VISIBLE_CUST_MARKER == true) {
                    //marker kunjungan
                    int index_kunjung = 1;
                    for (int i = 0; i < list_daftar_perjalanan.Count; i++)
                    {
                        PointLatLng pos = new PointLatLng(list_daftar_perjalanan[i].Latitude, list_daftar_perjalanan[i].Longitude);
                        GMapMarker it = new GMapMarker(pos);
                        {
                            it.ZIndex = (indexZ + 1);
                            indexZ++;

                            if (list_daftar_perjalanan[i].Checkin != null)
                            {
                                var dateTimeStr = "2000-01-01";
                                var user_time = DateTime.Parse(dateTimeStr);

                                if (list_daftar_perjalanan[i].Checkin > user_time)
                                {
                                    it.Shape = new CustomMarkerDemo(this, it, "" + list_daftar_perjalanan[i].NamaPelanggan, index_kunjung);
                                    list_daftar_perjalanan[i].Index = index_kunjung;
                                    list_daftar_perjalanan[i].Keterangan = "Sudah Kunjung";

                                    index_kunjung++;
                                }
                                else
                                {
                                    it.Shape = new CustomMarkerDemo(this, it, "" + list_daftar_perjalanan[i].NamaPelanggan, 0); //belum kunjung
                                    list_daftar_perjalanan[i].Index = 0;
                                    list_daftar_perjalanan[i].Keterangan = "Belum Kunjung";
                                }
                            }
                            else
                            {
                                it.Shape = new CustomMarkerDemo(this, it, "" + list_daftar_perjalanan[i].NamaPelanggan, 0); //belum kunjung
                                list_daftar_perjalanan[i].Index = 0;
                                list_daftar_perjalanan[i].Keterangan = "Belum Kunjung";
                            }
                        }
                        MainMap.Markers.Add(it);
                    }
                }

                //tanpa zoom dan center


            }
            catch (Exception e) { }
        }

        private void ProsesAmbilGPSSales(TipeTampilSales TIPE)
        {
            TIPETAMPILSALES_TMP = TIPE;

            Model_Login m = new Model_Login();
            bool stat = false;

            bool ada = false;
            list_daftar_sales.Clear();

            // grid_progress.Visibility = Visibility.Visible;

            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.ProgressChanged += delegate
            {

            };
            worker.DoWork += delegate
            {
                try
                {
                    if (TIPE==TipeTampilSales.All)
                    {
                        Console.WriteLine("tipe all");
                        #region tipe all
                        using (SqlConnection connection = new SqlConnection(Setting_variabel.mykoneksi_sql))
                        {
                            connection.Open();

                            String sqlQuery = "select kode,nama,latitude,longitude from salesperson where Aktif=1 and Latitude is not null and Longitude is not null and Longitude!='' and Latitude!='' and CAST(Latitude as float)!=0 and CAST(Longitude as float)!=0";
                            using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                            {
                                SqlDataReader reader = command.ExecuteReader();

                                while (reader.Read())
                                {

                                    Model_Sales item = new Model_Sales();
                                    item.KodeSales = reader.GetString(0);
                                    item.NamaSales = reader.GetString(1);

                                    double latitude = 0;
                                    double longitude = 0;
                                    try
                                    {
                                        latitude = Double.Parse(reader.GetString(2));
                                    }
                                    catch (Exception e) { }

                                    try
                                    {
                                        longitude = Double.Parse(reader.GetString(3));
                                    }
                                    catch (Exception e) { }

                                    item.Latitude = latitude;
                                    item.Longitude = longitude;

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
                        #endregion tipe all
                    }
                    else if (TIPE == TipeTampilSales.Selected)
                    {
                        Console.WriteLine("tipe selected");
                        #region tipe selected
                        using (SqlConnection connection = new SqlConnection(Setting_variabel.mykoneksi_sql))
                        {
                            connection.Open();

                            String sqlQuery = "select kode,nama,latitude,longitude from salesperson where Aktif=1 and Latitude is not null and Longitude is not null and Longitude!='' and Latitude!='' and CAST(Latitude as float)!=0 and CAST(Longitude as float)!=0 and kode='"+KODESALES+"'";
                            //try
                            //{
                            //    if(strb.Length>0)
                            //    strb.Remove(0, strb.Length);
                            //}
                            //catch (Exception e) { }

                            using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                            {
                                SqlDataReader reader = command.ExecuteReader();

                                while (reader.Read())
                                {

                                    Model_Sales item = new Model_Sales();
                                    item.KodeSales = reader.GetString(0);
                                    item.NamaSales = reader.GetString(1);

                                    double latitude = 0;
                                    double longitude = 0;
                                    try
                                    {
                                        latitude = Double.Parse(reader.GetString(2));
                                    }
                                    catch (Exception e) { }

                                    try
                                    {
                                        longitude = Double.Parse(reader.GetString(3));
                                    }
                                    catch (Exception e) { }

                                    item.Latitude = latitude;
                                    item.Longitude = longitude;

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
                        #endregion tipe selected
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
                //grid_progress.Visibility = Visibility.Collapsed;

                m.IsSukses = stat;

                if (m.IsSukses == true)
                {
                    //sukses tampilkan
                    try
                    {
                        MainMap.Markers.Clear();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("" + e.Message + " " + e.StackTrace);
                    }

                    int indexZ = 0;
                    if (Setting_variabel.IS_VISIBLE_SALES_MARKER == true)
                    {
                        //marker sales
                        for (int i = 0; i < list_daftar_sales.Count; i++)
                        {
                            PointLatLng pos = new PointLatLng(list_daftar_sales[i].Latitude, list_daftar_sales[i].Longitude);
                            GMapMarker it = new GMapMarker(pos);
                            {
                                it.ZIndex = (indexZ + 1);
                                indexZ++;
                                it.Shape = new CustomMarkerDemoSales(this, it, "" + list_daftar_sales[i].NamaSales);
                            }
                            MainMap.Markers.Add(it);
                        }
                    }

                    if (Setting_variabel.IS_VISIBLE_CUST_MARKER == true)
                    {
                        //marker kunjungan
                        int index_kunjung = 1;
                        for (int i = 0; i < list_daftar_perjalanan.Count; i++)
                        {
                            PointLatLng pos = new PointLatLng(list_daftar_perjalanan[i].Latitude, list_daftar_perjalanan[i].Longitude);
                            GMapMarker it = new GMapMarker(pos);
                            {
                                it.ZIndex = (indexZ + 1);
                                indexZ++;

                                if (list_daftar_perjalanan[i].Checkin != null)
                                {
                                    var dateTimeStr = "2000-01-01";
                                    var user_time = DateTime.Parse(dateTimeStr);

                                    if (list_daftar_perjalanan[i].Checkin > user_time)
                                    {
                                        it.Shape = new CustomMarkerDemo(this, it, "" + list_daftar_perjalanan[i].NamaPelanggan, index_kunjung);
                                        list_daftar_perjalanan[i].Index = index_kunjung;
                                        list_daftar_perjalanan[i].Keterangan = "Sudah Kunjung";

                                        index_kunjung++;
                                    }
                                    else
                                    {
                                        it.Shape = new CustomMarkerDemo(this, it, "" + list_daftar_perjalanan[i].NamaPelanggan, 0); //belum kunjung
                                        list_daftar_perjalanan[i].Index = 0;
                                        list_daftar_perjalanan[i].Keterangan = "Belum Kunjung";
                                    }
                                }
                                else
                                {
                                    it.Shape = new CustomMarkerDemo(this, it, "" + list_daftar_perjalanan[i].NamaPelanggan, 0); //belum kunjung
                                    list_daftar_perjalanan[i].Index = 0;
                                    list_daftar_perjalanan[i].Keterangan = "Belum Kunjung";
                                }
                            }
                            MainMap.Markers.Add(it);
                        }
                    }
                }
                else
                {
                    //MessageBox.Show("" + m.Keterangan, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }

            };
            worker.RunWorkerAsync();
        }

        private void ProsesAmbilGPSPerjalanan()
        {
            Model_Login m = new Model_Login();
            bool stat = false;

            bool ada = false;
            list_daftar_perjalanan.Clear();

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

                        String sqlQuery = "select dp.kodenota,dp.cust,p.perusahaan,dp.checkin,dp.checkout, " + 
                            "case when(CAST(dp.Latitude as float) = 0 or dp.Latitude is null or dp.Latitude = '') " + 
                            "then p.Latitude else dp.Latitude end as Latitude, " + 
                            "case when(CAST(dp.Longitude as float) = 0 or dp.Longitude is null or dp.Longitude = '') " + 
                            "then p.Longitude else dp.Longitude end as Longitude " + 
                            "from DetailPerjalanan dp join ( " + 
                            "select perusahaan, latitude, longitude, kode = min(kode) from pelanggan " + 
                            "group by cif, perusahaan, latitude, longitude) p on dp.cust = p.Kode " + 
                            "where dp.kodenota = '" + KODENOTA + "' " + 
                            "order by CASE WHEN dp.Checkin IS NULL THEN '01/01/9900 12:00:00 AM' " + 
                            "WHEN dp.Checkin = '01/01/1900 12:00:00 AM' THEN '01/01/9900 12:00:00 AM' " + 
                            "ELSE dp.Checkin END, p.Perusahaan asc";
                        using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                        {
                            SqlDataReader reader = command.ExecuteReader();

                            while (reader.Read())
                            {

                                Model_Perjalanan item = new Model_Perjalanan();
                                item.Kodenota = reader.GetString(0);
                                item.KodePelanggan = reader.GetString(1);
                                item.NamaPelanggan = reader.GetString(2);

                                try
                                {
                                    item.Checkin = reader.GetDateTime(3);
                                }
                                catch (Exception e) { }

                                try
                                {
                                    item.Checkout = reader.GetDateTime(4);
                                }
                                catch (Exception e) { }

                                double latitude = 0;
                                double longitude = 0;
                                try
                                {
                                    latitude = Double.Parse(reader.GetString(5));
                                }
                                catch (Exception e) { }

                                try
                                {
                                    longitude = Double.Parse(reader.GetString(6));
                                }
                                catch (Exception e) { }

                                item.Latitude = latitude;
                                item.Longitude = longitude;

                                App.Current.Dispatcher.Invoke((Action)delegate // <--- HERE
                                {
                                    list_daftar_perjalanan.Add(item);
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
                        MessageBox.Show("Could not found travel detail for sales \"" + KODESALES + "\" at " + TGLSEKARANG + " ",
                                        "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    } else
                    {
                        //sukses tampilkan
                        try
                        {
                            MainMap.Markers.Clear();
                        }
                        catch (Exception e) {
                            Console.WriteLine("" + e.Message + " " + e.StackTrace);
                        }


                        int indexZ = 0;
                        if (Setting_variabel.IS_VISIBLE_SALES_MARKER == true)
                        {
                            //marker sales
                            for (int i = 0; i < list_daftar_sales.Count; i++)
                            {
                                PointLatLng pos = new PointLatLng(list_daftar_sales[i].Latitude, list_daftar_sales[i].Longitude);
                                GMapMarker it = new GMapMarker(pos);
                                {
                                    it.ZIndex = (indexZ + 1);
                                    indexZ++;
                                    it.Shape = new CustomMarkerDemoSales(this, it, "" + list_daftar_sales[i].NamaSales);
                                }
                                MainMap.Markers.Add(it);
                            }
                        }

                        if (Setting_variabel.IS_VISIBLE_CUST_MARKER == true) {
                            //marker kunjungan
                            int index_kunjung = 1;
                            for (int i = 0; i < list_daftar_perjalanan.Count; i++)
                            {
                                PointLatLng pos = new PointLatLng(list_daftar_perjalanan[i].Latitude, list_daftar_perjalanan[i].Longitude);
                                GMapMarker it = new GMapMarker(pos);
                                {
                                    it.ZIndex = (indexZ + 1);
                                    indexZ++;

                                    if (list_daftar_perjalanan[i].Checkin != null)
                                    {
                                        var dateTimeStr = "2000-01-01";
                                        var user_time = DateTime.Parse(dateTimeStr);

                                        if (list_daftar_perjalanan[i].Checkin > user_time)
                                        {
                                            it.Shape = new CustomMarkerDemo(this, it, "" + list_daftar_perjalanan[i].NamaPelanggan, index_kunjung);
                                            list_daftar_perjalanan[i].Index = index_kunjung;
                                            list_daftar_perjalanan[i].Keterangan = "Sudah Kunjung";

                                            index_kunjung++;
                                        }
                                        else
                                        {
                                            it.Shape = new CustomMarkerDemo(this, it, "" + list_daftar_perjalanan[i].NamaPelanggan, 0); //belum kunjung
                                            list_daftar_perjalanan[i].Index = 0;
                                            list_daftar_perjalanan[i].Keterangan = "Belum Kunjung";
                                        }
                                    }
                                    else
                                    {
                                        it.Shape = new CustomMarkerDemo(this, it, "" + list_daftar_perjalanan[i].NamaPelanggan, 0); //belum kunjung
                                        list_daftar_perjalanan[i].Index = 0;
                                        list_daftar_perjalanan[i].Keterangan = "Belum Kunjung";
                                    }
                                }
                                MainMap.Markers.Add(it);
                            }
                        }

                        if (MainMap.Markers.Count > 1)
                        {
                            MainMap.ZoomAndCenterMarkers(null);
                        }

                        //GeoCoderStatusCode status = GeoCoderStatusCode.Unknow;

                        //PointLatLng? city = GMapProviders.GoogleMap.GetPoint("Lithuania, Vilnius", out status);
                        //if (city != null && status == GeoCoderStatusCode.G_GEO_SUCCESS)
                        //{
                        //    GMapMarker it = new GMapMarker(city.Value);
                        //    {
                        //        it.ZIndex = 55;
                        //        it.Shape = new CustomMarkerDemo(this, it, "Welcome to Lithuania! ;}");
                        //    }
                        //    MainMap.Markers.Add(it);

                        //    #region -- add some markers and zone around them --
                        //    //if(false)
                        //    {
                        //        List<PointAndInfo> objects = new List<PointAndInfo>();
                        //        {
                        //            string area = "Antakalnis";
                        //            PointLatLng? pos = GMapProviders.GoogleMap.GetPoint("Lithuania, Vilnius, " + area, out status);
                        //            if (pos != null && status == GeoCoderStatusCode.G_GEO_SUCCESS)
                        //            {
                        //                objects.Add(new PointAndInfo(pos.Value, area));
                        //            }
                        //        }
                        //        {
                        //            string area = "Senamiestis";
                        //            PointLatLng? pos = GMapProviders.GoogleMap.GetPoint("Lithuania, Vilnius, " + area, out status);
                        //            if (pos != null && status == GeoCoderStatusCode.G_GEO_SUCCESS)
                        //            {
                        //                objects.Add(new PointAndInfo(pos.Value, area));
                        //            }
                        //        }
                        //        {
                        //            string area = "Pilaite";
                        //            PointLatLng? pos = GMapProviders.GoogleMap.GetPoint("Lithuania, Vilnius, " + area, out status);
                        //            if (pos != null && status == GeoCoderStatusCode.G_GEO_SUCCESS)
                        //            {
                        //                objects.Add(new PointAndInfo(pos.Value, area));
                        //            }
                        //        }
                        //        AddDemoZone(8.8, city.Value, objects);
                        //    }
                        //    #endregion
                        //}

                    }
                }
                else
                {
                    MessageBox.Show("" + m.Keterangan, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }

            };
            worker.RunWorkerAsync();
        }

        private void btn_proses_Click(object sender, RoutedEventArgs e)
        {
            Proses();
        }

        private void btn_signout_Click(object sender, RoutedEventArgs e)
        {

            MessageBoxResult result = MessageBox.Show("Do you want to sign out?", "Sign out", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                EksekSignout();
            }
            else
            {

            }


        }


        private void EksekSignout()
        {
            tabcontrol_utama.SelectedIndex = 0;

            textbox_email.Text = "";
            textbox_password.Password = "";

            panel_detail.Visibility = Visibility.Collapsed;

            label_kodenota.Content = "";
            label_checkin.Content = "0/0";
            label_detail_sales.Content = "Details";
            label_tanggal.Content = "No date selected";
            label_sales.Content = "No sales selected";
            label_status.Content = "In progress";
            label_user.Content = "Ahmad Reza Musthafa";

            KODENOTA = "";
            TGLSEKARANG = "";
            TGLBESOK = "";
            KODESALES = "";
            NAMASALES = "";

            list_daftar_perjalanan.Clear();
            list_daftar_sales.Clear();
        }

        /*
         
         SQL KONEKSI
         
         
         */

        public void CekLogin(string username, string password)
        {
            Model_Login m = new Model_Login();
            bool stat = false;

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
                    Setting_variabel.Update_UserPassmykoneksi_sql(username, password);
                    using (SqlConnection connection = new SqlConnection(Setting_variabel.mykoneksi_sql))
                    {
                        connection.Open();

                        String sqlQuery = "select kode,nama from staff where email='" + username + "' or nama='" + username + "'";
                        using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                        {
                            SqlDataReader reader = command.ExecuteReader();
                            stat = false;

                            while (reader.Read())
                            {
                                Setting_variabel.NAMA = reader.GetString(1);
                                stat = true;
                            }
                            reader.Close();
                        }

                        m.Keterangan = "Unable to process your request: Make sure you have entered the correct username";

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

                label_user.Content = "" + Setting_variabel.NAMA;

                m.IsSukses = stat;

                if (m.IsSukses == true)
                {
                    tabcontrol_utama.SelectedIndex = 1;
                    ProsesAmbilGPSSales(TipeTampilSales.All);
                }
                else
                {
                    MessageBox.Show("" + m.Keterangan, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            };
            worker.RunWorkerAsync();

        }

        private void btnLeftMenuHide_Click(object sender, RoutedEventArgs e)
        {
            ShowHideMenu("sbHideLeftMenu", btnLeftMenuHide, btnLeftMenuShow, pnlLeftMenu);
        }

        private void btnLeftMenuShow_Click(object sender, RoutedEventArgs e)
        {
            ShowHideMenu("sbShowLeftMenu", btnLeftMenuHide, btnLeftMenuShow, pnlLeftMenu);
        }
        private void btnRightMenuHide_Click(object sender, RoutedEventArgs e)
        {
            ShowHideMenu("sbHideRightMenu", btnRightMenuHide, btnRightMenuShow, pnlRightMenu);
        }

        private void btnRightMenuShow_Click(object sender, RoutedEventArgs e)
        {
            ShowHideMenu("sbShowRightMenu", btnRightMenuHide, btnRightMenuShow, pnlRightMenu);
        }

        private void ShowHideMenu(string Storyboards, Button btnHide, Button btnShow, StackPanel pnl)
        {
            Storyboard sb = Resources[Storyboards] as Storyboard;
            sb.Begin(pnl);

            if (Storyboards.Contains("Show"))
            {
                btnHide.Visibility = System.Windows.Visibility.Visible;
                btnShow.Visibility = System.Windows.Visibility.Hidden;
            }
            else if (Storyboards.Contains("Hide"))
            {
                btnHide.Visibility = System.Windows.Visibility.Hidden;
                btnShow.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private void list_daftar_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            list_daftar.SelectedItem = null;
        }

        private void checkbox_pilihsales_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox checkbox = sender as CheckBox;
            Model_Sales item = checkbox.DataContext as Model_Sales;
            Console.WriteLine("CHECK VIEW: " + item.KodeSales + " - " + item.NamaSales);

            item.IsVisible = true;
        }

        private void checkbox_pilihsales_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox checkbox = sender as CheckBox;
            Model_Sales item = checkbox.DataContext as Model_Sales;
            Console.WriteLine("UNCHECK VIEW: " + item.KodeSales + " - " + item.NamaSales);

            item.IsVisible = false;
        }

        private void listview_daftar_sales_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            listview_daftar_sales.SelectedItem = null;
            ProsesRefreshMarkerGPS();
        }

        private void checkbox_cust_Checked(object sender, RoutedEventArgs e)
        {
            Setting_variabel.IS_VISIBLE_CUST_MARKER = true;
            ProsesRefreshMarkerGPS();
        }

        private void checkbox_cust_Unchecked(object sender, RoutedEventArgs e)
        {
            Setting_variabel.IS_VISIBLE_CUST_MARKER = false;
            ProsesRefreshMarkerGPS();
        }

        private void checkbox_sales_Checked(object sender, RoutedEventArgs e)
        {
            Setting_variabel.IS_VISIBLE_SALES_MARKER = true;
            ProsesRefreshMarkerGPS();
        }

        private void checkbox_sales_Unchecked(object sender, RoutedEventArgs e)
        {
            Setting_variabel.IS_VISIBLE_SALES_MARKER = false;
            ProsesRefreshMarkerGPS();
        }
        
       
        enum TipeTampilSales
        {
            All,
            Selected
        }
        
        TipeTampilSales TIPETAMPILSALES_TMP= TipeTampilSales.All;
        private void radio_sales_all_Checked(object sender, RoutedEventArgs e)
        {
            ProsesAmbilGPSSales(TipeTampilSales.All);
        }

        private void radio_sales_all_Unchecked(object sender, RoutedEventArgs e)
        {

        }

        private void radio_sales_sel_Checked(object sender, RoutedEventArgs e)
        {
            ProsesAmbilGPSSales(TipeTampilSales.Selected);
        }

        private void radio_sales_sel_Unchecked(object sender, RoutedEventArgs e)
        {

        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (timer_refresh != null)
            {
                timer_refresh.Enabled = false;
            }
        }

        private void btn_preference_Click(object sender, RoutedEventArgs e)
        {
            //tampil
            if (list_preference.Count == 0)
            {
                //tambah
                Window_Preference mx = new Window_Preference();
                mx.Closed += delegate
                {
                    list_preference.Remove(mx);
                    GC.Collect();
                };


                list_preference.Add(mx);

                //jalankan
                //list_date_select[list_date_select.Count - 1].Topmost = true;
                list_preference[list_preference.Count - 1].ShowDialog();
            }
        }
    }


    public class MapValidationRule : ValidationRule
    {
        bool UserAcceptedLicenseOnce = false;
        internal MainWindow Window;

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (!(value is OpenStreetMapProviderBase))
            {
                if (!UserAcceptedLicenseOnce)
                {
                    if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + System.IO.Path.DirectorySeparatorChar + "License.txt"))
                    {
                        string ctn = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + System.IO.Path.DirectorySeparatorChar + "License.txt");
                        int li = ctn.IndexOf("License");
                        string txt = ctn.Substring(li);

                        var d = new Message();
                        d.richTextBox1.Text = txt;

                        if (true == d.ShowDialog())
                        {
                            UserAcceptedLicenseOnce = true;
                            if (Window != null)
                            {
                                Window.Title += " - license accepted by " + Environment.UserName + " at " + DateTime.Now;
                            }
                        }
                    }
                    else
                    {
                        // user deleted License.txt ;}
                        UserAcceptedLicenseOnce = true;
                    }
                }

                if (!UserAcceptedLicenseOnce)
                {
                    return new ValidationResult(false, "user do not accepted license ;/");
                }
            }

            return new ValidationResult(true, null);
        }
    }
}
