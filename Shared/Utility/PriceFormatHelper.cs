using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization;
using Shared.InstrumentModels;

namespace Shared.Utility
{
    
    public class PriceFormatHelper
    {
        private Dictionary<string, List<PriceFormat>> cache = new Dictionary<string, List<PriceFormat>>();
        private PriceFormat defaultPriceFormat;

        private static PriceFormatHelper _instance;

        public static PriceFormatHelper GetInstance()
        {
            if( _instance == null)
                _instance = new PriceFormatHelper();

            return _instance;
        }

        public void LoadPriceFormats()
        {

            //TODO: TANVEER/TA Mock this instead---------------

            //this.cache = DBHelper.getInstance().GetPriceFormats(out this.defaultPriceFormat);

/*            StreamReader sr = new StreamReader("priceFormatConfig.csv");
            string line = sr.ReadLine();
            line = sr.ReadLine();
            while (line != null)
            {
                string[] tStr = line.Split(',');
                if (tStr.Length > 3)
                {
                    PriceFormat pFormat = new PriceFormat();
                    pFormat.productGroup = tStr[0];
                    pFormat.productType = tStr[1];
                    pFormat.tickSize = Convert.ToDouble(tStr[2]);
                    pFormat.displayMultiplier = Convert.ToDouble(tStr[3]);
                    pFormat.numberOfDecimals = Convert.ToInt32(tStr[4]);
                    pFormat.priceConfigType = Convert.ToInt32(tStr[5]);

                    if (pFormat.productGroup == "Default")
                        this.defaultPriceFormat = pFormat;
                    else
                    {
                        if (this.cache.ContainsKey(pFormat.productGroup))
                        {
                            this.cache[pFormat.productGroup].Add(pFormat);
                        }
                        else
                        {
                            this.cache.Add(pFormat.productGroup, new List<PriceFormat>() { pFormat });
                        }
                    }

                    line = sr.ReadLine();
                }
            }

            sr.Close();
 * */
        }

        public PriceFormat GetPriceFormatForInstrument(InstrumentShared instrument)
        {
            try
            {
                if (instrument != null)
                {
                    if (this.cache.ContainsKey(instrument.productCode))
                    {
                        foreach (PriceFormat pFormat in this.cache[instrument.productCode])
                        {
                            if (instrument.strategyType == pFormat.productType)
                                return pFormat;
                        }
                        foreach(PriceFormat pFormat in this.cache[instrument.productCode])
                        {
                            if (pFormat.productType == "Outright")
                                return pFormat;
                        }
                    }
                }
            }
            catch (Exception err)
            {
                Alerting.Alert.LoggerHome.GetLogger(this).Error("GetPriceFormatForInstrument", err);
                
            }

            return this.defaultPriceFormat;
        }

    }

    [Serializable]
    public class PriceFormat
    {
        public string productGroup;
        public string productType;
        public double tickSize;
        public double displayMultiplier;
        public int numberOfDecimals;
        public int priceConfigType;

        public PriceFormat()
        { }
        
        public PriceFormat(string pProductGroup,string pProductType,double pTickSize,double pDisplayMultiplier,int pNumDecimals,int pConfigType)
        {
            this.productGroup = pProductGroup;
            this.productType = pProductType;
            this.tickSize = pTickSize;
            this.displayMultiplier = pDisplayMultiplier;
            this.numberOfDecimals = pNumDecimals;
            this.priceConfigType = pConfigType;
        }
    }
}
