using System;

namespace Shared.Utility
{
    [Serializable]
    public abstract class PriceConverter
    {
        public double _ticSize;
        public double _multiplier;
        public string _displayFormat;
        public double _displayMultipler;
        public string _product;

        public PriceConverter(double ticSize, double displayMultipler)
        {
            this._ticSize = ticSize;
            this._displayMultipler = displayMultipler;
        }

        public PriceConverter(double ticSize, double displayMultipler, string displayFormat)
        {
            this._ticSize = ticSize;
            this._displayMultipler = displayMultipler;
            this._displayFormat = displayFormat;
        }

        public virtual string displayPrice(double priceToFormat)
        {
            int whole = (int)Math.Truncate(priceToFormat);

            priceToFormat -= whole;
            int tick = (int)Math.Truncate(priceToFormat * 32);

            priceToFormat = (priceToFormat * 32) - tick;
            int fraction = (int)Math.Truncate(priceToFormat * 4);

            string display = whole.ToString();
            if (tick < 10)
            {
                display = display + "0" + tick.ToString();
            }
            else
            {
                display = display + tick.ToString();
            }
            switch (fraction)
            {
                case 0:
                    display = display + "0";
                    break;

                case 1:
                    display = display + "2";
                    break;

                case 2:
                    display = display + "5";
                    break;

                case 3:
                    display = display + "7";
                    break;
            }
            return display;
        }
        public abstract string orderPrice(double priceToFormat,double priceAdjustment);
        public abstract string tickUp(string priceToFormat);
        public abstract string tickDown(string priceToFormat);
        public abstract string displayNetChng(double priceToFormat);
    }

    public class euroQuarterTickConvert : PriceConverter
    {
        public euroQuarterTickConvert(double ticSize, double displayMultipler)
            : base(ticSize, displayMultipler) { }

        public override string displayPrice(double priceToFormat)
        {
            string returnValue;
            returnValue = String.Format("{0:0.00}", priceToFormat * this._displayMultipler);
            return returnValue;
        }

        public override string orderPrice(double priceToFormat, double priceAdjustment)
        {
            string returnValue;
            returnValue = (priceToFormat / this._displayMultipler).ToString();
            return returnValue;
        }

        public override string tickUp(string priceToFormat)
        {
            return (Convert.ToDouble(priceToFormat) + this._ticSize).ToString();
        }

        public override string tickDown(string priceToFormat)
        {
            return (Convert.ToDouble(priceToFormat) - this._ticSize).ToString();
        }

        public override string displayNetChng(double priceToFormat)
        {
            if (priceToFormat > 0)
                return "+" + string.Format("{0:0.0#}", priceToFormat);
            else
                return string.Format("{0:0.0#}", priceToFormat);
        }
    }

    public class euroRegularFutureConvert : PriceConverter
    {

        public euroRegularFutureConvert(double ticSize, double displayMultipler)
            : base(ticSize, displayMultipler) { }

        public override string displayPrice(double priceToFormat)
        {
            string returnValue = String.Format("{0:0.0#}", priceToFormat * this._displayMultipler);
            return returnValue;
        }

        public override string orderPrice(double priceToFormat, double priceAdjustment)
        {
            string returnValue = (priceToFormat / this._displayMultipler).ToString();
            return returnValue;
        }

        public override string tickUp(string priceToFormat)
        {
            return (Convert.ToDouble(priceToFormat) + this._ticSize).ToString();
        }

        public override string tickDown(string priceToFormat)
        {
            return (Convert.ToDouble(priceToFormat) - this._ticSize).ToString();
        }
        public override string displayNetChng(double priceToFormat)
        {
            if (priceToFormat > 0)
                return "+" + string.Format("{0:0.0#}", priceToFormat);
            else
                return string.Format("{0:0.0#}", priceToFormat);
        }
    }

    public class euroPacksAndBundles : PriceConverter
    {

        public euroPacksAndBundles(double ticSize, double displayMultipler)
            : base(ticSize, displayMultipler) { }

        public override string displayPrice(double priceToFormat)
        {
            string returnValue = String.Format("{0:0.00}", priceToFormat * this._displayMultipler);
            return returnValue;
        }

        public override string orderPrice(double priceToFormat, double priceAdjustment)
        {
            string returnValue = priceToFormat.ToString();
            return returnValue;
        }

        public override string tickUp(string priceToFormat)
        {
            return (Convert.ToDouble(priceToFormat) + this._ticSize).ToString();
        }

        public override string tickDown(string priceToFormat)
        {
            return (Convert.ToDouble(priceToFormat) - this._ticSize).ToString();
        }

        public override string displayNetChng(double priceToFormat)
        {
            if (priceToFormat > 0)
                return "+" + string.Format("{0:0.0#}", priceToFormat);
            else
                return string.Format("{0:0.0#}", priceToFormat);
        }
    }

