using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

    public class IndicatorRSI : IndicatorBase, IIndicator
    {
        public IndicatorRSI()
        {
            this.indicator = this;
        this.period = 14;
        }

    public void setPeriod(int period)
    {
        this.period = period;
    }

    public string getName()
        {
            return "RSI";
        }

        public double getResult()
        {
            return this.result;
        }

        public double getResult2()
        {
            return this.result2;
        }

        public Operation GetOperation(double[] arrayPriceOpen, double[] arrayPriceClose, double[] arrayPriceLow, double[] arrayPriceHigh, double[] arrayVolume)
        {
            try
            {
                int outBegidx, outNbElement;
                double[] arrayresultTA = new double[arrayPriceClose.Length];
                arrayresultTA = new double[arrayPriceClose.Length];
                TicTacTec.TA.Library.Core.Rsi(0, arrayPriceClose.Length - 1, arrayPriceClose, this.period, out outBegidx, out outNbElement, arrayresultTA);
                double value = arrayresultTA[outNbElement - 1];
                this.result = value;
                if(value > 70)
                    return Operation.sell;
                if (value < 30)
                    return Operation.buy;
                return Operation.nothing;
            }
            catch
            {
                return Operation.nothing;
            }
        }

      
    }
