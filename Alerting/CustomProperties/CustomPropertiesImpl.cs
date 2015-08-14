using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Alerting.Alert;

namespace Alerting.CustomProperties
{
    public class CustomPropertiesImpl: ICustomProperties
    {
        private const string XMLFile = "Alerting.dll.config";

        public bool NoValidation { get; set; }
        public bool ValidateAllLoggerInstances{get ; set ; }
        
        public string EmailTo { get; set; }
        public string EmailFrom { get; set; }
        public string EmailSMTPServer { get; set; }
        public string EmailSubject { get; set; }
       // public string EmailBodyTemplate { get; set; }
        public bool TestMode { get; set; }
        public string EmailSubjectPrefix { get; set; }
        public string EmailSubjectSuffix { get; set; }
        public string EmailBodyHeader { get; set; }
        public string EmailBodyFooter { get; set; }


        public int EmailThreashholdCount { get; set; }
        public int EmailThreashholdDuration { get; set; }
        public bool EmailDisabled { get; set; }

        public bool EmailManagerEnabled { get; set; }

        public string EmailErrorMessageBodyTemplate { get; set; }
        public bool UseEmailHelperNullImpl { get; set; }

        public CustomPropertiesImpl()
        {
            Load();
        }

        protected void Load()
        {
//throw new Exception("test");
            // read xml file

            try
            {

                IDictionary<string, string> properties = GetProperties();

                NoValidation = GetBoolProperty(properties, "NoValidation");
                ValidateAllLoggerInstances = GetBoolProperty(properties, "ValidateAllLoggerInstances");

                EmailTo = GetStringProperty(properties, "EmailTo");
                EmailFrom = GetStringProperty(properties, "EmailFrom");
                EmailSMTPServer = GetStringProperty(properties, "EmailSMTPServer");
                EmailSubject = GetStringProperty(properties, "EmailSubject");
                //EmailBodyTemplate = GetStringProperty(properties, "EmailBodyTemplate");

                TestMode = GetBoolProperty(properties, "TestMode");

                EmailSubjectPrefix = GetStringProperty(properties, "EmailSubjectPrefix");
                EmailSubjectSuffix = GetStringProperty(properties, "EmailSubjectSuffix");
                EmailBodyHeader = GetStringProperty(properties, "EmailBodyHeader");
                EmailBodyFooter = GetStringProperty(properties, "EmailBodyFooter");

                EmailThreashholdCount = GetIntProperty(properties, "EmailThreashholdCount");
                EmailThreashholdDuration = GetIntProperty(properties, "EmailThreashholdDuration");
                EmailDisabled = GetBoolProperty(properties, "EmailDisabled");

                EmailManagerEnabled = GetBoolProperty(properties, "EmailManagerEnabled");
                EmailErrorMessageBodyTemplate = GetStringProperty(properties, "EmailErrorMessageBodyTemplate");
                UseEmailHelperNullImpl = GetBoolProperty(properties, "UseEmailHelperNullImpl");
            }
            catch (Exception e)
            {

             LoggerHome.LogToWindowsEventLog("Failed to load Custom properties: "+ e.ToString());
            }

        }

        protected bool GetBoolProperty(IDictionary<string, string> properties, string name)
        {
            bool boolVal = false;
            try
            {
                bool.TryParse(properties[name], out boolVal);
            }
            catch (Exception e)
            {
              //  LoggerHome.GetLogger(this).Debug("Property: "+ name+" not found. Value set to [false]",e );
                LoggerHome.LogToWindowsEventLog("Property: " + name + " not found. Value set to [false] "+ e.ToString());

            }

            return boolVal;
        }

        protected int GetIntProperty(IDictionary<string, string> properties, string name)
        {
            int  intVal = 0;

            try
            {
                int.TryParse(properties[name], out intVal);
            }
            catch (Exception e)
            {
                //  LoggerHome.GetLogger(this).Debug("Property: "+ name+" not found. Value set to [false]",e );
                LoggerHome.LogToWindowsEventLog("Property: " + name + " not found. Value set to [0] " + e.ToString());

            }

            return intVal;
        }