    public class thirtyYearSpreadConvert : PriceConverter
    {
        public thirtyYearSpreadConvert(double ticSize, double displayMultipler)
            : base(ticSize, displayMultipler) { }

        public override string orderPrice(double priceToFormat,double priceAdjustment)
        {
            //string returnValue;
            //string sPriceToFormat = priceToFormat.ToString();
            //double ppx = Convert.ToDouble(sPriceToFormat.Substring(3, sPriceToFormat.Length - 3));
            //ppx =  ppx  / 32;

            //int dLen = sPriceToFormat.Length - 3; 
            //double dRet = Convert.ToDouble(sPriceToFormat.Substring(0, dLen)) + ppx;


            //returnValue = dRet.ToString();
            //return returnValue;
            string returnValue;
            string sPriceToFormat = priceToFormat.ToString();
           
            //double ppx = Convert.ToDouble(sPriceToFormat.GetLast(3));
            double ppx = 0;
            if (sPriceToFormat.Length < 3)
                ppx = Convert.ToDouble(sPriceToFormat.GetLast(sPriceToFormat.Length));
            else
                ppx = Convert.ToDouble(sPriceToFormat.GetLast(3));

            ppx = (((ppx + priceAdjustment) * 2) / 10) / 64;

            //int dLen = sPriceToFormat.Length - 3;
            //double dRet = Convert.ToDouble(sPriceToFormat.Substring(0, dLen)) + ppx;

            double dRet = 0;
            if (sPriceToFormat.Length > 3)
            {
                int dLen = sPriceToFormat.Length - 3;
                dRet = Convert.ToDouble(sPriceToFormat.Substring(0, dLen)) + ppx;
            }
            else
            {
                dRet = ppx;
            }

            returnValue = dRet.ToString();
            return returnValue;
        }

        public override string tickUp(string priceToFormat)
        {
            string returnValue;
            if (priceToFormat.Substring(3, priceToFormat.Length - 3) == "31.5")
            {
                returnValue = (Convert.ToDouble(priceToFormat) + _ticSize + 68).ToString();
            }
            else
            {
                returnValue = (Convert.ToDouble(priceToFormat) + _ticSize).ToString();
            }
            return returnValue;
        }

        public override string tickDown(string priceToFormat)
        {
            string returnValue;
            if (priceToFormat.Substring(3, priceToFormat.Length - 3) == "00.0")
            {
                returnValue = (Convert.ToDouble(priceToFormat) - (_ticSize + 68)).ToString();
            }
            else
            {
                returnValue = (Convert.ToDouble(priceToFormat) - (_ticSize)).ToString();
            }
            return returnValue;
        }

        public override string displayNetChng(double priceToFormat)
        {
            string returnValue;
            double retValue = (priceToFormat - Math.Truncate(priceToFormat)) * 64;
            double dValue = (retValue / 2) * 10;

            returnValue = String.Format("{0:0}", dValue);


            if (dValue > 0 && returnValue.Length < 3)
            {
                if (returnValue.Length == 2)
                    returnValue = "0" + returnValue;
                else if (returnValue.Length == 1)
                    returnValue = "00" + returnValue;
            }
            else if (dValue < 0 && returnValue.Length < 4)
            {
                if (returnValue.Length == 3)
                    returnValue = returnValue.Insert(1, "0");
                else if (returnValue.Length == 2)
                    returnValue = returnValue = returnValue.Insert(1, "00");
            }


            if (Math.Truncate(priceToFormat) != 0)
                returnValue = Math.Truncate(priceToFormat).ToString() + returnValue;
            if (priceToFormat > 0)
                returnValue = "+" + returnValue;
            else if (priceToFormat < 0 && returnValue.IndexOf("-") < 0)
                returnValue = "-" + returnValue;

            return returnValue;
            //if (priceToFormat > 0)
            //    return "+" + string.Format("{0:0.0#}", priceToFormat);
            //else
            //    return string.Format("{0:0.0#}", priceToFormat);
        }
    }

    public class tenYearFutureConvert : PriceConverter
    {
        public tenYearFutureConvert(double ticSize, double displayMultipler)
            : base(ticSize, displayMultipler) { }

