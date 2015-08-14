using System;
using System.Collections.Generic;
using System.Timers;
using System.Linq;
using SciChartElD.Common;

namespace SciChartElD.Data
{
    public interface IPriceDataService
    {
        void SubscribePriceUpdate(Action<PriceBar> callback);
        IEnumerable<PriceBar> GetHistoricalData();
    }

    public class PriceDataService : IPriceDataService
    {
        private readonly Instrument _instrument;
        private readonly TimeFrame _timeframe;
        private readonly DateTime _startDate;
        private Timer _timer;
        private DateTime _endDate;
        private Action<PriceBar> _priceUpdateCallback;
        private int _currentIndex;

        public PriceDataService(Instrument instrument, TimeFrame timeframe, DateTime startDate, DateTime endDate)
        {
            _instrument = instrument;
            _timeframe = timeframe;
            _startDate = startDate;
            _endDate = endDate;
        }

        public IEnumerable<PriceBar> GetHistoricalData()
        {
            var priceSeries = DataManager.Instance.GetPriceData(_instrument.Symbol + "_" + _timeframe.Value);
            _currentIndex = priceSeries.IndexOf(priceSeries.FirstOrDefault(x => x.DateTime > _endDate));
            return priceSeries.TakeWhile(x => x.DateTime > _startDate && x.DateTime < _endDate);
        }

        public void SubscribePriceUpdate(Action<PriceBar> callback)
        {
            _priceUpdateCallback = callback;
            StartTimer();
        }

        private void StartTimer()
        {
            StopTimer();

            _timer = new Timer(10);
            _timer.Elapsed += TimerOnElapsed;
            _timer.Start();
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            lock (this)
            {
                var priceSeries = DataManager.Instance.GetPriceData(_instrument.Symbol + "_" + _timeframe.Value);
                _priceUpdateCallback(Next(priceSeries));
            }
        }

        private PriceBar Next(PriceSeries priceSeries)
        {
            return priceSeries[_currentIndex++];
        }

        private void StopTimer()
        {
            if (_timer != null)
            {
                _timer.Stop();
                _timer.Elapsed -= TimerOnElapsed;
                _timer = null;
            }
        }
    }
}