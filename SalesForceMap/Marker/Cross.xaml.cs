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

namespace SalesForceMap.Marker
{
    /// <summary>
    /// Interaction logic for Cross.xaml
    /// </summary>
    public partial class Cross : UserControl
    {
        public Cross()
        {
            InitializeComponent();
            this.IsHitTestVisible = false;
        }
    }
}