        public override string orderPrice(double priceToFormat, double priceAdjustment)
        {
            string returnValue;
            string sPriceToFormat = priceToFormat.ToString();
            //double ppx = Convert.ToDouble(sPriceToFormat.GetLast(3));
            double ppx = 0;
            if (sPriceToFormat.Length < 3)
                ppx = Convert.ToDouble(sPriceToFormat.GetLast(sPriceToFormat.Length));
            else
                ppx = Convert.ToDouble(sPriceToFormat.GetLast(3));

            ppx = (((ppx + priceAdjustment) * 2) / 10) / 64;

            //int dLen = sPriceToFormat.Length - 3; 
            //double dRet = Convert.ToDouble(sPriceToFormat.Substring(0, dLen)) + ppx;
            double dRet = 0;
            if (sPriceToFormat.Length > 3)
            {
                int dLen = sPriceToFormat.Length - 3;
                dRet = Convert.ToDouble(sPriceToFormat.Substring(0, dLen)) + ppx;
            }
            else
            {
                dRet = ppx;
            }

            returnValue = dRet.ToString();
            return returnValue;
        }

        public override string tickUp(string priceToFormat)
        {
            string returnValue;
            if (priceToFormat.Substring(3, priceToFormat.Length - 3) == "31.5")
            {
                returnValue = (Convert.ToDouble(priceToFormat) + _ticSize + 68).ToString();
            }
            else
            {
                returnValue = (Convert.ToDouble(priceToFormat) + _ticSize).ToString();
            }
            return returnValue;
        }

        public override string tickDown(string priceToFormat)
        {
            string returnValue;
            if (priceToFormat.Substring(3, priceToFormat.Length - 3) == "00.0")
            {
                returnValue = (Convert.ToDouble(priceToFormat) - (_ticSize + 68)).ToString();
            }
            else
            {
                returnValue = (Convert.ToDouble(priceToFormat) - (_ticSize)).ToString();
            }
            return returnValue;
        }

        public override string displayNetChng(double priceToFormat)
        {
            string returnValue;
            double retValue = (priceToFormat - Math.Truncate(priceToFormat)) * 64;
            double dValue = (retValue / 2) * 10;

            returnValue = String.Format("{0:0}", dValue);


            if (dValue > 0 && returnValue.Length < 3)
            {
                if( returnValue.Length == 2)
                    returnValue = "0" + returnValue;
                else if( returnValue.Length == 1)
                    returnValue = "00" + returnValue;
            }
            else if (dValue < 0 && returnValue.Length < 4)
            {
                if (returnValue.Length == 3)
                    returnValue = returnValue.Insert(1, "0");
                else if (returnValue.Length == 2)
                    returnValue = returnValue = returnValue.Insert(1, "00");
            }
            

            if( Math.Truncate(priceToFormat) != 0)
                returnValue = Math.Truncate(priceToFormat).ToString() + returnValue;
            if (priceToFormat > 0)
                returnValue = "+" + returnValue;
            else if (priceToFormat < 0 && returnValue.IndexOf("-") < 0)
                returnValue = "-" + returnValue;

            return returnValue;
        }
    }

    public class tenYearSpreadConvert : PriceConverter
    {
        public tenYearSpreadConvert(double ticSize, double displayMultipler)
            : base(ticSize, displayMultipler) { }



        public override string displayPrice(double priceToFormat)
        {
            string returnValue;
            if (priceToFormat >= 1.0)
            {
                //double retValue = (priceToFormat - Math.Floor(priceToFormat)) * 32;
                //double dValue = (retValue / 10);

                //returnValue = String.Format("{0:0}", dValue);

                returnValue = String.Format("{0:0.000}", ((priceToFormat - Math.Floor(priceToFormat)) * 32) * 10);
                returnValue = Math.Floor(priceToFormat).ToString() + returnValue.Substring(1, returnValue.Length - 1);
            }
            else
            {
                returnValue = (((priceToFormat - Math.Floor(priceToFormat)) * 32) / 100).ToString();
            }
            return returnValue;
        }

        public override string orderPrice(double priceToFormat, double priceAdjustment)
        {
            string returnValue;
            double tprice = priceToFormat;
            double pprice = 0.0;
            if (tprice < 1)
            {
                pprice = Math.Floor(tprice) + (tprice * 100) / 32;
            }
            else
            {
                pprice = Math.Floor(tprice) + ((tprice - Math.Floor(tprice)) / 32) * 10;
            }
            returnValue = pprice.ToString();
            return returnValue;
        }

        public override string tickUp(string priceToFormat)
        {
            string returnValue;
            int numOfChars = priceToFormat.Length - (priceToFormat.IndexOf(".") + 1);
            if (priceToFormat.Substring(priceToFormat.IndexOf(".") + 1, numOfChars) == "315" || priceToFormat.Substring(priceToFormat.IndexOf(".") + 1, numOfChars) == "3175")
            {
                returnValue = (Convert.ToDouble(priceToFormat) + (_ticSize + .680)).ToString();
            }
            else
            {
                returnValue = (Convert.ToDouble(priceToFormat) + _ticSize).ToString();
            }

            switch (returnValue.Length)
            {
                case 1:
                    returnValue = returnValue + ".0000";
                    break;
                case 3:
                    returnValue = returnValue + "000";
                    break;
                case 4:
                    returnValue = returnValue + "00";
                    break;
                case 5:
                    returnValue = returnValue + "0";
                    break;
                default:
                    break;
            }

            return returnValue;
        }

