using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Shapes;

namespace SciChartElD
{
    public partial class ChartWindow : Window
    {
        public ChartWindow()
            : this("Tanveer")
        {

        }
        public ChartWindow(string pChartName)
        {
            InitializeComponent();
            this.Title = pChartName;
            this.AllowDrop = true;
        }
    }
}
