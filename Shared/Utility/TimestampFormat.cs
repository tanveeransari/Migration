using Dapfor.Net.Data;
using Dapfor.Net.Formats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Utility
{
    public class TimestampFormat : IFormat
    {
        public string Format(IDataField dataField)
        {
            string formattedString = string.Format("{0:MM/dd/yy HH:mm:ss:fff}", dataField.Value);
            return formattedString;
        }

        public bool CanParse(string text, IDataField dataField)
        {
            return false;
        }

        public void Parse(string text, IDataField dataField)
        {
        }
    }
}