        public override string tickDown(string priceToFormat)
        {
            string returnValue;
            int numOfChars = priceToFormat.Length - (priceToFormat.IndexOf(".") + 1);
            if (priceToFormat.Substring(priceToFormat.IndexOf(".") + 1, numOfChars) == "000" || priceToFormat.Substring(priceToFormat.IndexOf(".") + 1, numOfChars) == "0000")
            {

                returnValue = (Convert.ToDouble(priceToFormat) - (_ticSize + .680)).ToString();
            }
            else
            {
                returnValue = (Convert.ToDouble(priceToFormat) - (_ticSize)).ToString();
            }

            switch (returnValue.Length)
            {
                case 1:
                    returnValue = returnValue + ".0000";
                    break;
                case 3:
                    returnValue = returnValue + "000";
                    break;
                case 4:
                    returnValue = returnValue + "00";
                    break;
                case 5:
                    returnValue = returnValue + "0";
                    break;
                default:
                    break;
            }

            return returnValue;
        }

        public override string displayNetChng(double priceToFormat)
        {
            if (priceToFormat > 0)
                return "+" + string.Format("{0:0.0#}", priceToFormat);
            else
                return string.Format("{0:0.0#}", priceToFormat);
        }
    }


    public class fiveYearFutureConvert : PriceConverter
    {
        public fiveYearFutureConvert(double ticSize, double displayMultipler)
            : base(ticSize, displayMultipler) { }

        public override string orderPrice(double priceToFormat, double priceAdjustment)
        {
            string returnValue;
            string sPriceToFormat = priceToFormat.ToString();
            double ppx;
            if (sPriceToFormat.Length < 3)
                ppx = Convert.ToDouble(sPriceToFormat.GetLast(sPriceToFormat.Length));
            else 
                ppx = Convert.ToDouble(sPriceToFormat.GetLast(3));
            
            //double ppx = Convert.ToDouble(sPriceToFormat.Substring(3, sPriceToFormat.Length - 3));
            ppx = ((ppx + priceAdjustment) * 4) / 128;
            ppx = ppx / 10;

            double dRet = 0;
            if (sPriceToFormat.Length > 3)
            {
                int dLen = sPriceToFormat.Length - 3;
                dRet = Convert.ToDouble(sPriceToFormat.Substring(0, dLen)) + ppx;
            }
            else 
            {
                dRet = ppx;

            }

            returnValue = dRet.ToString();
            return returnValue;

            //string returnValue;
            //string sPriceToFormat = priceToFormat.ToString();
            //string ppx = ((Convert.ToDouble(sPriceToFormat.Substring(3, sPriceToFormat.Length - 3))) / 32).ToString();
            //ppx = ppx.Substring(1, ppx.Length - 1);
            //returnValue = sPriceToFormat.Substring(0, 3) + ppx;
            //return returnValue;


        }

        public override string tickUp(string priceToFormat)
        {
            string returnValue;
            if (priceToFormat.Substring(3, priceToFormat.Length - 3) == "31.5")
            {
                returnValue = (Convert.ToDouble(priceToFormat) + _ticSize + 68).ToString();
            }
            else
            {
                returnValue = (Convert.ToDouble(priceToFormat) + _ticSize).ToString();
            }
            return returnValue;
        }

        public override string tickDown(string priceToFormat)
        {
            string returnValue;
            if (priceToFormat.Substring(3, priceToFormat.Length - 3) == "00.0")
            {
                returnValue = (Convert.ToDouble(priceToFormat) - (_ticSize + 68)).ToString();
            }
            else
            {
                returnValue = (Convert.ToDouble(priceToFormat) - (_ticSize)).ToString();
            }
            return returnValue;
        }

        public override string displayNetChng(double priceToFormat)
        {
            string returnValue;
            double retValue = 0.0;
            double pxFormat = Math.Abs(priceToFormat);
            retValue = (pxFormat - Math.Truncate(pxFormat)) * 128;

            //if (priceToFormat > 1.0)
            //    retValue = (pxFormat - Math.Floor(pxFormat)) * 128;
            //else
            //    retValue =  pxFormat * 128;

            double dValue = (retValue / 4);

            int sigDigit = Convert.ToInt32(Math.Truncate(dValue));
            string finDigit = Convert.ToString(Math.Truncate((dValue - Math.Truncate(dValue)) * 10));
            returnValue = String.Format("{0:0}", sigDigit) + finDigit;


            if (returnValue.Length == 2)
            {
                returnValue = "0" + returnValue;
            }
            //returnValue = Math.Floor(priceToFormat).ToString() + returnValue;


            if (Math.Truncate(priceToFormat) != 0)
                returnValue = Math.Truncate(priceToFormat).ToString() + returnValue;
            if (priceToFormat > 0)
                returnValue = "+" + returnValue;
            else if (priceToFormat < 0 && returnValue.IndexOf("-") < 0)
                returnValue = "-" + returnValue;

            return returnValue;

            //if (priceToFormat > 0)
            //    return "+" + string.Format("{0:0.0#}", priceToFormat);
            //else
            //    return string.Format("{0:0.0#}", priceToFormat);
        }
    }

