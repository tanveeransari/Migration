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

namespace SciChartElD
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int counter = 1;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void chartCmd_Click(object sender, RoutedEventArgs e)
        {
            string formName = "Chart " + counter++;
            ChartWindow frm = new ChartWindow(formName);
            frm.Show();
        }
    }
}
