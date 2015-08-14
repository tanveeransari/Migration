/*
using System;
using System.Collections.Generic;
using Alerting.Alert;
using MySql.Data.MySqlClient;
using Oro;
using Shared.InstrumentModels;
using Shared.Services;
using Shared.UserData;

namespace Shared.Utility
{
    internal class DBHelper
    {
        private static DBHelper _dbHelper;
        private string _connectionString;
        private MySqlConnection _connection;

        public static DBHelper getInstance()
        {
            if (_dbHelper == null)
            {
#if DEBUG
                //_dbHelper = new DBHelper(GlobalData.appConfig.DatabaseServer, "eldorado", "admin", "12345678");
                _dbHelper = new DBHelper(GlobalData.appConfig.DatabaseServer, "eldorado", "frontend", "12345678");
#else
                _dbHelper = new DBHelper(GlobalData.appConfig.DatabaseServer, "eldorado", "frontend", "12345678");
#endif
            }

            return _dbHelper;
        }

        private DBHelper(string server, string database, string uid, string password)
        {
            this._connectionString = "SERVER=" + server + ";" + "DATABASE=" + database + ";" + "UID=" + uid + ";" + "PASSWORD=" + password + ";";
            this._connection = new MySqlConnection(this._connectionString);
        }

        private bool OpenConnection()
        {
            try
            {
                if (this._connection.State == System.Data.ConnectionState.Closed)
                    this._connection.Open();
                return true;
            }
            catch (Exception err)
            {
                LoggerHome.GetLogger(this).Error("Error in OpenConnection " + err.ToString());
            }
            return false;
        }

        private bool CloseConnection()
        {
            try
            {
                this._connection.Close();
                return true;
            }
            catch (Exception err)
            {
                LoggerHome.GetLogger(this).Error("Error in CloseConnection " + err.ToString() + "," + err.StackTrace);
            }
            return false;
        }

        public IList<Product> GetProductData()
        {
            List<Product> productList = new List<Product>();
            try
            {
                string query = "returnAllProducts";

                if (this.OpenConnection() == true)
                {
                    MySqlCommand sCmd = new MySqlCommand(query, this._connection);

                    MySqlDataReader dataReader = sCmd.ExecuteReader();
                    while (dataReader.Read())
                    {
                        string productName = dataReader.GetString(dataReader.GetOrdinal("productSymbol"));
                        string productCode = dataReader.GetString(dataReader.GetOrdinal("productCode"));
                        string productType = dataReader.GetString(dataReader.GetOrdinal("productType"));

                        if (productType != "Future")
                            continue;

                        Product pProduct = new Product();
                        pProduct.Id = productCode;
                        pProduct.Type = productType == "Future" ? "F" : "S";

                        pProduct.Name = productName;
                        productList.Add(pProduct);
                    }
                    this.CloseConnection();
                }
            }
            catch (Exception err)
            {
                LoggerHome.GetLogger(this).Error("Error in Get Product Data " + err.ToString());
                this.CloseConnection();
            }

            return productList;
        }

        public List<RawInstrument> GetRawInstruments()
        {
            List<RawInstrument> instList = new List<RawInstrument>();
            try
            {
                string query = "returnAllProductDetail";

                if (this.OpenConnection() == true)
                {
                    MySqlCommand sCmd = new MySqlCommand(query, this._connection);

                    MySqlDataReader dataReader = sCmd.ExecuteReader();
                    while (dataReader.Read())
                    {
                        string productName = dataReader.GetString(dataReader.GetOrdinal("securityDescription"));
                        string productCode = dataReader.GetString(dataReader.GetOrdinal("symbol"));
                        string productType = dataReader.GetString(dataReader.GetOrdinal("productType"));
                        string strategyType = dataReader.GetString(dataReader.GetOrdinal("strategyType"));
                        if (strategyType == "SP")
                            strategyType = "Calendar";

                        string exchange = dataReader.GetString(dataReader.GetOrdinal( "securityExchange")).Substring(1);
                        DateTime productExpiration = dataReader.GetDateTime(dataReader.GetOrdinal("productExpiration"));
                        string productFamily = dataReader.GetString(dataReader.GetOrdinal("productComplex"));

                        double minPxIncrement = Convert.ToDouble(dataReader.GetValue(dataReader.GetOrdinal("minPriceIncrement")));
                        double tickValue = Convert.ToDouble(dataReader.GetValue(dataReader.GetOrdinal("tickValue")));
                        double highLimitPx = Convert.ToDouble(dataReader.GetValue(dataReader.GetOrdinal("highLimitPrice")));
                        double lowLimitPx = Convert.ToDouble(dataReader.GetValue(dataReader.GetOrdinal("lowLimitPrice")));

                        int securityID = Convert.ToInt32(dataReader.GetValue(dataReader.GetOrdinal("securityID")));
                        double pxConvertFactor = Convert.ToDouble(dataReader.GetValue(dataReader.GetOrdinal("orderPriceConvertFactor")));
                        string productGroup = dataReader.GetString(dataReader.GetOrdinal("productGroup"));

                        RawInstrument rData = new RawInstrument(securityID, productName, productCode, productType, strategyType,
                                                exchange, productFamily, productExpiration, minPxIncrement, tickValue,
                                                highLimitPx, lowLimitPx, pxConvertFactor,productGroup);

                        instList.Add(rData);
                    }
                    this.CloseConnection();
                }
            }
            catch (Exception err)
            {
                LoggerHome.GetLogger(this).Error("Error in GetRawInstruments " + err.ToString());
                this.CloseConnection();
            }

            return instList;
        }

        public Dictionary<string, List<Tuple<string, int>>> GetSpreadLegs(out Dictionary<string,List<string>> symbolToSpreadMap)
        {
            Dictionary<string, List<Tuple<string, int>>> retList = new Dictionary<string, List<Tuple<string, int>>>();
            symbolToSpreadMap = new Dictionary<string, List<string>>();
            try
            {
                string query = "returnAllProductLegs";

                if (this.OpenConnection() == true)
                {
                    MySqlCommand sCmd = new MySqlCommand(query, this._connection);
                    
                    MySqlDataReader dataReader = sCmd.ExecuteReader();
                    while (dataReader.Read())
                    {
                        string mainProduct = dataReader.GetString(dataReader.GetOrdinal("mainProduct"));
                        string legProduct = dataReader.GetString(dataReader.GetOrdinal("legProduct"));
                        int legRatio = Convert.ToInt32(dataReader.GetValue(dataReader.GetOrdinal("legRatio")));
                        int legSide = Convert.ToInt32(dataReader.GetValue(dataReader.GetOrdinal("legSide")));

                        if (legSide == 2)
                            legSide = -1;
                        legRatio = legRatio * legSide;

                        if (retList.ContainsKey(mainProduct))
                        {
                            List<Tuple<string, int>> pList = retList[mainProduct];
                            pList.Add(Tuple.Create<string,int>(legProduct,legRatio));
                        }
                        else
                        {
                            List<Tuple<string, int>> pList = new List<Tuple<string, int>>();
                            pList.Add(Tuple.Create<string,int>(legProduct,legRatio));
                            retList.Add(mainProduct, pList);
                        }

                        if (symbolToSpreadMap.ContainsKey(legProduct))
                        {
                            symbolToSpreadMap[legProduct].Add(mainProduct);
                        }
                        else
                        {
                            List<string> sList = new List<string>();
                            sList.Add(mainProduct);
                            symbolToSpreadMap[legProduct] = sList;
                        }
                    }

                    this.CloseConnection();

                    foreach(var list in retList.Values)
                    {
                        list.TrimExcess();
                    }
                }
            }
            catch (Exception err)
            {
                LoggerHome.GetLogger(this).Error("Error in GetSpreadLegs " + err.ToString());
                this.CloseConnection();
            }

            return retList;
        }

        public List<TraderRole> GetTraders(string username)
        {
            List<TraderRole> list = new List<TraderRole>();

            try
            {
                string query = "risk.GetTraderInformationForUser";

                if (this.OpenConnection() == true)
                {
                    MySqlCommand sCmd = new MySqlCommand(query, this._connection);
                    sCmd.CommandType = System.Data.CommandType.StoredProcedure;
                    sCmd.Parameters.AddWithValue("?username", username);
                    MySqlDataReader dataReader = sCmd.ExecuteReader();
                    while (dataReader.Read())
                    {
                        string dTraderID = dataReader.GetString(dataReader.GetOrdinal("traderID"));
                        //string dTag50 = dataReader.GetString(dataReader.GetOrdinal("tag50"));
                        double dCreditLimit = Convert.ToDouble(dataReader.GetValue(dataReader.GetOrdinal("creditLimit")));
                        int dIDTrader = Convert.ToInt32(dataReader.GetValue(dataReader.GetOrdinal("idTrader")));

                        TraderRole newTraderRole = new TraderRole()
                        {
                            dbTraderID = dIDTrader,
                            traderID = dTraderID,
                            //tag50 = dTag50,
                            creditLimit = dCreditLimit
                        };

                        list.Add(newTraderRole);
                    }

                    dataReader.Close();
                    dataReader = null;
                    sCmd = null;
                    this.CloseConnection();
                }
            }
            catch (Exception err)
            {
                LoggerHome.GetLogger(this).Error("Error in GetTraders " + err.ToString());
                this.CloseConnection();
            }
            return list;
        }

        public List<Account> GetAccountForTrader(int traderID)
        {
            List<Account> list = new List<Account>();
            try
            {
                string query = "risk.GetTraderAccounts";
                if (this.OpenConnection() == true)
                {
                    MySqlCommand sCmd = new MySqlCommand(query, this._connection);
                    sCmd.CommandType = System.Data.CommandType.StoredProcedure;
                    sCmd.Parameters.AddWithValue("?traderID", traderID);
                    MySqlDataReader dataReader = sCmd.ExecuteReader();
                    while (dataReader.Read())
                    {
                        int pTraderID = Convert.ToInt32(dataReader.GetValue(dataReader.GetOrdinal("idTrader")));
                        int cAccountID = Convert.ToInt32(dataReader.GetValue(dataReader.GetOrdinal("clearingAccountId")));
                        string sAccount = dataReader.GetString(dataReader.GetOrdinal("clearingAccount"));

                        Account newAccount = new Account()
                        {
                            clearingAccountID = cAccountID,
                            clearingAccount = sAccount,
                            traderID = pTraderID
                        };

                        list.Add(newAccount);
                    }

                    dataReader.Close();
                    dataReader = null;
                    sCmd = null;
                    this.CloseConnection();
                }
            }
            catch (Exception err)
            {
                LoggerHome.GetLogger(this).Error("Error in GetAccountTrader " + err.ToString());
                this.CloseConnection();
            }
            return list;
        }

        public List<TradingLimits> GetLimitsForTrader(string traderID)
        {
            List<TradingLimits> list = new List<TradingLimits>();
            try
            {
                string query = "risk.GetTraderLimits";
                if (this.OpenConnection() == true)
                {
                    MySqlCommand sCmd = new MySqlCommand(query, this._connection);
                    sCmd.CommandType = System.Data.CommandType.StoredProcedure;
                    sCmd.Parameters.AddWithValue("?traderID", traderID);
                    MySqlDataReader dataReader = sCmd.ExecuteReader();
                    while (dataReader.Read())
                    {
                        TradingLimits tLimits = new TradingLimits();

                        tLimits.product = dataReader.GetString(dataReader.GetOrdinal("symbol"));
                        tLimits.productType = dataReader.GetString(dataReader.GetOrdinal("productFamily"));
                        tLimits.maxTheoOutrightPosition = Convert.ToInt32(dataReader.GetValue(dataReader.GetOrdinal("maxTheoreticalOutrightPosition")));
                        tLimits.maxTheoContractInventory = Convert.ToInt32(dataReader.GetValue(dataReader.GetOrdinal("maxTheoreticalContractInventory")));
                        tLimits.maxTheoProductInventory = Convert.ToInt32(dataReader.GetValue(dataReader.GetOrdinal("maxTheoreticalProductInventory")));
                        tLimits.maxClipOutright = Convert.ToInt32(dataReader.GetValue(dataReader.GetOrdinal("maxClipOutright")));
                        tLimits.maxClipSpread = Convert.ToInt32(dataReader.GetValue(dataReader.GetOrdinal("maxClipSpread")));
                        tLimits.workingOutrightDiscount = Convert.ToInt32(dataReader.GetValue(dataReader.GetOrdinal("workingOutrightDiscount")));
                        tLimits.workingOutrightInventoryDiscount = Convert.ToInt32(dataReader.GetValue(dataReader.GetOrdinal("workingOutrightInventoryDiscount")));
                        tLimits.maxOrderQty = Convert.ToInt32(dataReader.GetValue(dataReader.GetOrdinal("maxOrderQuantity")));
                        tLimits.productLimit = Convert.ToInt32(dataReader.GetValue(dataReader.GetOrdinal("productLimit")));

                        list.Add(tLimits);
                    }

                    dataReader.Close();
                    dataReader = null;
                    sCmd = null;
                    this.CloseConnection();
                }
            }
            catch (Exception err)
            {
                LoggerHome.GetLogger(this).Error("Error in Get Limits For Trader " + err.ToString());
                this.CloseConnection();
            }
            return list;
        }

        public List<TradingLimits> GetOverrideProductLimitsForTrader(string traderID)
        {
            List<TradingLimits> list = new List<TradingLimits>();
            try
            {
                string query = "risk.GetTraderProductLimitOverride";
                if (this.OpenConnection() == true)
                {
                    MySqlCommand sCmd = new MySqlCommand(query, this._connection);
                    sCmd.CommandType = System.Data.CommandType.StoredProcedure;
                    sCmd.Parameters.AddWithValue("?traderID", traderID);
                    MySqlDataReader dataReader = sCmd.ExecuteReader();
                    while (dataReader.Read())
                    {
                        TradingLimits tLimits = new TradingLimits();

                        tLimits.product = dataReader.GetString(dataReader.GetOrdinal("symbol"));
                        tLimits.productType = dataReader.GetString(dataReader.GetOrdinal("productFamily"));
                        tLimits.maxTheoOutrightPosition = Convert.ToInt32(dataReader.GetValue(dataReader.GetOrdinal("maxTheoreticalOutrightPosition")));
                        tLimits.maxTheoContractInventory = Convert.ToInt32(dataReader.GetValue(dataReader.GetOrdinal("maxTheoreticalContractInventory")));
                        tLimits.maxTheoProductInventory = Convert.ToInt32(dataReader.GetValue(dataReader.GetOrdinal("maxTheoreticalProductInventory")));
                        tLimits.maxClipOutright = Convert.ToInt32(dataReader.GetValue(dataReader.GetOrdinal("maxClipOutright")));
                        tLimits.maxClipSpread = Convert.ToInt32(dataReader.GetValue(dataReader.GetOrdinal("maxClipSpread")));
                        tLimits.workingOutrightDiscount = Convert.ToInt32(dataReader.GetValue(dataReader.GetOrdinal("workingOutrightDiscount")));
                        tLimits.workingOutrightInventoryDiscount = Convert.ToInt32(dataReader.GetValue(dataReader.GetOrdinal("workingOutrightInventoryDiscount")));
                        tLimits.maxOrderQty = Convert.ToInt32(dataReader.GetValue(dataReader.GetOrdinal("maxOrderQuantity")));
                        tLimits.productLimit = Convert.ToInt32(dataReader.GetValue(dataReader.GetOrdinal("productLimit")));

                        list.Add(tLimits);
                    }

                    dataReader.Close();
                    dataReader = null;
                    sCmd = null;
                    this.CloseConnection();
                }
            }
            catch (Exception err)
            {
                LoggerHome.GetLogger(this).Error("Error in Get Limits For Trader " + err.ToString());
                this.CloseConnection();
            }
            return list;
        }

        public bool GetOverrideLossLimit(string traderID,out double limit)
        {
            limit = 0;
            bool isRecordFound = false;
            try
            {
                string query = "risk.GetTraderLimitsOverride";
                if (this.OpenConnection() == true)
                {
                    MySqlCommand sCmd = new MySqlCommand(query, this._connection);
                    sCmd.CommandType = System.Data.CommandType.StoredProcedure;
                    sCmd.Parameters.AddWithValue("?traderID", traderID);
                    MySqlDataReader dataReader = sCmd.ExecuteReader();
                    while (dataReader.Read())
                    {
                        isRecordFound = true;
                        limit = Convert.ToDouble(dataReader.GetValue(dataReader.GetOrdinal("creditLimit")));
                    }

                    dataReader.Close();
                    dataReader = null;
                    sCmd = null;
                    this.CloseConnection();
                }
            }
            catch (Exception err)
            {
                LoggerHome.GetLogger(this).Error("Error in Get Limits For Trader " + err.ToString());
                this.CloseConnection();
            }

            return isRecordFound;
        }


        public bool AuthenticateUser(string username, string password, out UserRole userRole)
        {
            bool isAuthenicated = false;
            userRole = new UserRole();
            int count = 0;
            try
            {
                string query = "risk.AuthenticateLogin";
                if (this.OpenConnection() == true)
                {
                    MySqlCommand sCmd = new MySqlCommand(query, this._connection);
                    sCmd.CommandType = System.Data.CommandType.StoredProcedure;
                    sCmd.Parameters.AddWithValue("?username", username);
                    sCmd.Parameters.AddWithValue("?passwd", password);
                    MySqlDataReader dataReader = sCmd.ExecuteReader();

                    while (dataReader.Read())
                    {
                        if (count > 1)
                            return false;

                        userRole.userID = Convert.ToInt32(dataReader.GetValue(dataReader.GetOrdinal("userID")));
                        string strUserType = dataReader.GetString(dataReader.GetOrdinal("userType"));
                        if (strUserType == "Trader")
                            userRole.userType = UserType.Trader;
                        else if (strUserType == "Admin")
                            userRole.userType = UserType.Admin;
                        else if (strUserType == "ClearingAdmin")
                            userRole.userType = UserType.ClearingAdmin;

                        string strStatus = dataReader.GetString(dataReader.GetOrdinal("statusName"));
                        if (strStatus == "Active")
                            userRole.status = ActivityStatus.Active;
                        else if (strStatus == "Inactive")
                            userRole.status = ActivityStatus.Inactive;

                        isAuthenicated = true;
                        count++;
                    }

                    dataReader.Close();
                    dataReader = null;
                    sCmd = null;
                    this.CloseConnection();
                }
            }
            catch (Exception err)
            {
                LoggerHome.GetLogger(this).Error("Error in AuthenticateUser " + err.ToString());
                this.CloseConnection();
            }
            return isAuthenicated;
        }

        public int UpdateLoginActivity(int userID, DateTime loginTime, string ipAddress, string machineName)
        {
            int activityID = 0;
            try
            {
                string query = "risk.UpdateLoginActivity";
                if (this.OpenConnection() == true)
                {
                    MySqlCommand sCmd = new MySqlCommand(query, this._connection);
                    sCmd.CommandType = System.Data.CommandType.StoredProcedure;
                    sCmd.Parameters.AddWithValue("?userID", userID);
                    sCmd.Parameters.AddWithValue("?loginTime", loginTime);
                    sCmd.Parameters.AddWithValue("?ipAddress", ipAddress);
                    sCmd.Parameters.AddWithValue("?machineName", machineName);

                    MySqlDataReader dataReader = sCmd.ExecuteReader();
                    if (dataReader.Read())
                    {
                        activityID = Convert.ToInt32(dataReader.GetValue(dataReader.GetOrdinal("idLoginActivity")));
                    }

                    sCmd = null;
                    this.CloseConnection();
                }
            }
            catch (Exception err)
            {
                LoggerHome.GetLogger(this).Error("Error in Update Login Activity " + err.ToString());
                this.CloseConnection();
            }
            return activityID;
        }

        public void UpdateLogoffActivity(int userID, int activityID, DateTime logoffTime, string machineName, string ipAddress)
        {
            try
            {
                string query = "risk.UpdateLogoffActivity";
                if (this.OpenConnection() == true)
                {
                    MySqlCommand sCmd = new MySqlCommand(query, this._connection);
                    sCmd.CommandType = System.Data.CommandType.StoredProcedure;
                    sCmd.Parameters.AddWithValue("?userID", userID);
                    sCmd.Parameters.AddWithValue("?activityID", activityID);
                    sCmd.Parameters.AddWithValue("?logoffTime", logoffTime);
                    sCmd.Parameters.AddWithValue("?ipAddress", ipAddress);
                    sCmd.Parameters.AddWithValue("?machineName", machineName);

                    sCmd.ExecuteNonQuery();

                    sCmd = null;
                    this.CloseConnection();
                }
            }
            catch (Exception err)
            {
                this.CloseConnection();
                LoggerHome.GetLogger(this).Error("Error in Update Logoff Activity" + err.ToString());
            }
        }

        public Dictionary<string, InstrumentLevels> GetSettleLevels()
        {
            Dictionary<string, InstrumentLevels> levelsMap = new Dictionary<string, InstrumentLevels>();
            try
            {
                string query = "eldorado.GetExoticSettles";
                if (this.OpenConnection() == true)
                {
                    MySqlCommand sCmd = new MySqlCommand(query, this._connection);
                    sCmd.CommandType = System.Data.CommandType.StoredProcedure;

                    MySqlDataReader sr = sCmd.ExecuteReader();
                    while (sr.Read())
                    {
                        string instrument = sr.GetString(sr.GetOrdinal("productSymbol"));
                        double oneWeekLow = Convert.ToDouble(sr.GetValue(sr.GetOrdinal("oneWeekLow")));
                        double oneWeekHigh = Convert.ToDouble(sr.GetValue(sr.GetOrdinal("oneWeekHigh")));
                        double twoWeekLow = Convert.ToDouble(sr.GetValue(sr.GetOrdinal("twoWeekLow")));
                        double twoWeekHigh = Convert.ToDouble(sr.GetValue(sr.GetOrdinal("twoWeekHigh")));
                        double threeWeekLow = Convert.ToDouble(sr.GetValue(sr.GetOrdinal("threeWeekLow")));
                        double threeWeekHigh = Convert.ToDouble(sr.GetValue(sr.GetOrdinal("threeWeekHigh")));
                        double oneMonthLow = Convert.ToDouble(sr.GetValue(sr.GetOrdinal("oneMonthLow")));
                        double oneMonthHigh = Convert.ToDouble(sr.GetValue(sr.GetOrdinal("oneMonthHigh")));
                        double threeMonthLow = Convert.ToDouble(sr.GetValue(sr.GetOrdinal("threeMonthLow")));
                        double threeMonthHigh = Convert.ToDouble(sr.GetValue(sr.GetOrdinal("threeMonthHigh")));
                        double sixMonthLow = Convert.ToDouble(sr.GetValue(sr.GetOrdinal("sixMonthLow")));
                        double sixMonthHigh = Convert.ToDouble(sr.GetValue(sr.GetOrdinal("sixMonthHigh")));
                        double nineMonthLow = Convert.ToDouble(sr.GetValue(sr.GetOrdinal("nineMonthLow")));
                        double nineMonthHigh = Convert.ToDouble(sr.GetValue(sr.GetOrdinal("nineMonthHigh")));
                        double oneYearLow = Convert.ToDouble(sr.GetValue(sr.GetOrdinal("oneYearLow")));
                        double oneYearHigh = Convert.ToDouble(sr.GetValue(sr.GetOrdinal("oneYearHigh")));

                        InstrumentLevels level = new InstrumentLevels(oneWeekLow, oneWeekHigh, twoWeekLow, twoWeekHigh, threeWeekLow, threeWeekHigh, oneMonthLow, oneMonthHigh,
                            threeMonthLow, threeMonthHigh, sixMonthLow, sixMonthHigh, nineMonthLow, nineMonthHigh, oneYearLow, oneYearHigh);

                        if (levelsMap.ContainsKey(instrument) == false)
                            levelsMap.Add(instrument, level);
                    }

                    sr.Close();
                    this.CloseConnection();
                }
            }
            catch (Exception err)
            {
                this.CloseConnection();
                LoggerHome.GetLogger(this).Error("Error in Get Instrument Levels" + err.ToString());
            }
            return levelsMap;
        }

        public Dictionary<string, InstrumentLevels> GetHighLowLevels()
        {
            Dictionary<string, InstrumentLevels> levelsMap = new Dictionary<string, InstrumentLevels>();
            try
            {
                string query = "eldorado.GetExoticHighLow";
                if (this.OpenConnection() == true)
                {
                    MySqlCommand sCmd = new MySqlCommand(query, this._connection);
                    sCmd.CommandType = System.Data.CommandType.StoredProcedure;

                    MySqlDataReader sr = sCmd.ExecuteReader();
                    while (sr.Read())
                    {
                        string instrument = sr.GetString(sr.GetOrdinal("productSymbol"));
                        double oneWeekLow = Convert.ToDouble(sr.GetValue(sr.GetOrdinal("oneWeekLow")));
                        double oneWeekHigh = Convert.ToDouble(sr.GetValue(sr.GetOrdinal("oneWeekHigh")));
                        double twoWeekLow = Convert.ToDouble(sr.GetValue(sr.GetOrdinal("twoWeekLow")));
                        double twoWeekHigh = Convert.ToDouble(sr.GetValue(sr.GetOrdinal("twoWeekHigh")));
                        double threeWeekLow = Convert.ToDouble(sr.GetValue(sr.GetOrdinal("threeWeekLow")));
                        double threeWeekHigh = Convert.ToDouble(sr.GetValue(sr.GetOrdinal("threeWeekHigh")));
                        double oneMonthLow = Convert.ToDouble(sr.GetValue(sr.GetOrdinal("oneMonthLow")));
                        double oneMonthHigh = Convert.ToDouble(sr.GetValue(sr.GetOrdinal("oneMonthHigh")));
                        double threeMonthLow = Convert.ToDouble(sr.GetValue(sr.GetOrdinal("threeMonthLow")));
                        double threeMonthHigh = Convert.ToDouble(sr.GetValue(sr.GetOrdinal("threeMonthHigh")));
                        double sixMonthLow = Convert.ToDouble(sr.GetValue(sr.GetOrdinal("sixMonthLow")));
                        double sixMonthHigh = Convert.ToDouble(sr.GetValue(sr.GetOrdinal("sixMonthHigh")));
                        double nineMonthLow = Convert.ToDouble(sr.GetValue(sr.GetOrdinal("nineMonthLow")));
                        double nineMonthHigh = Convert.ToDouble(sr.GetValue(sr.GetOrdinal("nineMonthHigh")));
                        double oneYearLow = Convert.ToDouble(sr.GetValue(sr.GetOrdinal("oneYearLow")));
                        double oneYearHigh = Convert.ToDouble(sr.GetValue(sr.GetOrdinal("oneYearHigh")));

                        InstrumentLevels level = new InstrumentLevels(oneWeekLow, oneWeekHigh, twoWeekLow, twoWeekHigh, threeWeekLow, threeWeekHigh, oneMonthLow, oneMonthHigh,
                            threeMonthLow, threeMonthHigh, sixMonthLow, sixMonthHigh, nineMonthLow, nineMonthHigh, oneYearLow, oneYearHigh);

                        if (levelsMap.ContainsKey(instrument) == false)
                            levelsMap.Add(instrument, level);
                    }

                    sr.Close();
                    this.CloseConnection();
                }
            }
            catch (Exception err)
            {
                this.CloseConnection();
                LoggerHome.GetLogger(this).Error("Error in Get Instrument Levels" + err.ToString());
            }
            return levelsMap;
        }

        public List<string> GetConsecutiveFlys()
        {
            List<string> symbolList = new List<string>();

            try
            {
                string query = "eldorado.GetConsecutiveFlys";
                if (this.OpenConnection() == true)
                {
                    MySqlCommand sCmd = new MySqlCommand(query, this._connection);
                    sCmd.CommandType = System.Data.CommandType.StoredProcedure;

                    MySqlDataReader sr = sCmd.ExecuteReader();
                    while (sr.Read())
                    {
                        string instrument = sr.GetString(sr.GetOrdinal("mainProduct"));
                        if (symbolList.Contains(instrument) == false)
                            symbolList.Add(instrument);
                    }

                    sr.Close();
                    this.CloseConnection();
                }
            }
            catch (Exception err)
            {
                this.CloseConnection();
                LoggerHome.GetLogger(this).Error("Error in GetConsecutive Flys" + err.ToString());
            }
             
            return symbolList;
        }

        public Dictionary<string, double> GetSettleData()
        {
            Dictionary<string, double> settleData = new Dictionary<string, double>();
            try
            {
                string query = "eldorado.returnProductSettleData";
                if (this.OpenConnection() == true)
                {
                    MySqlCommand sCmd = new MySqlCommand(query, this._connection);
                    sCmd.CommandType = System.Data.CommandType.StoredProcedure;

                    MySqlDataReader sr = sCmd.ExecuteReader();
                    while (sr.Read())
                    {
                        string instrument = sr.GetString(sr.GetOrdinal("productSymbol"));
                        double settle = Convert.ToDouble(sr.GetValue(sr.GetOrdinal("close")));

                        settleData[instrument] = settle;
                    }

                    sr.Close();
                    this.CloseConnection();
                }
            }
            catch (Exception err)
            {
                this.CloseConnection();
                LoggerHome.GetLogger(this).Error("Error in GetSettleData" + err.ToString());
            }
            return settleData;
        }

        public Dictionary<string, Dictionary<string, string>> GetAlgosForUser(string user)
        {
            Dictionary<string, Dictionary<string, string>> algoMap = new Dictionary<string, Dictionary<string, string>>();
            try
            {
                string query = "risk.GetAlgosForUser";
                if (this.OpenConnection() == true)
                {
                    MySqlCommand sCmd = new MySqlCommand(query, this._connection);
                    sCmd.CommandType = System.Data.CommandType.StoredProcedure;
                    sCmd.Parameters.AddWithValue("?user", user);

                    MySqlDataReader sr = sCmd.ExecuteReader();
                    while (sr.Read())
                    {
                        string algoName = sr.GetString(sr.GetOrdinal("algoName"));
                        string algoType = sr.GetString(sr.GetOrdinal("algoTypeName"));
                        string paramName = sr.GetString(sr.GetOrdinal("parameterName"));
                        string paramValue = sr.GetString(sr.GetOrdinal("parameterValue"));

                        if (algoMap.ContainsKey(algoName + "|" + algoType) == false)
                        {
                            Dictionary<string, string> paramMap = new Dictionary<string, string>();
                            paramMap.Add(paramName, paramValue);
                            algoMap[algoName + "|" + algoType] = paramMap;
                        }
                        else
                        {
                            Dictionary<string, string> paramMap = algoMap[algoName + "|" + algoType];
                            if (paramMap.ContainsKey(paramName) == false)
                                paramMap.Add(paramName, paramValue);
                        }
                    }

                    sr.Close();
                    this.CloseConnection();
                }
            }
            catch (Exception err)
            {
                this.CloseConnection();
                LoggerHome.GetLogger(this).Error("Error in GetAlgosForUser" + err.ToString());
            }

            return algoMap;
        }

        public List<string> GetAlgosNoParamsForUser(string user)
        {
            List<string> algoMap = new List<string>();
            try
            {
                string query = "risk.GetAlgosNoParamsForUser";
                if (this.OpenConnection() == true)
                {
                    MySqlCommand sCmd = new MySqlCommand(query, this._connection);
                    sCmd.CommandType = System.Data.CommandType.StoredProcedure;
                    sCmd.Parameters.AddWithValue("?user", user);

                    MySqlDataReader sr = sCmd.ExecuteReader();
                    while (sr.Read())
                    {
                        string algoName = sr.GetString(sr.GetOrdinal("algoName"));
                        string algoType = sr.GetString(sr.GetOrdinal("algoTypeName"));

                        if (algoMap.Contains(algoName + "|" + algoType) == false)
                        {
                            algoMap.Add(algoName + "|" + algoType);
                        }
                    }

                    sr.Close();
                    this.CloseConnection();
                }
            }
            catch (Exception err)
            {
                this.CloseConnection();
                LoggerHome.GetLogger(this).Error("Error in GetAlgosForUser" + err.ToString());
            }

            return algoMap;
        }


        public Dictionary<string, double> GetCloseValues(out Dictionary<string, double> settleValues)
        {
            Dictionary<string, double> closeValues = new Dictionary<string, double>();
            settleValues = new Dictionary<string, double>();
            try
            {
                string query = "eldorado.GetCloseValues";
                if (this.OpenConnection() == true)
                {
                    MySqlCommand sCmd = new MySqlCommand(query, this._connection);
                    sCmd.CommandType = System.Data.CommandType.StoredProcedure;

                    MySqlDataReader sr = sCmd.ExecuteReader();
                    while (sr.Read())
                    {
                        string productName = sr.GetString(sr.GetOrdinal("productSymbol"));
                        double closePx = Convert.ToDouble(sr.GetValue(sr.GetOrdinal("close")));
                        double settlePx = Convert.ToDouble(sr.GetValue(sr.GetOrdinal("settlePrice")));

                        if (closeValues.ContainsKey(productName) == false)
                            closeValues.Add(productName, closePx);

                        if (settleValues.ContainsKey(productName) == false)
                            settleValues.Add(productName, settlePx);
                    } 

                    sr.Close();
                    this.CloseConnection();
                }
            }
            catch (Exception err)
            {
                this.CloseConnection();
                LoggerHome.GetLogger(this).Error("Error in GetCloseValues" + err.ToString());
            }
            return closeValues;
        }

        public Dictionary<string, List<PriceFormat>> GetPriceFormats(out PriceFormat defaultFormat)
        {
            Dictionary<string, List<PriceFormat>> pxFormatCache = new Dictionary<string, List<PriceFormat>>();
            defaultFormat = new PriceFormat();
            try
            {
                string query = "eldorado.GetPriceFormat";
                if (this.OpenConnection() == true)
                {
                    MySqlCommand sCmd = new MySqlCommand(query, this._connection);
                    sCmd.CommandType = System.Data.CommandType.StoredProcedure;

                    MySqlDataReader sr = sCmd.ExecuteReader();
                    while (sr.Read())
                    {
                        string productGroup = sr.GetString(sr.GetOrdinal("ProductGroup"));
                        string productType = sr.GetString(sr.GetOrdinal("ProductType"));
                        double ticks = Convert.ToDouble(sr.GetValue(sr.GetOrdinal("Ticks")));
                        double displayMultiplier = Convert.ToDouble(sr.GetValue(sr.GetOrdinal("DisplayMultiplier")));
                        int numofDecimals = Convert.ToInt32(sr.GetValue(sr.GetOrdinal("NumberofDecimals")));
                        int priceConfig = Convert.ToInt32(sr.GetValue(sr.GetOrdinal("PriceConfig")));

                        PriceFormat pxFormat = new PriceFormat(productGroup, productType, ticks, displayMultiplier, numofDecimals, priceConfig);

                        if (pxFormat.productGroup == "Default")
                            defaultFormat = pxFormat;

                        if (pxFormatCache.ContainsKey(pxFormat.productGroup))
                        {
                            pxFormatCache[pxFormat.productGroup].Add(pxFormat);
                        }
                        else
                        {
                            pxFormatCache.Add(pxFormat.productGroup, new List<PriceFormat>() { pxFormat });
                        }
                    }

                    sr.Close();
                    this.CloseConnection();
                }
            }
            catch (Exception err)
            {
                this.CloseConnection();
                LoggerHome.GetLogger(this).Error("Error in GetPriceFormats" + err.ToString());
            }
            return pxFormatCache;
        }

        public List<HaloConfig> GetHaloConfig(string userID,string traderID)
        {
            List<HaloConfig> sList = new List<HaloConfig>();
            try
            {
                string query = "eldorado.returnHaloConfig";

                if (this.OpenConnection() == true)
                {
                    MySqlCommand sCmd = new MySqlCommand(query, this._connection);
                    sCmd.CommandType = System.Data.CommandType.StoredProcedure;
                    sCmd.Parameters.AddWithValue("?username", userID);
                    sCmd.Parameters.AddWithValue("?traderID", traderID);
                    
                    MySqlDataReader sr = sCmd.ExecuteReader();
                    while (sr.Read())
                    {
                        string configName = sr.GetString(sr.GetOrdinal("channelName"));
                        string bookChannel = sr.GetString(sr.GetOrdinal("bookChannel"));
                        string tradeChannel = sr.GetString(sr.GetOrdinal("tradeChannel"));
                        string statsChannel = sr.GetString(sr.GetOrdinal("statsChannel"));
                        string enrichedChannel = sr.GetString(sr.GetOrdinal("enrichedChannel"));
                        string onDemandChannel = sr.GetString(sr.GetOrdinal("onDemandChannel"));
                        string protocol = sr.GetString(sr.GetOrdinal("protocol"));

                        HaloConfig pConfig = new HaloConfig(configName, bookChannel, tradeChannel, statsChannel, enrichedChannel, onDemandChannel,protocol);
                        sList.Add(pConfig);
                    }


                    sr.Close();
                    this.CloseConnection();
                }

            }
            catch (Exception err)
            {
                LoggerHome.GetLogger(this).Error("Error getting halo channel information ", err);
            }

            return sList;   
        }

        public List<GatewayConfig> GetGatewayConfig(string userID)
        {
            List<GatewayConfig> sList = new List<GatewayConfig>();
            try
            {
                string query = "risk.returnGatewayConfig";

                if (this.OpenConnection() == true)
                {
                    MySqlCommand sCmd = new MySqlCommand(query, this._connection);
                    sCmd.CommandType = System.Data.CommandType.StoredProcedure;
                    sCmd.Parameters.AddWithValue("?userName", userID);

                    MySqlDataReader sr = sCmd.ExecuteReader();
                    while (sr.Read())
                    {
                        int channelID = Convert.ToInt32(sr.GetValue(sr.GetOrdinal("channelID")));
                        string configName = sr.GetString(sr.GetOrdinal("channelName"));
                        string multicastChannel = sr.GetString(sr.GetOrdinal("multicastChannel"));
                        string protocol = sr.GetString(sr.GetOrdinal("protocol"));
                        string directChannel = sr.GetString(sr.GetOrdinal("directChannel"));

                        GatewayConfig pConfig = new GatewayConfig(channelID, configName, multicastChannel, protocol, directChannel);
                        sList.Add(pConfig);
                    }

                    sr.Close();
                    this.CloseConnection();
                }

            }
            catch (Exception err)
            {
                LoggerHome.GetLogger(this).Error("Error getting gateway channel information ", err);
            }

            return sList;
        }

        public List<GatewayConfig> GetDirectChannelConfig(string userID)
        {
            List<GatewayConfig> sList = new List<GatewayConfig>();
            try
            {
                string query = "risk.returnGatewayDirectChannelConfig";
                if (this.OpenConnection() == true)
                {
                    MySqlCommand sCmd = new MySqlCommand(query, this._connection);
                    sCmd.CommandType = System.Data.CommandType.StoredProcedure;
                    sCmd.Parameters.AddWithValue("?userName", userID);

                    MySqlDataReader sr = sCmd.ExecuteReader();
                    while (sr.Read())
                    {
                        int channelID = Convert.ToInt32(sr.GetValue(sr.GetOrdinal("channelID")));
                        string configName = sr.GetString(sr.GetOrdinal("channelName"));
                        string directChannel = sr.GetString(sr.GetOrdinal("address"));

                        GatewayConfig pConfig = new GatewayConfig(channelID, configName, "", "", directChannel);
                        sList.Add(pConfig);
                    }

                    sr.Close();
                    this.CloseConnection();
                }
            }
            catch (Exception)
            { }
            return sList;    
        }


        public List<string> GetSingleSessionInstruments()
        {
            List<string> sList = new List<string>();
            try
            {
                string query = "eldorado.returnSingleSessionInstruments";
                if (this.OpenConnection() == true)
                {
                    MySqlCommand sCmd = new MySqlCommand(query, this._connection);
                    sCmd.CommandType = System.Data.CommandType.StoredProcedure;

                    MySqlDataReader sr = sCmd.ExecuteReader();
                    while (sr.Read())
                    {
                        string symbol = sr.GetString(sr.GetOrdinal("symbol"));
                        string strategyType = sr.GetString(sr.GetOrdinal("strategyType"));

                        if (sList.Contains(symbol + "," + strategyType) == false)
                            sList.Add(symbol + "," + strategyType);
                    }

                    sr.Close();
                    this.CloseConnection();
                }
            }
            catch (Exception err)
            {
                LoggerHome.GetLogger(this).Audit("Error in GetSingleSessionInstruments " + err.ToString());
            }

            return sList;
        }

        public bool UpdatePassword(string userID, string password)
        {
            try
            {
                string query = "risk.changePassword";
                if (this.OpenConnection() == true)
                {
                    MySqlCommand sCmd = new MySqlCommand(query, this._connection);
                    sCmd.CommandType = System.Data.CommandType.StoredProcedure;
                    sCmd.Parameters.AddWithValue("?username", userID);
                    sCmd.Parameters.AddWithValue("?passwd", password);
                    int result = sCmd.ExecuteNonQuery();

                    sCmd = null;
                    this.CloseConnection();
                }
            }
            catch (Exception err)
            {
                LoggerHome.GetLogger(this).Error("Error in Update Password ", err);
                this.CloseConnection();
                return false;
            }

            return true;
        }

        public List<string> GetChartSymbols()
        {
            List<string> sList = new List<string>();
            try
            {
                string query = "eldorado.GetChartSymbols";
                if (this.OpenConnection() == true)
                {
                    MySqlCommand sCmd = new MySqlCommand(query, this._connection);
                    sCmd.CommandType = System.Data.CommandType.StoredProcedure;

                    MySqlDataReader sr = sCmd.ExecuteReader();
                    while (sr.Read())
                    {
                        string symbol = sr.GetString(sr.GetOrdinal("Symbol"));

                        sList.Add(symbol);
                    }

                    sr.Close();
                    this.CloseConnection();
                }
            }
            catch (Exception)
            { 
            }

            return sList;
        }
    }
}
*/