    public class fiveYearSpreadConvert : PriceConverter
    {
        public fiveYearSpreadConvert(double ticSize, double displayMultipler)
            : base(ticSize, displayMultipler) { }

        public override string displayPrice(double priceToFormat)
        {
            string returnValue;
            if (priceToFormat >= 1.0)
            {
                returnValue = String.Format("{0:0.000}", ((priceToFormat - Math.Floor(priceToFormat)) * 32) / 10);
                returnValue = Math.Floor(priceToFormat).ToString() + returnValue.Substring(1, returnValue.Length - 1);
            }
            else
            {
                returnValue = (((priceToFormat - Math.Floor(priceToFormat)) * 32) / 100).ToString();
            }
            return returnValue;
        }

        public override string orderPrice(double priceToFormat, double priceAdjustment)
        {
            string returnValue;
            double tprice = priceToFormat;
            double pprice = 0.0;
            if (tprice < 1)
            {
                pprice = Math.Floor(tprice) + (tprice * 100) / 32;
            }
            else
            {
                pprice = Math.Floor(tprice) + ((tprice - Math.Floor(tprice)) / 32) * 10;
            }
            returnValue = pprice.ToString();
            return returnValue;
        }

        public override string tickUp(string priceToFormat)
        {
            string returnValue;
            int numOfChars = priceToFormat.Length - (priceToFormat.IndexOf(".") + 1);
            if (priceToFormat.Substring(priceToFormat.IndexOf(".") + 1, numOfChars) == "315" || priceToFormat.Substring(priceToFormat.IndexOf(".") + 1, numOfChars) == "3175")
            {
                returnValue = (Convert.ToDouble(priceToFormat) + (_ticSize + .680)).ToString();
            }
            else
            {
                returnValue = (Convert.ToDouble(priceToFormat) + _ticSize).ToString();
            }

            switch (returnValue.Length)
            {
                case 1:
                    returnValue = returnValue + ".0000";
                    break;
                case 3:
                    returnValue = returnValue + "000";
                    break;
                case 4:
                    returnValue = returnValue + "00";
                    break;
                case 5:
                    returnValue = returnValue + "0";
                    break;
                default:
                    break;
            }

            return returnValue;
        }

        public override string tickDown(string priceToFormat)
        {
            string returnValue;
            int numOfChars = priceToFormat.Length - (priceToFormat.IndexOf(".") + 1);
            if (priceToFormat.Substring(priceToFormat.IndexOf(".") + 1, numOfChars) == "000" || priceToFormat.Substring(priceToFormat.IndexOf(".") + 1, numOfChars) == "0000")
            {

                returnValue = (Convert.ToDouble(priceToFormat) - (_ticSize + .680)).ToString();
            }
            else
            {
                returnValue = (Convert.ToDouble(priceToFormat) - (_ticSize)).ToString();
            }

            switch (returnValue.Length)
            {
                case 1:
                    returnValue = returnValue + ".0000";
                    break;
                case 3:
                    returnValue = returnValue + "000";
                    break;
                case 4:
                    returnValue = returnValue + "00";
                    break;
                case 5:
                    returnValue = returnValue + "0";
                    break;
                default:
                    break;
            }

            return returnValue;
        }

        public override string displayNetChng(double priceToFormat)
        {
            if (priceToFormat > 0)
                return "+" + string.Format("{0:0.0#}", priceToFormat);
            else
                return string.Format("{0:0.0#}", priceToFormat);
        }
    }

    public class qmFutureConvert : PriceConverter
    {
        public qmFutureConvert(double ticSize, double displayMultipler)
            : base(ticSize, displayMultipler) { }

        public override string displayPrice(double priceToFormat)
        {
            string returnValue;
            returnValue = String.Format("{0:0.000}", priceToFormat * _displayMultipler);
            return returnValue;
        }

        public override string orderPrice(double priceToFormat, double priceAdjustment)
        {
            string returnValue;
            returnValue = (priceToFormat / _displayMultipler).ToString();
            return returnValue;
        }

        public override string tickUp(string priceToFormat)
        {
            return (Convert.ToDouble(priceToFormat) + this._ticSize).ToString();
        }

