using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using GMap.NET;

namespace SalesForceMap
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    //public partial class App : Application
    //{
    //}

    public partial class App : Application
    {
        
    }

    public class Dummy
    {

    }

    public struct PointAndInfo
    {
        public PointLatLng Point;
        public string Info;

        public PointAndInfo(PointLatLng point, string info)
        {
            Point = point;
            Info = info;
        }
    }
}
