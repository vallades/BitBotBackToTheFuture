using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

    public class IndicatorMOM: IndicatorBase, IIndicator
    {



        public IndicatorMOM()
        {
            this.indicator = this;
        this.period = 10;
        }
        public string getName()
        {
            return "MOM";
        }

    public void setPeriod(int period)
    {
        this.period = period;
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
                double[] result = new double[arrayPriceClose.Length];                
                TicTacTec.TA.Library.Core.Mom(0, arrayPriceClose.Length - 1,arrayPriceClose,this.period, out outBegidx, out outNbElement, result);
                double priceClose = arrayPriceClose[arrayPriceClose.Length - 1];
                double value = result[outNbElement - 1];
                this.result = value;
                if (value > 100)
                    return Operation.sell;
                if (value < -100)
                    return Operation.buy;
                return Operation.nothing;
            }
            catch
            {
                return Operation.nothing;
            }
        }
    }
