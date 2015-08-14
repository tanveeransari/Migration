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

        /// <summary>
        /// Pass window key down event to control to handle
        /// </summary>
        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            Debug.WriteLine("  *** ***  KeyDown  main window   *** ");
            this.sciChartControl.OnSciChartControlKeyDown(sender, e);
        }
    }
}
