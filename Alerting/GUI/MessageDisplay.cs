using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Alerting.GUI
{
    public partial class MessageDisplay : Form
    {
        public MessageDisplay()
        {
            InitializeComponent();
        }

        public MessageDisplay(String message, String caption, MessageBoxIcon messageBoxIcon): this()
        {
            textBoxMessage.Text = message;
            this.Text = caption;

            pictureBox1.Image =  GetIcon(messageBoxIcon);
        }

        public static void Show(String message, String caption, MessageBoxIcon messageBoxIcon)
        {
            MessageDisplay messageDisplay = new MessageDisplay(message, caption, messageBoxIcon);
            messageDisplay.ShowDialog();
        }

        public Image GetIcon(MessageBoxIcon messageBoxIcon)
        {
            Image result = null;

            if (messageBoxIcon == MessageBoxIcon.Warning)
            {
                result = System.Drawing.SystemIcons.Warning.ToBitmap();
            }
            else if (messageBoxIcon == MessageBoxIcon.Stop)
            {
                result = System.Drawing.SystemIcons.Hand.ToBitmap();
            }
            else if (messageBoxIcon == MessageBoxIcon.Question)
            {
                result = System.Drawing.SystemIcons.Question.ToBitmap();
            }
            else if (messageBoxIcon == MessageBoxIcon.Information)
            {
                result = System.Drawing.SystemIcons.Information.ToBitmap();
            }
            else if (messageBoxIcon == MessageBoxIcon.Hand)
            {
                result = System.Drawing.SystemIcons.Hand.ToBitmap();
            }
            else if (messageBoxIcon == MessageBoxIcon.Exclamation)
            {
                result = System.Drawing.SystemIcons.Exclamation.ToBitmap();
            }
            else if (messageBoxIcon == MessageBoxIcon.Error)
            {
                result = System.Drawing.SystemIcons.Error.ToBitmap();
            }
            else if (messageBoxIcon == MessageBoxIcon.Asterisk)
            {
                result = System.Drawing.SystemIcons.Asterisk.ToBitmap();
            }

            return result;

        }

        //FormatMessage(message, exception), "Notification: "+severity, MessageBoxButtons.OK, GetMessageBoxIcon(severity)
        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }
        
        private void buttonLogFiles_Click(object sender, EventArgs e)
        {
            LogSelectorForm logSelectorForm  = new LogSelectorForm();
            logSelectorForm.Show();

            
        }
    }
}
