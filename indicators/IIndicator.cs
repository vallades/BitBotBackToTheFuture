using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



    public enum Operation
    {
        buy,
        sell,
        nothing
    };

    public interface IIndicator
    {
        String getName();
        void setPeriod(int period);
        double getResult();
        double getResult2();
        Operation GetOperation(double[] arrayPriceOpen, double[] arrayPriceClose, double[] arrayPriceLow, double[] arrayPriceHigh, double[] arrayVolume);
    }