        public override string tickDown(string priceToFormat)
        {
            return (Convert.ToDouble(priceToFormat) - this._ticSize).ToString();
        }

        public override string displayNetChng(double priceToFormat)
        {
            if (priceToFormat > 0)
                return "+" + string.Format("{0:0.0#}", priceToFormat);
            else
                return string.Format("{0:0.0#}", priceToFormat);
        }
    }

    public class clFutureConvert : PriceConverter
    {
        public clFutureConvert(double ticSize, double displayMultipler)
            : base(ticSize, displayMultipler) { }

        public override string displayPrice(double priceToFormat)
        {
            string returnValue;
            returnValue = String.Format("{0:0}", priceToFormat * _displayMultipler);
            return returnValue;
        }

        public override string orderPrice(double priceToFormat, double priceAdjustment)
        {
            string returnValue;
            returnValue = (priceToFormat / _displayMultipler).ToString();
            return returnValue;
        }

        public override string tickUp(string priceToFormat)
        {
            return (Convert.ToDouble(priceToFormat) + this._ticSize).ToString();
        }

        public override string tickDown(string priceToFormat)
        {
            return (Convert.ToDouble(priceToFormat) - this._ticSize).ToString();
        }

        public override string displayNetChng(double priceToFormat)
        {
            if (priceToFormat > 0)
                return "+" + string.Format("{0:0}", priceToFormat);
            else
                return string.Format("{0:0}", priceToFormat);
        }
    }

    public class nikkeiFutureConvert : PriceConverter
    {
        public nikkeiFutureConvert(double ticSize, double displayMultipler)
            : base(ticSize, displayMultipler) { }

        public override string displayPrice(double priceToFormat)
        {
            string returnValue;
            returnValue = String.Format("{0:0}", priceToFormat * _displayMultipler);
            return returnValue;
        }

        public override string orderPrice(double priceToFormat, double priceAdjustment)
        {
            string returnValue;
            returnValue = (priceToFormat / _displayMultipler).ToString();
            return returnValue;
        }

        public override string tickUp(string priceToFormat)
        {
            return (Convert.ToDouble(priceToFormat) + this._ticSize).ToString();
        }

        public override string tickDown(string priceToFormat)
        {
            return (Convert.ToDouble(priceToFormat) - this._ticSize).ToString();
        }

        public override string displayNetChng(double priceToFormat)
        {
            if (priceToFormat > 0)
                return "+" + string.Format("{0:0.0#}", priceToFormat);
            else
                return string.Format("{0:0.0#}", priceToFormat);
        }

    }

    public class zbFutureConvert : PriceConverter
    {
        public zbFutureConvert(double ticSize, double displayMultipler)
            : base(ticSize, displayMultipler) { }

        public override string displayPrice(double priceToFormat)
        {
            string returnValue;
            returnValue = String.Format("{0:0}", (priceToFormat - Math.Floor(priceToFormat)) * 32);

            switch (returnValue.Length)
            {
                case 1:
                    returnValue = "0" + returnValue + "0";
                    break;
                case 2:
                    returnValue = returnValue + "0";
                    break;
                default:
                    break;
            }

            returnValue = Math.Floor(priceToFormat).ToString() + returnValue;
            return returnValue;
        }

        public override string orderPrice(double priceToFormat, double priceAdjustment)
        {

            string returnValue;
            string ppx = "";
            string sPriceToFormat = priceToFormat.ToString();

            ppx = ((Convert.ToDouble(sPriceToFormat.Substring(3, sPriceToFormat.Length - 3)) / 10) / 32).ToString();
            ppx = ppx.Substring(1, ppx.Length - 1);
            returnValue = sPriceToFormat.Substring(0, 3) + ppx;

            return returnValue;
        }

        public override string tickUp(string priceToFormat)
        {
            string returnValue;

            if (priceToFormat.Substring(3, priceToFormat.Length - 3) == "310")
            {
                returnValue = (Convert.ToDouble(priceToFormat) + (_ticSize + 680)).ToString();
            }
            else
            {
                returnValue = (Convert.ToDouble(priceToFormat) + _ticSize).ToString();
            }

            return returnValue;
        }

        public override string tickDown(string priceToFormat)
        {

            string returnValue;

            if (priceToFormat.Substring(3, priceToFormat.Length - 3) == "000")
            {
                returnValue = (Convert.ToDouble(priceToFormat) - (_ticSize + 680)).ToString();
            }
            else
            {
                returnValue = (Convert.ToDouble(priceToFormat) - (_ticSize)).ToString();
            }
            return returnValue;
        }

        public override string displayNetChng(double priceToFormat)
        {
            if (priceToFormat > 0)
                return "+" + string.Format("{0:0.0#}", priceToFormat);
            else
                return string.Format("{0:0.0#}", priceToFormat);
        }
    }