        protected string GetStringProperty(IDictionary<string, string> properties, string name)
        {
            string result = "";

            try
            {

                if (properties[name] != null)
                {
                    result = properties[name];
                }
                else
                {
                  //  LoggerHome.GetLogger(this).Debug("Property: " + name + " not found. Value set to []");
                    LoggerHome.LogToWindowsEventLog("Property: " + name + " not found. Value set to [] ");
                }
            }
            catch (Exception e)
            {
              //  LoggerHome.GetLogger(this).Debug("Property: " + name + " not found. Value set to []", e);
                LoggerHome.LogToWindowsEventLog("Property: " + name + " not found. Value set to [] " + e.ToString());
            }

            return result;
        }

        static public string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = uri.ToString().Substring(0,uri.ToString().LastIndexOf("/")+1);

               // string path = Uri.UnescapeDataString(uri.Path);
               // return Path.GetDirectoryName(path);
                return path;
            }
        }

        protected IDictionary<string, string> GetProperties()
        {
            
            //string configFile = AssemblyDirectory+"\\"+ XMLFile;

            string path = LoggerHome.ExePath;
            if (string.IsNullOrEmpty(path))
            {
                path = AssemblyDirectory;
            }

            //string configFile = path + "\\" + XMLFile;
            string configFile = path + XMLFile;

            IDictionary<string, string> names = new Dictionary<string, string>();

            XmlDocument xmlDoc = new XmlDocument();

            if (ValidateFile(configFile) == true)
            {
                xmlDoc.Load(configFile);
                const string elementToSearchFor = "setting";

                XmlNodeList nodes = xmlDoc.GetElementsByTagName(elementToSearchFor);
                PopulateEntityList(names, nodes);
            }
            else
            {
                //LoggerHome.GetLogger(this).Error("Could not find (or empty file) Import Config file:" + configFile);
                LoggerHome.LogToWindowsEventLog("Could not find (or empty file) Import Config file:" + configFile);
            }

            return names;
        }

        protected void PopulateEntityList(IDictionary<string, string> names, XmlNodeList nodes)
        {

            foreach (XmlNode node in nodes)
            {
                if (node != null && node.Attributes != null)
                {
                    string name = node.Attributes["name"].Value;
                    string value = node.InnerText;
                    
                    
                    names.Add(name,value);
                }
                else
                {
                    //LoggerHome.GetLogger(this).Error("XmlDocument.GetElementsByTagName returned an item in list that is null or a null attribute. Check that XML is well formed.");
                    LoggerHome.LogToWindowsEventLog("XmlDocument.GetElementsByTagName returned an item in list that is null or a null attribute. Check that XML is well formed.");
                }
            }
        }


        protected bool ValidateFile(string fileName)
        {
            bool result = true;
            // maybe add extra logging here?
            // or let calling method handle exception and action
            //if (!System.IO.File.Exists(fileName))
            if (!RemoteFileExists(fileName))
            {
                //   throw new System.IO.FileNotFoundException("The rejects file specified could not be found.", fileName);
                result = false;
            }
            //else
            //{
            //    FileInfo fileInfo = new FileInfo(fileName);
            //    if (fileInfo.Length < 1)
            //    {
            //        // empty file
            //        result = false;
            //    }
            //}
            return result;
        }

        ///
        /// Checks the file exists or not.
        ///
        /// The URL of the remote file.
        /// True : If the file exits, False if file not exists
        private bool RemoteFileExists(string url)
        {
            bool result = false;
            using (WebClient client = new WebClient())
            {
                try
                {
                    Stream stream = client.OpenRead(url);
                    if (stream != null)
                    {
                        result = true;
                    }
                    else
                    {
                        result = false;
                    }
                }
                catch
                {
                    //Any exception will returns false.
                    result = false;
                }
            }
            return result;
        }
    }
}
