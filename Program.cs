using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace BitBotBackToTheFuture
{
    class MainClass
    {


        //REAL NET
        static string version = "0.0.0.2";
        static string location = @"c:\bitmex\";
        static string bitmexKey = "";
        static string bitmexSecret = "";
        static string timeGraph = "";
        static string statusShort = "";
        static string statusLong = "";
        static string pair = "";
        static int qtdyContacts = 0;
        static int interval = 0;
        static int intervalOrder = 0;
        static double profit = 0;
        static double fee = 0;
        private static string bitmexDomain = "";
        static BitMEX.BitMEXApi bitMEXApi = null;

        



        public static void Main(string[] args)
        {
            try
            {
                //Config
                log("Deleron - Back to the future - v"+version+" - Bitmex version");
                log("by Matheus Grijo");
                log("GITHUB http://github.com/matheusgrijo");
                log(" ******* DONATE ********* ");
                log("BTC 39DWjHHGXJh9q82ZrxkA8fiZoE37wL8jgh");
                log("BCH qqzwkd4klrfafwvl7ru7p7wpyt5z3sjk6y909xq0qk");
                log("ETH 0x3017E79f460023435ccD285Ff30Bd10834D20777");
                log("ETC 0x088E7E67af94293DB55D61c7B55E2B098d2258D9");
                log("Load config...");

                String jsonConfig = System.IO.File.ReadAllText(@"c:\bitmex\key.txt");
                JContainer jCointaner = (JContainer)JsonConvert.DeserializeObject(jsonConfig, (typeof(JContainer)));

                bitmexKey = jCointaner["key"].ToString();
                bitmexSecret = jCointaner["secret"].ToString();
                bitmexDomain = jCointaner["domain"].ToString();
                statusShort = jCointaner["short"].ToString();
                statusLong = jCointaner["long"].ToString();
                pair = jCointaner["pair"].ToString();
                timeGraph = jCointaner["timeGraph"].ToString();
                qtdyContacts = int.Parse(jCointaner["contract"].ToString());
                interval = int.Parse(jCointaner["interval"].ToString());
                intervalOrder = int.Parse(jCointaner["intervalOrder"].ToString());
                profit = double.Parse(jCointaner["profit"].ToString());
                fee = double.Parse(jCointaner["fee"].ToString());                
                bitMEXApi = new BitMEX.BitMEXApi(bitmexKey, bitmexSecret, bitmexDomain);

                log("OK!");

                log("Total open orders: " + bitMEXApi.GetOpenOrders(pair).Count);
                log("");

                //makeOrder("Sell");
                while (true)
                {
                    List<BitMEX.Candle> lstCandle = bitMEXApi.GetCandleHistory(pair, 100, timeGraph);

                    double[] arrayPriceClose = new double[100];
                    int i = 0;
                    foreach (var candle in lstCandle)
                    {
                        arrayPriceClose[i] = (double)candle.Close;
                        i++;
                    }

                    Array.Reverse(arrayPriceClose);

                    double[] rsi = Indicators.RSI(arrayPriceClose, getIndicators("rsi","period"));
                    double[] ema3 = Indicators.EMA(arrayPriceClose, getIndicators("emaShort","period"));
                    double[] ema5 = Indicators.EMA(arrayPriceClose, getIndicators("emaLong","period"));
                    //double[] cog = Indicators.COG(arrayPriceClose, 10);

                    Console.Title = DateTime.Now.ToString() + " - " + pair + "(" + lstCandle[0].Close + ")";
                    log(DateTime.Now.ToString());
                    log("CLOSE PRICE " + lstCandle[0].Close);
                    log("RSI " + rsi[99]);
                    log("EMA LONG " + ema5[99]);
                    log("EMA SHORT " + ema3[99]);
                    //log("COG " + cog[99]);
                    log("");

                    //Strategy cross EMA and RSI
                    if (ema3[98] < ema5[98] && ema3[99] > ema5[99] && rsi[99] > getIndicators("rsi", "limit") && rsi[98] > rsi[97])
                    {
                        makeOrder("Buy");
                    }
                    if (ema3[98] > ema5[98] && ema3[99] < ema5[99] && rsi[99] < getIndicators("rsi", "limit") && rsi[98] < rsi[97])
                    {
                        makeOrder("Sell");
                    }

                    log("wait " + interval + "ms");
                    Thread.Sleep(interval);
                }



            }
            catch(Exception ex)
            {
                log("ERROR FATAL::" + ex.Message + ex.StackTrace);
            }


        }


        static void makeOrder(string side)
        {

            log("Make order " + side);
            if (side == "Sell" && statusShort == "enable")
            {
                String json = bitMEXApi.MarketOrder(pair, "Sell", qtdyContacts);
                log(json);
                JContainer jCointaner = (JContainer)JsonConvert.DeserializeObject(json, (typeof(JContainer)));
                double price = double.Parse(jCointaner["price"].ToString().Replace(".", "."));
                price -= (price * (fee + profit)) / 100;
                price = Math.Floor(price);
                json = bitMEXApi.PostOrderPostOnly(pair, "Buy", price, qtdyContacts);
                log(json);
            }
            if (side == "Buy" && statusLong == "enable")
            {
                String json = bitMEXApi.MarketOrder(pair, "Buy", qtdyContacts);
                log(json);
                JContainer jCointaner = (JContainer)JsonConvert.DeserializeObject(json, (typeof(JContainer)));
                double price = double.Parse(jCointaner["price"].ToString().Replace(".", "."));
                price += (price * (fee + profit)) / 100;
                price = Math.Ceiling(price);
                json = bitMEXApi.PostOrderPostOnly(pair, "Sell", price, qtdyContacts);
                log(json);
            }

            log("wait " + intervalOrder + "ms");
            Thread.Sleep(intervalOrder);

        }


        static int getIndicators(string nameIndicator,string field)
        {
            String jsonConfig = System.IO.File.ReadAllText(@"c:\bitmex\key.txt");
            JContainer jCointaner = (JContainer)JsonConvert.DeserializeObject(jsonConfig, (typeof(JContainer)));
            foreach (var item in jCointaner["indicators"])
            {
                if (item["name"].ToString() == nameIndicator)
                {
                    return int.Parse(item[field].ToString());
                }
            }
            return 0;
        }

        static void log(string value)
        {
            try
            {
                value = "[" + DateTime.Now.ToString() + "] - " + value;
                System.IO.StreamWriter w = new StreamWriter(location + DateTime.Now.ToString("yyyyMMdd") + "_log.txt",true);
                w.WriteLine(value);
                Console.WriteLine(value);
                w.Close();
                w.Dispose();
            }
            catch { }
        }

        class Indicators
        {
            public static double[] COG(double[] price, int period)
            {

                var cog = new double[price.Length];
                for (int i = period - 1; i < price.Length; ++i)
                {
                    var weightedSum = 0.0;
                    var sum = 0.0;
                    for (int j = 0; j < period; ++j)
                    {
                        weightedSum += price[i - period + j + 1] * (period - j);
                        sum += price[i - period + j + 1];
                    }

                    cog[i] = -weightedSum / sum;
                }

                return cog;
            }
            public static double[] EMA(double[] price, int period)
            {
                var ema = new double[price.Length];
                double sum = price[0];
                double coeff = 2.0 / (1.0 + period);

                for (int i = 0; i < price.Length; i++)
                {
                    sum += coeff * (price[i] - sum);
                    ema[i] = sum;
                }

                return ema;
            }

            public static double[] RSI(double[] price, int period)
            {
                var rsi = new double[price.Length];

                double gain = 0.0;
                double loss = 0.0;

                // first RSI value
                rsi[0] = 0.0;
                for (int i = 1; i <= period; ++i)
                {
                    var diff = price[i] - price[i - 1];
                    if (diff >= 0)
                    {
                        gain += diff;
                    }
                    else
                    {
                        loss -= diff;
                    }
                }

                double avrg = gain / period;
                double avrl = loss / period;
                double rs = gain / loss;
                rsi[period] = 100 - (100 / (1 + rs));

                for (int i = period + 1; i < price.Length; ++i)
                {
                    var diff = price[i] - price[i - 1];

                    if (diff >= 0)
                    {
                        avrg = ((avrg * (period - 1)) + diff) / period;
                        avrl = (avrl * (period - 1)) / period;
                    }
                    else
                    {
                        avrl = ((avrl * (period - 1)) - diff) / period;
                        avrg = (avrg * (period - 1)) / period;
                    }

                    rs = avrg / avrl;

                    rsi[i] = 100 - (100 / (1 + rs));
                }

                return rsi;
            }
        }

    }
}