    public class zbSpreadConvert : PriceConverter
    {
        public zbSpreadConvert(double ticSize, double displayMultipler)
            : base(ticSize, displayMultipler) { }

        public override string displayPrice(double priceToFormat)
        {
            string returnValue;
            if (priceToFormat >= 1.0)
            {
                returnValue = String.Format("{0:0.0000}", ((priceToFormat - Math.Floor(priceToFormat)) * 32) / 100);
                returnValue = Math.Floor(priceToFormat).ToString() + returnValue.Substring(1, returnValue.Length - 1);
            }
            else
            {
                returnValue = (((priceToFormat - Math.Floor(priceToFormat)) * 32) / 100).ToString();
            }
            return returnValue;
        }

        public override string orderPrice(double priceToFormat, double priceAdjustment)
        {
            string returnValue;
            double tprice = priceToFormat;
            double pprice = 0.0;
            if (tprice < 1)
            {
                pprice = (tprice * 100) / 32;
            }
            else
            {
                pprice = Math.Floor(tprice) + ((tprice - Math.Floor(tprice)) / 32) * 100;
            }
            returnValue = pprice.ToString();

            return returnValue;
        }

        public override string tickUp(string priceToFormat)
        {
            string returnValue;
            if (priceToFormat.Substring(priceToFormat.IndexOf(".") + 1, 4) == "3175")
            {
                returnValue = (Convert.ToDouble(priceToFormat) + (_ticSize + .6800)).ToString();
            }
            else
            {
                returnValue = (Convert.ToDouble(priceToFormat) + _ticSize).ToString();
            }

            switch (returnValue.Length)
            {
                case 1:
                    returnValue = returnValue + ".0000";
                    break;
                case 3:
                    returnValue = returnValue + "000";
                    break;
                case 4:
                    returnValue = returnValue + "00";
                    break;
                case 5:
                    returnValue = returnValue + "0";
                    break;
                default:
                    break;
            }

            return returnValue;
        }

        public override string tickDown(string priceToFormat)
        {
            string returnValue;

            if (priceToFormat.Substring(priceToFormat.IndexOf(".") + 1, 4) == "0000")
            {

                returnValue = (Convert.ToDouble(priceToFormat) - (_ticSize + .6800)).ToString();
            }
            else
            {
                returnValue = (Convert.ToDouble(priceToFormat) - (_ticSize)).ToString();
            }

            switch (returnValue.Length)
            {
                case 1:
                    returnValue = returnValue + ".0000";
                    break;
                case 3:
                    returnValue = returnValue + "000";
                    break;
                case 4:
                    returnValue = returnValue + "00";
                    break;
                case 5:
                    returnValue = returnValue + "0";
                    break;
                default:
                    break;
            }

            return returnValue;
        }

        public override string displayNetChng(double priceToFormat)
        {
            if (priceToFormat > 0)
                return "+" + string.Format("{0:0.0#}", priceToFormat);
            else
                return string.Format("{0:0.0#}", priceToFormat);
        }

    }

    public class defaultPriceConvert : PriceConverter
    {
        public defaultPriceConvert(double ticSize, double displayMultipler)
            : base(ticSize, displayMultipler) { }

        public override string displayPrice(double priceToFormat)
        {
            string returnValue;
            returnValue = String.Format("{0:0.000}", priceToFormat);
            return returnValue;
        }

        public override string orderPrice(double priceToFormat, double priceAdjustment)
        {
            string returnValue;
            returnValue = String.Format("{0:0.00}", priceToFormat);
            return returnValue;
        }

        public override string tickUp(string priceToFormat)
        {
            return "";
        }

        public override string tickDown(string priceToFormat)
        {
            return "";
        }

        public override string displayNetChng(double priceToFormat)
        {
            if (priceToFormat > 0)
                return "+" + string.Format("{0:0.0#}", priceToFormat);
            else
                return string.Format("{0:0.0#}", priceToFormat);
        }
    }

    public class ESFutureConvert : PriceConverter
    {

        public ESFutureConvert(double ticSize, double displayMultipler)
            : base(ticSize, displayMultipler) { }

        public override string displayPrice(double priceToFormat)
        {
            string returnValue = String.Format("{0:0}", priceToFormat * this._displayMultipler);
            return returnValue;
        }

        public override string orderPrice(double priceToFormat, double priceAdjustment)
        {
            string returnValue = String.Format("{0:0.00}", priceToFormat / this._displayMultipler);
            return returnValue;
        }

        public override string tickUp(string priceToFormat)
        {
            return String.Format("{0:0.00}", Convert.ToDouble(priceToFormat) + this._ticSize);
        }

