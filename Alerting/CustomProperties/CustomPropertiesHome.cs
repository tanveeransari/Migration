using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alerting.CustomProperties
{
    public class CustomPropertiesHome
    {
        private static ICustomProperties customProperties;

        static public ICustomProperties GetCustomProperties()
        {
            if (customProperties == null)
            {
                customProperties = new CustomPropertiesImpl();
 
            }
            return customProperties;
        }
    }
}
