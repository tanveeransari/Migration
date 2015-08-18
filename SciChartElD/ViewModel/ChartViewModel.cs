using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using Abt.Controls.SciChart;
using Abt.Controls.SciChart.ChartModifiers;
using Abt.Controls.SciChart.Model.DataSeries;
using Abt.Controls.SciChart.Utility.Mouse;
using Abt.Controls.SciChart.Visuals;
using Abt.Controls.SciChart.Visuals.Annotations;
using Abt.Controls.SciChart.Visuals.Axes;
using Abt.Controls.SciChart.Visuals.RenderableSeries;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using ProtoBuf;
using SciChartElD.Data;
using SciChartElD.Utility;
using Shared;
using Shared.Charts;
using Shared.InstrumentModels;
using Shared.Preferences;
using Shared.Utility;
using TicTacTec.TA.Library;

namespace SciChartElD.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class ChartViewModel : ViewModelBase
    {
        public const string SelectedChartTypePropertyName = "SelectedChartType";

        private readonly IMarketDataService _marketDataService;
        private ObservableCollection<IChartSeriesViewModel> _seriesViewModels = new ObservableCollection<IChartSeriesViewModel>();

        #region Members from ElD
        public bool InitialRangeNeedsCalculation = true;

        private PriceBar _lastPrice;
        private IndexRange _xVisibleRange;
        private BarDuration barDuration = BarDuration.OneMinute;

        private XyDataSeries<DateTime, long> _volumeData = new XyDataSeries<DateTime, long>();
        //private XyDataSeries<DateTime, double> _bidAskVolumeCumSeries = new XyDataSeries<DateTime, double>();
        //private XyDataSeries<DateTime, double> _bidVolumeSeries = new XyDataSeries<DateTime, double>();
        //private XyDataSeries<DateTime, double> _askVolumeSeries = new XyDataSeries<DateTime, double>();


        private Dictionary<string, MenuItemViewModel> durationModelMap = new Dictionary<string, MenuItemViewModel>();
        private List<string> yAxisIDList = new List<string>() { "Y1", "Y2", "Y3", "Y4", "Y5" };
        private Dictionary<string, string> instrumentToAxisMap = new Dictionary<string, string>();
        /*
        private Dictionary<string, List<IRenderableSeries>> instrumentToRenderableSeriesMap = new Dictionary<string, List<IRenderableSeries>>();
        private Dictionary<string, IRenderableSeries> technicalIndidcatorSeriesMap = new Dictionary<string, IRenderableSeries>();
        private Dictionary<string, OhlcDataSeries<DateTime, double>> symbolToDataSeriesMap = new Dictionary<string, OhlcDataSeries<DateTime, double>>();
        private Dictionary<string, XyDataSeries<DateTime, double>> symbolToXYDataSeriesMap = new Dictionary<string, XyDataSeries<DateTime, double>>();
        private Dictionary<string, XyDataSeries<DateTime, double>> techIndicatorMap = new Dictionary<string, XyDataSeries<DateTime, double>>();

        private Dictionary<string, RowDefinition> techIndicatorToRowDefinitionMap = new Dictionary<string, RowDefinition>();*/

        private TechStudyParameters techStudyParameters = new TechStudyParameters();
        #endregion

        public ChartViewModel()
        {
            this.SetDefaults();

            if (IsInDesignMode)
            {
                // Code runs in Blend --> create design time data.
                _marketDataService = new MarketDataService(new DateTime(2015, 08, 11), 1, 5000);
            }
            else
            {
                // Code runs "for real"
                _marketDataService = new MarketDataService(new DateTime(2015, 08, 11), 1, 1000);
            }

            var ds0 = new OhlcDataSeries<DateTime, double>();
            _seriesViewModels.Add(new ChartSeriesViewModel(ds0, new FastOhlcRenderableSeries() { Name = "Series1" }));
            // Append 500 historical bars to data series
            var prices = _marketDataService.GetHistoricalData(500);
            ds0.Append(
                prices.Select(x => x.DateTime),
                prices.Select(x => x.Open),
                prices.Select(x => x.High),
                prices.Select(x => x.Low),
                prices.Select(x => x.Close));

            _marketDataService.SubscribePriceUpdate(OnNewPrice);
            _xVisibleRange = new IndexRange(0, 1000);

            SelectedChartType = ChartType.FastOhlc;

            //this.DeleteCommand = new RelayCommand(DeleteCommandExecuted);
        }

        //private void DeleteCommandExecuted()
        //{
        //    throw new NotImplementedException();
        //}

        //public RelayCommand DeleteCommand { get; set; }

        public ICommand StartUpdatesCommand
        {
            get
            {
                return new ActionCommand(() =>
                {
                    _marketDataService.SubscribePriceUpdate(OnNewPrice);
                    Debug.WriteLine("StartUpdatesCommand invoked " + DateTime.Now.ToShortTimeString());
                });
            }
        }

        private void OnNewPrice(PriceBar price)
        {
            // Ensure only one update processed at a time from multi-threaded timer
            lock (this)
            {
                // Update the last price, or append? 
                var ds0 = (IOhlcDataSeries<DateTime, double>)_seriesViewModels[0].DataSeries;

                if (_lastPrice != null && _lastPrice.DateTime == price.DateTime)
                {
                    ds0.Update(price.DateTime, price.Open, price.High, price.Low, price.Close);
                }
                else
                {
                    ds0.Append(price.DateTime, price.Open, price.High, price.Low, price.Close);

                    // If the latest appending point is inside the viewport (i.e. not off the edge of the screen)
                    // then scroll the viewport 1 bar, to keep the latest bar at the same place
                    if (XVisibleRange.Max > ds0.Count)
                    {
                        var existingRange = _xVisibleRange;
                        var newRange = new IndexRange(existingRange.Min + 1, existingRange.Max + 1);
                        XVisibleRange = newRange;
                    }
                }

                _lastPrice = price;
            }
        }

        public ICommand StopUpdatesCommand { get { return new ActionCommand(() => _marketDataService.ClearSubscriptions()); } }


        #region Display Properties - change to Depndnc properties later
        private string _backgroundColor;
        public string BackgroundColor
        {
            get
            {
                return _backgroundColor;
            }
            set
            {
                _backgroundColor = value;
                RaisePropertyChanged("BackgroundColor");
            }
        }

        private string _upColor;
        public string UpColor
        {
            get
            {
                return _upColor;
            }
            set
            {
                _upColor = value;
                RaisePropertyChanged("UpColor");
            }
        }

        private string _upWickColor;
        public string UpWickColor
        {
            get
            {
                return _upWickColor;
            }
            set
            {
                _upWickColor = value;
                RaisePropertyChanged("UpWickColor");
            }
        }

        private string _downColor;
        public string DownColor
        {
            get
            {
                return _downColor;
            }
            set
            {
                _downColor = value;
                RaisePropertyChanged("DownColor");
            }
        }


        private string _downWickColor;
        public string DownWickColor
        {
            get
            {
                return _downWickColor;
            }
            set
            {
                _downWickColor = value;
                RaisePropertyChanged("DownWickColor");
            }
        }

        private string _showVerticalLines;
        public string ShowVerticalLines
        {
            get { return _showVerticalLines; }
            set
            {
                _showVerticalLines = value;
                RaisePropertyChanged("ShowVerticalLines");
            }
        }

        private string _showHorizontalLines;
        public string ShowHorizontalLines
        {
            get { return _showHorizontalLines; }
            set
            {
                _showHorizontalLines = value;
                RaisePropertyChanged("ShowHorizontalLines");
                RaisePropertyChanged("ShowHLines");
            }
        }

        public bool ShowHLines
        {
            get
            {
                if (_showHorizontalLines.ToUpper().StartsWith("T"))
                    return true;
                else
                    return false;
            }
        }

        private bool _showCursor;
        public bool ShowCursor
        {
            get { return _showCursor; }
            set
            {
                _showCursor = value;
                RaisePropertyChanged("ShowCursor");
            }
        }

        private bool _showWaitLabel;
        public bool ShowWaitLabel
        {
            get { return _showWaitLabel; }
            set
            {
                _showWaitLabel = value;
                RaisePropertyChanged("ShowWaitLabel");
            }
        }

        private void setModifier(ModifierType modifierType)
        {
            ChartModifier = modifierType;
        }

        private ModifierType _chartModifier; // CrosshairsCursor etc
        public ModifierType ChartModifier
        {
            get
            {
                return _chartModifier;
            }
            set
            {
                _chartModifier = value;
                RaisePropertyChanged("ChartModifier");
            }
        }

        #region Label Formatters
        private ILabelProvider _labelFormatterY1;
        public ILabelProvider XLabelFormatterY1
        {
            get { return _labelFormatterY1; }
            set
            {
                _labelFormatterY1 = value;
                RaisePropertyChanged("XLabelFormatterY1");
            }
        }

        private ILabelProvider _labelFormatterY2;
        public ILabelProvider XLabelFormatterY2
        {
            get { return _labelFormatterY2; }
            set
            {
                _labelFormatterY2 = value;
                RaisePropertyChanged("XLabelFormatterY2");
            }
        }

        private ILabelProvider _labelFormatterY3;
        public ILabelProvider XLabelFormatterY3
        {
            get { return _labelFormatterY3; }
            set
            {
                _labelFormatterY3 = value;
                RaisePropertyChanged("XLabelFormatterY3");
            }
        }

        private ILabelProvider _labelFormatterY4;
        public ILabelProvider XLabelFormatterY4
        {
            get { return _labelFormatterY4; }
            set
            {
                _labelFormatterY4 = value;
                RaisePropertyChanged("XLabelFormatterY4");
            }
        }

        private ILabelProvider _labelFormatterY5;
        public ILabelProvider XLabelFormatterY5
        {
            get { return _labelFormatterY5; }
            set
            {
                _labelFormatterY5 = value;
                RaisePropertyChanged("XLabelFormatterY5");
            }
        }

        private ILabelProvider _xLabelFormatter;
        public ILabelProvider XLabelFormatter
        {
            get { return _xLabelFormatter; }
            set
            {
                _xLabelFormatter = value;
                RaisePropertyChanged("XLabelFormatter");
            }
        }

        #endregion

        private string _Y2Color;
        public string Y2Color
        {
            get
            {
                return _Y2Color;
            }
            set
            {
                _Y2Color = value;
                RaisePropertyChanged("Y2Color");
            }
        }


        private string _Y3Color;
        public string Y3Color
        {
            get { return _Y3Color; }
            set
            {
                _Y3Color = value;
                RaisePropertyChanged("Y3Color");
            }
        }

        private string _Y4Color;
        public string Y4Color
        {
            get { return _Y4Color; }
            set
            {
                _Y4Color = value;
                RaisePropertyChanged("Y4Color");
            }
        }

        private string _Y5Color;
        public string Y5Color
        {
            get { return _Y5Color; }
            set
            {
                _Y5Color = value;
                RaisePropertyChanged("Y5Color");
            }
        }

        public string ADXLineColor
        {
            get
            {
                return techStudyParameters.ADX_LineColor;
            }
        }
        public string ATRLineColor
        {
            get
            {
                return techStudyParameters.ATR_LineColor;
            }
        }

        public string CCILineColor
        {
            get
            {
                return techStudyParameters.CCI_LineColor;
            }
        }
        public string FiboLineColor
        {
            get
            {
                return techStudyParameters.Fibonacci_LineColor;
            }
        }
        public string UpperBandColor
        {
            get
            {
                return techStudyParameters.BB_UpperLineColor;
            }
        }
        public string MiddleBandColor
        {
            get
            {
                return techStudyParameters.BB_MiddleLineColor;
            }
        }
        public string LowerBandColor
        {
            get
            {
                return techStudyParameters.BB_LowerLineColor;
            }
        }
        public string LRColor
        {
            get
            {
                return techStudyParameters.LR_LineColor;
            }
        }
        public string RSIColor
        {
            get
            {
                return techStudyParameters.RSI_LineColor;
            }
        }
        public string SMAColor
        {
            get
            {
                return techStudyParameters.SMA_LineColor;
            }
        }
        public string VWAPColor
        {
            get
            {
                return techStudyParameters.VWAP_LineColor;
            }
        }
        public string MACDColor
        {
            get
            {
                return techStudyParameters.MACD_LineColor;
            }
        }
        public string MACDSignalColor
        {
            get
            {
                return techStudyParameters.MACD_SignalColor;
            }
        }
        public string StochLineOneColor
        {
            get
            {
                return techStudyParameters.STOCH_LineColorOne;
            }
        }
        public string StochLineTwoColor
        {
            get
            {
                return techStudyParameters.STOCH_LineColorTwo;
            }
        }
        public string TrendLineColor
        {
            get
            {
                return techStudyParameters.TREND_LineColor;
            }
        }

        #endregion

        #region Properties - Change to DP Later?

        public ActionCommand SetCursorModifierCommand { get { return new ActionCommand(() => setModifier(ModifierType.CrosshairsCursor)); } }
        public ActionCommand SetRolloverModifierCommand { get { return new ActionCommand(() => setModifier(ModifierType.Rollover)); } }
        public ActionCommand SetRubberBandZoomModifierCommand { get { return new ActionCommand(() => setModifier(ModifierType.RubberBandZoom)); } }
        public ActionCommand SetZoomPanModifierCommand { get { return new ActionCommand(() => setModifier(ModifierType.ZoomPan)); } }
        public ActionCommand SetNullModifierCommand { get { return new ActionCommand(() => setModifier(ModifierType.Null)); } }


        public XyDataSeries<DateTime, long> VolumeData
        {
            get { return _volumeData; }
            set
            {
                _volumeData = value;
                RaisePropertyChanged("VolumeData");
            }
        }

        public IndexRange XVisibleRange
        {
            get { return _xVisibleRange; }
            set
            {
                _xVisibleRange = value;
                RaisePropertyChanged("XVisibleRange");
            }
        }

        private IRange _priceVisibleRange;
        public IRange PriceVisibleRange
        {
            get { return _priceVisibleRange; }
            set
            {
                _priceVisibleRange = value;
                RaisePropertyChanged("PriceVisibleRange");
            }
        }

        private double _barTimeFrame = TimeSpan.FromMinutes(1).TotalSeconds;
        public double BarTimeFrame
        {
            get { return _barTimeFrame; }
            set
            {
                _barTimeFrame = value;
                RaisePropertyChanged("BarTimeFrame");
            }
        }

        public ObservableCollection<MenuItemViewModel> ContextMenuList { get; set; }

        private AxisCollection _chartYAxes = new AxisCollection();
        public AxisCollection ChartYAxes
        {
            get { return _chartYAxes; }
            set
            {
                _chartYAxes = value;
                RaisePropertyChanged("ChartYAxes");
            }
        }

        private NumericAxis _yAxis = null;
        public NumericAxis DefaultYAxis
        {
            get { return _yAxis; }
            set
            {
                _yAxis = value;
                RaisePropertyChanged("DefaultYAxis");
            }
        }

        private ChartType _selectedChartType = ChartType.FastOhlc;

        public ChartType SelectedChartType
        {
            get
            {
                return _selectedChartType;
            }

            set
            {
                if (_selectedChartType == value)
                {
                    return;
                }

                _selectedChartType = value;
                RaisePropertyChanged(SelectedChartTypePropertyName);
            }
        }

        #endregion

        public ObservableCollection<IChartSeriesViewModel> SeriesViewModels
        {
            get { return _seriesViewModels; }
            set
            {
                _seriesViewModels = value;
                RaisePropertyChanged("SeriesViewModels");
            }
        }

        public void SetDefaults()
        {
            this.AddContextMenuItems();
            this.SetPreferencesDefaults();
            //this.UpdateChartData();
            //this.AddLegendAnnotation();
            this.hardCodeUserPrefs();
        }

        public void hardCodeUserPrefs()
        {
            BackgroundColor = "#000000";
            UpColor = "#7052CC54";
            UpWickColor = "#FF52CC54";
            DownColor = "#D0E26565";
            DownWickColor = "#FFE26565";
            ShowHorizontalLines = "True";
            ShowVerticalLines = "True";
            ShowCursor = true;

            Y2Color = "#FFFF0000";
            Y3Color = "#FFFFFF00";
            Y4Color = "#FF0099FF";
            Y5Color = "#FF996600";
        }

        public void AddContextMenuItems()
        {
            ContextMenuList = new ObservableCollection<MenuItemViewModel>();

            ICommand prefCommand = new ActionCommand(() => ShowPreferences());
            ContextMenuList.Add(new MenuItemViewModel("Preferences", null, prefCommand, false));

            #region ChartType
            ICommand ohlcCommand = new ActionCommand(() => SelectedChartType = ChartType.FastOhlc);
            MenuItemViewModel ohlc = new MenuItemViewModel("OHLC", null, ohlcCommand, false);
            //chartTypeModelMap.Add("OHLC", ohlc);

            ICommand candleCommand = new ActionCommand(() => SelectedChartType = ChartType.FastCandlestick);
            MenuItemViewModel candlestick = new MenuItemViewModel("CandleStick", null, candleCommand, true);
            //chartTypeModelMap.Add("CandleStick", candlestick);

            ICommand lineCommand = new ActionCommand(() => SelectedChartType = ChartType.FastLine);
            MenuItemViewModel line = new MenuItemViewModel("Line", null, lineCommand, true);
            //chartTypeModelMap.Add("Line", line);


            ObservableCollection<MenuItemViewModel> chartTypeColl = new ObservableCollection<MenuItemViewModel>();
            chartTypeColl.Add(ohlc);
            chartTypeColl.Add(candlestick);
            chartTypeColl.Add(line);

            ContextMenuList.Add(new MenuItemViewModel("ChartType", chartTypeColl));
            #endregion ChartType

            #region Duration
            ObservableCollection<MenuItemViewModel> durationColl = new ObservableCollection<MenuItemViewModel>();

            ICommand oneMinCommand = new ActionCommand(() => ChangeDuration(BarDuration.OneMinute));
            MenuItemViewModel oneMin = new MenuItemViewModel("1 Minute", null, oneMinCommand, false);
            durationModelMap.Add("1Min", oneMin);
            durationColl.Add(oneMin);

            ICommand threeMinCommand = new ActionCommand(() => ChangeDuration(BarDuration.ThreeMinute));
            MenuItemViewModel threeMin = new MenuItemViewModel("3 Minute", null, threeMinCommand, false);
            durationModelMap.Add("3Min", threeMin);
            durationColl.Add(threeMin);

            ICommand fiveMinCommand = new ActionCommand(() => ChangeDuration(BarDuration.FiveMinute));
            MenuItemViewModel fiveMin = new MenuItemViewModel("5 Minute", null, fiveMinCommand, false);
            //durationModelMap.Add("5Min", oneMin);
            durationModelMap.Add("5Min", fiveMin);
            durationColl.Add(fiveMin);

            ICommand tenMinCommand = new ActionCommand(() => ChangeDuration(BarDuration.TenMinute));
            MenuItemViewModel tenMin = new MenuItemViewModel("10 Minute", null, tenMinCommand, false);
            durationColl.Add(tenMin);

            ICommand thirtyMinCommand = new ActionCommand(() => ChangeDuration(BarDuration.ThirtyMinute));
            MenuItemViewModel thirtyMin = new MenuItemViewModel("30 Minute", null, thirtyMinCommand, false);
            durationColl.Add(thirtyMin);

            ICommand hourlyCommand = new ActionCommand(() => ChangeDuration(BarDuration.Hourly));
            MenuItemViewModel sixtyMin = new MenuItemViewModel("60 Minute", null, hourlyCommand, false);
            durationColl.Add(sixtyMin);

            ICommand dailyCommand = new ActionCommand(() => ChangeDuration(BarDuration.Daily));
            MenuItemViewModel dailyDur = new MenuItemViewModel("Daily", null, dailyCommand, false);
            durationColl.Add(dailyDur);

            ContextMenuList.Add(new MenuItemViewModel("Duration", durationColl, null, false));
            #endregion

            #region Technical Indicators

            ObservableCollection<MenuItemViewModel> technicalColl = new ObservableCollection<MenuItemViewModel>();
            /*
            ICommand ADXCommand = new ActionCommand(() => ADXAddRemove());
            MenuItemViewModel adx = new MenuItemViewModel("ADX", null, ADXCommand, false);
            technicalColl.Add(adx);
            
             ICommand CCICommand = new ActionCommand(() => CCIAddRemove());
             MenuItemViewModel cci = new MenuItemViewModel("CCI", null, CCICommand, false);
             technicalColl.Add(cci);

             ICommand BBCommand = new ActionCommand(() => BBandsAddRemove());
             MenuItemViewModel bbands = new MenuItemViewModel("Bollinger Bands", null, BBCommand, false);
             technicalColl.Add(bbands);

             ICommand FibCommand = new ActionCommand(() => FibAddRemove());
             MenuItemViewModel fib = new MenuItemViewModel("Fibonacci", null, FibCommand, false);
             technicalColl.Add(fib);

             ICommand LRCommand = new ActionCommand(() => LRAddRemove());
             MenuItemViewModel lr = new MenuItemViewModel("Linear Regression", null, LRCommand, false);
             technicalColl.Add(lr);

             ICommand MACDCommand = new ActionCommand(() => MACDAddRemove());
             MenuItemViewModel macd = new MenuItemViewModel("MACD", null, MACDCommand, false);
             technicalColl.Add(macd);

             ICommand SMACommand = new ActionCommand(() => SMAAddRemove());
             MenuItemViewModel sma = new MenuItemViewModel("Moving Average", null, SMACommand, false);
             technicalColl.Add(sma);

             ICommand SARCommand = new ActionCommand(() => SARAddRemove());
             MenuItemViewModel sar = new MenuItemViewModel("Parabolic SAR", null, SARCommand, false);
             technicalColl.Add(sar);

             ICommand RSICommand = new ActionCommand(() => RSIAddRemove());
             MenuItemViewModel rsi = new MenuItemViewModel("RSI", null, RSICommand, false);
             technicalColl.Add(rsi);

             ICommand StochCommand = new ActionCommand(() => StochAddRemove());
             MenuItemViewModel stoch = new MenuItemViewModel("Stochastic", null, StochCommand, false);
             technicalColl.Add(stoch);

             ICommand TrendlineCommand = new ActionCommand(() => AddTrendLine());
             MenuItemViewModel trendLine = new MenuItemViewModel("Trend Line", null, TrendlineCommand, false);
             technicalColl.Add(trendLine);

             ICommand VolumeCommand = new ActionCommand(() => AddRemoveVolume());
             MenuItemViewModel volume = new MenuItemViewModel("Volume", null, VolumeCommand, false);
             technicalColl.Add(volume);
             ICommand VWAPCommand = new ActionCommand(() => VWAPAddRemove());
             MenuItemViewModel vwap = new MenuItemViewModel("Volume Weighted Average Price", null, VWAPCommand, false);
             technicalColl.Add(vwap);

             ICommand ATRCommand = new ActionCommand(() => ATRAddRemove());
             MenuItemViewModel atr = new MenuItemViewModel("ATR", null, ATRCommand, false);
             technicalColl.Add(atr);

             ICommand VolumeAtPriceCommand = new ActionCommand(() => VolumeAtPriceAddRemove());
             MenuItemViewModel volumeAtPx = new MenuItemViewModel("Volume At Price", null, VolumeAtPriceCommand, false);
             //technicalColl.Add(volumeAtPx);

             ICommand HorizontalLineCommand = new ActionCommand(() => HorizontalLineAddRemove());
             MenuItemViewModel horizontalLine = new MenuItemViewModel("Horizontal Line", null, HorizontalLineCommand, false);
             technicalColl.Add(horizontalLine);


             ContextMenuList.Add(new MenuItemViewModel("Technical Indicators", technicalColl, null, false));

             */
            #endregion

            //ICommand techPrefCommand = new ActionCommand(() => ShowTechPreferences());
            //ContextMenuList.Add(new MenuItemViewModel("Technical Indicators Preferences", null, techPrefCommand, false));

            ICommand editSeriesCommand = new ActionCommand(() => ShowSeriesEdit());
            ContextMenuList.Add(new MenuItemViewModel("Edit Series", null, editSeriesCommand, false));
        }

        private void ChangeDuration(BarDuration barDuration)
        {
            this.ProcessChangeDuration(barDuration);
        }
        private void ProcessChangeDuration(BarDuration duration)
        {
            barDuration = duration;
            /*

            BarTimeFrame = TimeSpan.FromMinutes(ChartUtility.GetTimeDuration(barDuration)).TotalSeconds;
            InitialRangeNeedsCalculation = true;

            if (barDuration == BarDuration.Daily)
                XLabelFormatter = new CustomDateLabelFormatter();
            else
                XLabelFormatter = new CustomDateTimeLabelFormatter();

            string durName = ChartUtility.GetBarDuration(barDuration);
            ChangeChecked(durName);

            VolumeData.Clear();
            _bidAskVolumeCumSeries.Clear();
            _bidVolumeSeries.Clear();
            _askVolumeSeries.Clear();
            bidAskCumulativeVolume = 0;

            var keys = symbolToDataSeriesMap.Keys.ToList();
            foreach (string key in keys)
            {
                OhlcDataSeries<DateTime, double> newSeries = new OhlcDataSeries<DateTime, double>();
                symbolToDataSeriesMap[key] = newSeries;

                XyDataSeries<DateTime, double> newLineSeries = new XyDataSeries<DateTime, double>();
                symbolToXYDataSeriesMap[key] = newLineSeries;

                if (instrumentToRenderableSeriesMap.ContainsKey(key))
                {
                    foreach (IRenderableSeries rSeries in instrumentToRenderableSeriesMap[key])
                    {
                        rSeries.DataSeries = newSeries;
                    }
                }
                chartForm.sciChartControl.priceChart.InvalidateElement();

                int numBars = (int)Math.Ceiling((DateTime.Now - DateTime.Now.AddDays(-(ChartUtility.GetNumberOfDays(barDuration) + 1))).TotalMinutes / ChartUtility.GetTimeDuration(barDuration));

                if (synInstrumentMap.ContainsKey(key))
                {
                    foreach (var symbol in synInstrumentMap[key].symbolToVarMap.Keys)
                        ChartDataServerConnector.getInstance().RequestData(symbol, ChartUtility.GetTimeDuration(barDuration), "", "", numBars, RecvHistoricalData);

                    tempSyntheticInstrument = key;
                    inSyntheticMode = true;
                }
                else
                {
                    ChartDataServerConnector.getInstance().RequestData(key, ChartUtility.GetTimeDuration(barDuration), "", "", numBars, RecvHistoricalData);
                }
            }
            */
        }

        internal void DeleteSeries()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Activated by space bar press
        /// </summary>
        internal void QueueResetChartRange()
        {
            throw new NotImplementedException();
        }

        private void ShowPreferences()
        {
            throw new NotImplementedException();
        }

        private void ShowSeriesEdit()
        {
            /*
            string seriesName = "";
            TechStudyParameters tParam = null;
            if (SelectedSeries != null && SelectedSeries != "")
            {
                if (SelectedSeries.IndexOf("Upper") >= 0 || SelectedSeries.IndexOf("Middle") >= 0 || SelectedSeries.IndexOf("Lower") >= 0)
                {
                    string studyIndex = "";
                    if (SelectedSeries.IndexOf("Upper") >= 0 || SelectedSeries.IndexOf("Lower") >= 0)
                        studyIndex = SelectedSeries.Substring(5);
                    else
                        studyIndex = SelectedSeries.Substring(6);

                    if (BBandPreferences.ContainsKey("BBands" + studyIndex))
                    {
                        seriesName = "BollingerBands";
                        tParam = BBandPreferences["BBands" + studyIndex];
                    }
                }
                else if (SelectedSeries.IndexOf("SMA") >= 0)
                {
                    if (SMAPreferences.ContainsKey(SelectedSeries))
                    {
                        seriesName = "Moving Average";
                        tParam = SMAPreferences[SelectedSeries];
                    }
                }

                if (seriesName != "")
                {
                    EditSeries edSeries = new EditSeries(seriesName, tParam);
                    edSeries.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
                    edSeries.Location = chartForm.Location;
                    System.Windows.Forms.DialogResult dr = edSeries.ShowDialog();

                    if (dr == System.Windows.Forms.DialogResult.OK)
                    {
                        switch (seriesName)
                        {
                            case "BollingerBands":
                                ProcessRemoveSeries(SelectedSeries);
                                BBandsAdd(edSeries.formParameters);
                                break;
                            case "Moving Average":
                                ProcessRemoveSeries(SelectedSeries);
                                SMAAdd(edSeries.formParameters);
                                break;
                        }
                    }
                }
            }
            else
            {
                ShowTechPreferences();
            }
            */
        }

        private void SetupVolumeAtPriceChart()
        {
            /*
            NumericAxis xLeft = new NumericAxis();
            xLeft.Id = "XAxisLeft";
            xLeft.AxisAlignment = AxisAlignment.Left;
            xLeft.Visibility = System.Windows.Visibility.Visible;

            xLeft.FlipCoordinates = true;

            this.priceChart.XAxes.Add(xLeft);

            NumericAxis yBottom = new NumericAxis();
            yBottom.Id = "YAxisBottom";
            //yBottom.Visibility = System.Windows.Visibility.Collapsed;
            yBottom.AxisAlignment = AxisAlignment.Bottom;
            //yBottom.AutoRange = AutoRange.Never;
            yBottom.DrawMajorGridLines = false;
            yBottom.DrawMinorGridLines = false;
            yBottom.DrawMajorBands = false;

            ChartYAxes.Add(yBottom);

            FastColumnRenderableSeries volumeAtPx = new FastColumnRenderableSeries();
            volumeAtPx.YAxisId = "YAxisBottom";
            volumeAtPx.XAxisId = "XAxisLeft";
            volumeAtPx.SetValue(Panel.ZIndexProperty, dataZIndex++);
            volumeAtPx.Style = (System.Windows.Style)this.Resources["VolumeAtPriceColumnSeriesStyle"];

            this.priceChart.RenderableSeries.Add(volumeAtPx);
            */
        }

        private void SubscribeToChartData(bool addInstrument, string symbol)
        {
            /*
            if (!addInstrument)
            {
                MarketDataManager.getInstance().ChartDataUnsubscribe(_instrument, _chartName);
                _instrument = symbol;
            }
            MarketDataManager.getInstance().ChartDataSubscribe(symbol, _chartName, ZMQMessageReceived);
            //this.AddWaitAnnotation();
            ShowWaitLabel = true;
            ChartDataServerConnector.getInstance().RequestData(symbol, ChartUtility.GetTimeDuration(barDuration), "", "", ChartUtility.GetNumberOfBars(barDuration), RecvHistoricalData);
            */
        }

        public void SetPreferencesDefaults()
        {
            Debug.WriteLine("SetPreferencesDefaults not implemented");
        }

    }
}