        public override string tickDown(string priceToFormat)
        {
            return String.Format("{0:0.00}", Convert.ToDouble(priceToFormat) - this._ticSize);
        }

        public override string displayNetChng(double priceToFormat)
        {
            if (priceToFormat > 0)
                return "+" + string.Format("{0:0}", priceToFormat);
            else
                return string.Format("{0:0}", priceToFormat);
        }
    }

    public class ESSpreadConvert : PriceConverter
    {

        public ESSpreadConvert(double ticSize, double displayMultipler)
            : base(ticSize, displayMultipler) { }

        public override string displayPrice(double priceToFormat)
        {
            string returnValue = String.Format("{0:0.00}", priceToFormat * this._displayMultipler);
            return returnValue;
        }

        public override string orderPrice(double priceToFormat, double priceAdjustment)
        {
            string returnValue = String.Format("{0:0.00}", priceToFormat / this._displayMultipler);
            return returnValue;
        }

        public override string tickUp(string priceToFormat)
        {
            return (Convert.ToDouble(priceToFormat) + this._ticSize).ToString();
        }

        public override string tickDown(string priceToFormat)
        {
            return (Convert.ToDouble(priceToFormat) - this._ticSize).ToString();
        }

        public override string displayNetChng(double priceToFormat)
        {
            if (priceToFormat > 0)
                return "+" + string.Format("{0:0}", priceToFormat);
            else
                return string.Format("{0:0}", priceToFormat);
        }
    }

    public class SixJFutureConvert : PriceConverter
    {

        public SixJFutureConvert(double ticSize, double displayMultipler)
            : base(ticSize, displayMultipler) { }

        public override string displayPrice(double priceToFormat)
        {
            string returnValue = String.Format("{0:0.0000}", priceToFormat * this._displayMultipler);
            return returnValue;
        }

        public override string orderPrice(double priceToFormat, double priceAdjustment)
        {
            string returnValue = String.Format("{0:0.000000}", priceToFormat / this._displayMultipler);
            return returnValue;
        }

        public override string tickUp(string priceToFormat)
        {
            return String.Format("{0:0.0000}", Convert.ToDouble(priceToFormat) + this._ticSize);
        }

        public override string tickDown(string priceToFormat)
        {
            return String.Format("{0:0.0000}", Convert.ToDouble(priceToFormat) - this._ticSize);
        }

        public override string displayNetChng(double priceToFormat)
        {
            if (priceToFormat > 0)
                return "+" + string.Format("{0:0.0#}", priceToFormat);
            else
                return string.Format("{0:0.0#}", priceToFormat);
        }
    }

    public class SixJSpreadConvert : PriceConverter
    {

        public SixJSpreadConvert(double ticSize, double displayMultipler)
            : base(ticSize, displayMultipler) { }

        public override string displayPrice(double priceToFormat)
        {
            string returnValue = String.Format("{0:0.0}", priceToFormat * this._displayMultipler);
            return returnValue;
        }

        public override string orderPrice(double priceToFormat, double priceAdjustment)
        {
            string returnValue = String.Format("{0:0.000000}", priceToFormat / this._displayMultipler);
            return returnValue;
        }

        public override string tickUp(string priceToFormat)
        {
            return String.Format("{0:0.0}", Convert.ToDouble(priceToFormat) + this._ticSize);
        }

        public override string tickDown(string priceToFormat)
        {
            return String.Format("{0:0.0}", Convert.ToDouble(priceToFormat) - this._ticSize);
        }

        public override string displayNetChng(double priceToFormat)
        {
            if (priceToFormat > 0)
                return "+" + string.Format("{0:0.0#}", priceToFormat);
            else
                return string.Format("{0:0.0#}", priceToFormat);
        }
    }

    public class ESSpreadConvertDCAT : PriceConverter
    {

        public ESSpreadConvertDCAT(double ticSize, double displayMultipler)
            : base(ticSize, displayMultipler) { }

        public override string displayPrice(double priceToFormat)
        {
            string returnValue = String.Format("{0:0}", priceToFormat * this._displayMultipler);
            return returnValue;
        }

        public override string orderPrice(double priceToFormat, double priceAdjustment)
        {
            string returnValue = String.Format("{0:0.00}", priceToFormat / this._displayMultipler);
            return returnValue;
        }

        public override string tickUp(string priceToFormat)
        {
            return (Convert.ToDouble(priceToFormat) + this._ticSize).ToString();
        }

        public override string tickDown(string priceToFormat)
        {
            return (Convert.ToDouble(priceToFormat) - this._ticSize).ToString();
        }

        public override string displayNetChng(double priceToFormat)
        {
            if (priceToFormat > 0)
                return "+" + string.Format("{0:0.0#}", priceToFormat);
            else
                return string.Format("{0:0.0#}", priceToFormat);
        }
    }
}

