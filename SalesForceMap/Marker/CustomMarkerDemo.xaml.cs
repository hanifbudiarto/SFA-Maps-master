using System;
using System.Collections.Generic;
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
using GMap.NET.WindowsPresentation;
using System.Windows.Controls.Primitives;

namespace SalesForceMap.Marker
{
    /// <summary>
    /// Interaction logic for CustomMarkerDemo.xaml
    /// </summary>
    public partial class CustomMarkerDemo : UserControl
    {
        Popup Popup;
        Label Label;
        GMapMarker Marker;
        MainWindow MainWindow;

        public CustomMarkerDemo(MainWindow window, GMapMarker marker, string title)
        { }

        public CustomMarkerDemo(MainWindow window, GMapMarker marker, string title,int index)
        {
            InitializeComponent();

            this.MainWindow = window;
            this.Marker = marker;

            Popup = new Popup();
            Label = new Label();
            
            StackPanel stackpanel = new StackPanel();
            Label label_index = new Label();

            this.Unloaded += new RoutedEventHandler(CustomMarkerDemo_Unloaded);
            this.Loaded += new RoutedEventHandler(CustomMarkerDemo_Loaded);
            this.SizeChanged += new SizeChangedEventHandler(CustomMarkerDemo_SizeChanged);
            this.MouseEnter += new MouseEventHandler(MarkerControl_MouseEnter);
            this.MouseLeave += new MouseEventHandler(MarkerControl_MouseLeave);
            this.MouseMove += new MouseEventHandler(CustomMarkerDemo_MouseMove);
            this.MouseLeftButtonUp += new MouseButtonEventHandler(CustomMarkerDemo_MouseLeftButtonUp);
            this.MouseLeftButtonDown += new MouseButtonEventHandler(CustomMarkerDemo_MouseLeftButtonDown);

            Popup.Placement = PlacementMode.Mouse;
            {
                //Label.Background = Brushes.Gray;
                //Label.Foreground = Brushes.White;
                //Label.BorderBrush = Brushes.WhiteSmoke;
                //Label.BorderThickness = new Thickness(2);
                Label.Padding = new Thickness(5);
                Label.Foreground = Brushes.Black;
                Label.FontSize = 16;
                Label.Content = title;

                label_index.Padding = new Thickness(5);
                label_index.Foreground = Brushes.Black;
                label_index.FontSize = 13;
                label_index.Content = "Kunjungan ke-"+index;

                if (index == 0)
                {
                    label_index.Visibility = Visibility.Collapsed;
                }else
                {
                    label_index.Visibility = Visibility.Visible;
                }

                stackpanel.Background = Brushes.White;
            }

            stackpanel.Children.Add(Label);
            stackpanel.Children.Add(label_index);
            Popup.Child = stackpanel;


            if (index == 0)
            {
                border_index.Visibility = Visibility.Collapsed;
                label_status.Content = "Belum Kunjung";
            }else
            {
                border_index.Visibility = Visibility.Visible;
                label_indeks.Content = "" + index;
                label_status.Content = "Sudah Kunjung";
            }

            //Popup.Child = Label;
        }

        void CustomMarkerDemo_Loaded(object sender, RoutedEventArgs e)
        {
            if (icon.Source.CanFreeze)
            {
                icon.Source.Freeze();
            }
        }

        void CustomMarkerDemo_Unloaded(object sender, RoutedEventArgs e)
        {
            this.Unloaded -= new RoutedEventHandler(CustomMarkerDemo_Unloaded);
            this.Loaded -= new RoutedEventHandler(CustomMarkerDemo_Loaded);
            this.SizeChanged -= new SizeChangedEventHandler(CustomMarkerDemo_SizeChanged);
            this.MouseEnter -= new MouseEventHandler(MarkerControl_MouseEnter);
            this.MouseLeave -= new MouseEventHandler(MarkerControl_MouseLeave);
            this.MouseMove -= new MouseEventHandler(CustomMarkerDemo_MouseMove);
            this.MouseLeftButtonUp -= new MouseButtonEventHandler(CustomMarkerDemo_MouseLeftButtonUp);
            this.MouseLeftButtonDown -= new MouseButtonEventHandler(CustomMarkerDemo_MouseLeftButtonDown);

            Marker.Shape = null;
            icon.Source = null;
            icon = null;
            Popup = null;
            Label = null;
        }

        void CustomMarkerDemo_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Marker.Offset = new Point(-e.NewSize.Width / 2, -e.NewSize.Height);
        }

        void CustomMarkerDemo_MouseMove(object sender, MouseEventArgs e)
        {
            //if (e.LeftButton == MouseButtonState.Pressed && IsMouseCaptured)
            //{
            //    Point p = e.GetPosition(MainWindow.MainMap);
            //    Marker.Position = MainWindow.MainMap.FromLocalToLatLng((int)(p.X), (int)(p.Y));
            //}
        }

        void CustomMarkerDemo_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!IsMouseCaptured)
            {
                Mouse.Capture(this);
            }
        }

        void CustomMarkerDemo_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (IsMouseCaptured)
            {
                Mouse.Capture(null);
            }
        }

        void MarkerControl_MouseLeave(object sender, MouseEventArgs e)
        {
            Marker.ZIndex -= 10000;
            Popup.IsOpen = false;
        }

        void MarkerControl_MouseEnter(object sender, MouseEventArgs e)
        {
            Marker.ZIndex += 10000;
            Popup.IsOpen = true;
        }
    }
}
