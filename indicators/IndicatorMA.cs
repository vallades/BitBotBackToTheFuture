using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

    public class IndicatorMA:IndicatorBase, IIndicator
    {

        public IndicatorMA()
        {
            this.indicator = this;
        }


    public void setPeriod(int period)
    {
        this.period = period;
    }

    public string getName()
        {
            return "MA";
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
                int outBegidxLonga, outNbElementLonga, outBegidxCurta, outNbElementCurta;
                double[] arrayLonga = new double[arrayPriceClose.Length];                
                TicTacTec.TA.Library.Core.MovingAverage(0, arrayPriceClose.Length - 1,arrayPriceClose,5,TicTacTec.TA.Library.Core.MAType.Ema, out outBegidxLonga, out outNbElementLonga, arrayLonga);
                double value = arrayLonga[outNbElementLonga - 1];
                this.result = value;

                double[]  arrayCurta = new double[arrayPriceClose.Length];
                TicTacTec.TA.Library.Core.MovingAverage(0, arrayPriceClose.Length - 1, arrayPriceClose, 3, TicTacTec.TA.Library.Core.MAType.Ema, out outBegidxCurta, out outNbElementCurta, arrayCurta);
                double value2 = arrayCurta[outNbElementCurta - 1];
                this.result2 = value2;                
                    
                
                if ((arrayLonga[outNbElementLonga - 2] >= arrayCurta[outNbElementCurta - 2]) && arrayCurta[outNbElementCurta - 1] > arrayLonga[outNbElementLonga - 1])
                    return Operation.buy;
                if ((arrayLonga[outNbElementLonga - 2] <= arrayCurta[outNbElementCurta - 2]) && arrayCurta[outNbElementCurta - 1] < arrayLonga[outNbElementLonga - 1])
                    return Operation.sell;

                return Operation.nothing;
            }
            catch
            {
                return Operation.nothing;
            }
        }
    }
