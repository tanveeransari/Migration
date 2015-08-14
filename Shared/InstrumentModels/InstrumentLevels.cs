using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.InstrumentModels
{
    public class InstrumentLevels
    {
        public double oneWeekLow = 0.0;
        public double oneWeekHigh = 0.0;
        public double twoWeekLow = 0.0;
        public double twoWeekHigh = 0.0;
        public double threeWeekHigh = 0.0;
        public double threeWeekLow = 0.0;
        public double oneMonthLow = 0.0;
        public double oneMonthHigh = 0.0;
        public double threeMonthLow = 0.0;
        public double threeMonthHigh = 0.0;
        public double sixMonthLow = 0.0;
        public double sixMonthHigh = 0.0;
        public double nineMonthLow = 0.0;
        public double nineMonthHigh = 0.0;
        public double oneYearLow = 0.0;
        public double oneYearHigh = 0.0;

        public InstrumentLevels()
        {
        }

        public InstrumentLevels(double pOneWeekLow, double pOneWeekHigh, double pTwoWeekLow, double pTwoWeekHigh, double pThreeWeekLow,
                                 double pThreeWeekHigh, double pOneMonthLow, double pOneMonthHigh, double pThreeMonthLow, double pThreeMonthHigh,
                                 double pSixMonthLow, double pSixMonthHigh, double pNineMonthLow, double pNineMonthHigh, double pOneYearLow, double pOneYearHigh)
        {
            this.oneWeekLow = pOneWeekLow;
            this.oneWeekHigh = pOneWeekHigh;
            this.twoWeekLow = pTwoWeekLow;
            this.twoWeekHigh = pTwoWeekHigh;
            this.threeWeekLow = pThreeWeekLow;
            this.threeWeekHigh = pThreeWeekHigh;
            this.oneMonthLow = pOneMonthLow;
            this.oneMonthHigh = pOneMonthHigh;
            this.threeMonthLow = pThreeMonthLow;
            this.threeMonthHigh = pThreeMonthHigh;
            this.sixMonthLow = pSixMonthLow;
            this.sixMonthHigh = pSixMonthHigh;
            this.nineMonthLow = pNineMonthLow;
            this.nineMonthHigh = pNineMonthHigh;
            this.oneYearLow = pOneYearLow;
            this.oneYearHigh = pOneYearHigh;
        }
    }
}
