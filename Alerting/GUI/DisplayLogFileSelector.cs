using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Alerting.Alert;

namespace Alerting.GUI
{
    public partial class DisplayLogFileSelector : UserControl
    {
        public DisplayLogFileSelector()
        {
            InitializeComponent();
         //   if (!DesignMode)
         //   {
         //       LoadLogs();
         //   }
        }

        private void LoadLogs()
        {
            IDictionary<string,string> list = LoggerHome.GetLogger(this).GetLogPathMaps();

            foreach (KeyValuePair<string, string> keyValuePair in list)
            {
                int index = comboBoxLogs.Items.Add(new LogModel(keyValuePair));
               
            }

        }

        private void comboBoxLogs_SelectedIndexChanged(object sender, EventArgs e)
        {
            //string name = LoggerHome.GetLogger(this).GetAllExceptionsPath();
            LogModel model = comboBoxLogs.SelectedItem as LogModel;

            if (model != null)
            {
                string name = model.FileLocation;

                if (name != "")
                {
                    LoggerHome.GetLogger(this).Audit("User clicked link to view log file:" + name);
                    System.Diagnostics.Process.Start(name);
                }
                else
                {
                    LoggerHome.GetLogger(this).Error("Can't find log file path for Logger.GetAllExceptionsPath");
                    MessageBox.Show("Log file path: " + name + " not found. Check log file for details", "Can't open log file", MessageBoxButtons.OK);

                }
            }
            else
            {
                // TODO handle null selection
            }
        }

        private void DisplayLogFileSelector_Load(object sender, EventArgs e)
        {
            if (!DesignMode)
            {
                LoadLogs();
            }

        }
    }

    public class LogModel
    {
        public string Name { get; set; }
        public string FileLocation { get; set; }

        public LogModel(KeyValuePair<string, string> keyValuePair)
        {
            if (keyValuePair.Key != null && keyValuePair.Value != null)
            {
                Name = keyValuePair.Key;
                FileLocation = keyValuePair.Value;
            }
        }

        public override string ToString()
        {
            return Name;
        }

        public override bool Equals(object obj)
        {
            bool result = false;

            LogModel model = obj as LogModel;

            if (model != null)
            {
                result = Name.Equals(model.Name);
            }

            return result;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}
