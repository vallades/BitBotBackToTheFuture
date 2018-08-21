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
        static string version = "0.0.0.3";
        static string location = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + "\\";
        static string bitmexKey = "";
        static string bitmexSecret = "";
        static string timeGraph = "";
        static string statusShort = "";
        static string statusLong = "";
        static string pair = "";
        static int qtdyContacts = 0;
        static int interval = 0;
        static int intervalOrder = 0;
        static int intervalCapture = 0;
        static double profit = 0;
        static double fee = 0;
        private static string bitmexDomain = "";
        static BitMEX.BitMEXApi bitMEXApi = null;

        static List<IIndicator> lstIndicatorsAll = new List<IIndicator>();
        static List<IIndicator> lstIndicatorsEntry = new List<IIndicator>();
        static List<IIndicator> lstIndicatorsEntryCross = new List<IIndicator>();

        static double[] arrayPriceClose = new double[100];
        static double[] arrayPriceHigh = new double[100];
        static double[] arrayPriceLow = new double[100];
        static double[] arrayPriceVolume = new double[100];
        static double[] arrayPriceOpen = new double[100];

        public static string SendResponse(HttpListenerRequest request)
        {
            lock (data)
            {
                try
                {
                    System.Data.DataSet ds = new System.Data.DataSet();
                    ds.ReadXml(location + "bd.xml");

                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("<html><head><title>BITMEX DASHBOARD</title>");
                    sb.AppendLine("<link rel='stylesheet' href='https://stackpath.bootstrapcdn.com/bootstrap/4.1.3/css/bootstrap.min.css' integrity='sha384-MCw98/SFnGE8fJT3GXwEOngsV7Zt27NXFoaoApmYm81iuXoPkFOJwJ8ERdknLPMO' crossorigin='anonymous'>");
                    sb.AppendLine("<script type='text/javascript' src='https://www.gstatic.com/charts/loader.js'></script>");
                    sb.AppendLine("</head><body><nav class='navbar navbar-expand-md navbar-dark bg-dark fixed-top'><a class='navbar-brand' href='#'>Deleron</a></nav>");
                    sb.AppendLine(" <main role='main' class='container'><br/><br/><br/>Deleron - <b>Back to the future</b> - v" + version + " - <b>BITMEX</b> version");
                    sb.AppendLine(" - by <b>Matheus Grijo</b> | ");
                    sb.AppendLine("<b>GITHUB</b> http://github.com/matheusgrijo<br/>");
                    sb.AppendLine("<hr>");

                    sb.AppendLine("Status: <b>running</b><br/>");
                    sb.AppendLine("Last update: <b>" + DateTime.Now.ToString() + "</b><br/>");
                    sb.AppendLine("OpenOrders: <b>" + ds.Tables[1].Rows[0]["Value"].ToString() + "</b><br/>");
                    sb.AppendLine("Amount: <b>" + ds.Tables[1].Rows[1]["Value"].ToString() + "</b><br/>");

                    try
                    {

                        String graph = "";

                        for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                        {
                            graph += "['" + ds.Tables[0].Rows[i][0].ToString() + "'," + ds.Tables[0].Rows[i][2].ToString() + "],";
                        }
                        graph = graph.Substring(0, graph.Length - 1);


                        sb.Append("<script>google.charts.load('current', {packages: ['corechart', 'line']}); " +
                                       " google.charts.setOnLoadCallback(drawBasic); " +

                        " function drawBasic() {" +


                            " var data = new google.visualization.DataTable();" +
                           " data.addColumn('string', 'Time');" +
                          "  data.addColumn('number', '" + "BTC" + "');" +

                           " data.addRows([ " +

                              graph +
                  "]); " +

                  "var options = {" +
                   " hAxis: {" +
                   "   title: 'Time'" +
                   " }," +
                   " vAxis: {" +
                   "   title: '" + "BTC" + "'" +
                   " }" +
                  "};" +

                    "var chart = new google.visualization.LineChart(document.getElementById('chart_div'));" +

                    "chart.draw(data, options);" +
                "}; </script><div id='chart_div'></div>");
                    }
                    catch
                    { }


                    String indicatorsEntry = "";
                    String indicatorsEntryCross = "";

                    foreach (var item in lstIndicatorsEntry)                    
                        indicatorsEntry += item.getName() + " ";

                    foreach (var item in lstIndicatorsEntryCross)
                        indicatorsEntryCross += item.getName() + " ";

                    sb.AppendLine("Indicators Entry: <b>" + indicatorsEntry + "</b><br/>");
                    sb.AppendLine("Indicators Entry Cross: <b>" + indicatorsEntryCross + "</b><br/>");
                    

                    sb.AppendLine(" </main> <footer class='footer'>" +
      "<div class='container'>");


                    sb.AppendLine("<br/><b>DONATE</b> <br/>");
                    sb.AppendLine("<b>BTC</b> 39DWjHHGXJh9q82ZrxkA8fiZoE37wL8jgh<br/>");
                    sb.AppendLine("<b>BCH</b> qqzwkd4klrfafwvl7ru7p7wpyt5z3sjk6y909xq0qk<br/>");
                    sb.AppendLine("<b>ETH</b> 0x3017E79f460023435ccD285Ff30Bd10834D20777<br/>");
                    sb.AppendLine("<b>ETC</b> 0x088E7E67af94293DB55D61c7B55E2B098d2258D9<br/>");
                    sb.AppendLine("<b>LTC</b> MVT8fxU4WBzdfH5XgvRPWkp7pE4UyzG9G5<br/></div></footer>");

                    sb.AppendLine("</body></html>");

                    return sb.ToString();
                }
                catch
                {
                    return "ERROR";
                }
            }
        }


        static Object data = new Object();
        public static void captureData()
        {
            lock (data)
            {
                try
                {
                    System.Data.DataSet ds = null;
                    bool create = false;
                    if (!System.IO.File.Exists(location + "bd.xml"))
                    {
                        System.Data.DataTable dt = new System.Data.DataTable("Balances");
                        dt.Columns.Add("Date");
                        dt.Columns.Add("Coin");
                        dt.Columns.Add("Amount");

                        dt.Rows.Add("", "", "");

                        System.Data.DataTable dtParameters = new System.Data.DataTable("Parameters");
                        dtParameters.Columns.Add("Parameter");
                        dtParameters.Columns.Add("Value");
                        dtParameters.Rows.Add("", "");


                        ds = new System.Data.DataSet();
                        ds.DataSetName = "Database";
                        ds.Tables.Add(dt);
                        ds.Tables.Add(dtParameters);
                        ds.WriteXml(location + "bd.xml");
                        create = true;
                    }

                    ds = new System.Data.DataSet();
                    ds.ReadXml(location + "bd.xml");

                    string json = bitMEXApi.GetWallet();
                    JContainer jCointaner = (JContainer)JsonConvert.DeserializeObject(json, (typeof(JContainer)));

                    if (create)
                        ds.Tables[0].Rows.Clear();

                    ds.Tables[0].Rows.Add(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), pair, jCointaner["amount"].ToString());

                    ds.Tables[1].Rows.Clear();
                    ds.Tables[1].Rows.Add("OpenOrders", bitMEXApi.GetOpenOrders(pair).Count);
                    ds.Tables[1].Rows.Add("Amount", jCointaner["amount"].ToString());


                    System.IO.File.Delete(location + "bd.xml");
                    ds.WriteXml(location + "bd.xml");

                }
                catch
                {

                }
            }
        }

        static void captureDataJob()
        {
            while (true)
            {
                try
                {
                    captureData();
                }
                catch
                {

                }
                System.Threading.Thread.Sleep(intervalCapture);
            }
        }


        static void getCandles()
        {
            try
            {
                arrayPriceClose = new double[100];
                arrayPriceHigh = new double[100];
                arrayPriceLow = new double[100];
                arrayPriceVolume = new double[100];
                arrayPriceOpen = new double[100];
                List<BitMEX.Candle> lstCandle = bitMEXApi.GetCandleHistory(pair, 100, timeGraph);
                int i = 0;
                foreach (var candle in lstCandle)
                {
                    arrayPriceClose[i] = (double)candle.Close;
                    arrayPriceHigh[i] = (double)candle.High;
                    arrayPriceLow[i] = (double)candle.Low;
                    arrayPriceVolume[i] = (double)candle.Volume;
                    arrayPriceOpen[i] = (double)candle.Open;
                    i++;
                }
                Array.Reverse(arrayPriceClose);
                Array.Reverse(arrayPriceHigh);
                Array.Reverse(arrayPriceLow);
                Array.Reverse(arrayPriceVolume);
                Array.Reverse(arrayPriceOpen);

                Console.Title = DateTime.Now.ToString() + " - " + pair + " - $ " + arrayPriceClose[99].ToString() + " v" + version;
            }
            catch (Exception ex)
            {
                log("GETCANDLES::" + ex.Message + ex.StackTrace);
            }

        }

        public static void Main(string[] args)
        {
            try
            {
                //Config
                log("Deleron - Back to the future - v" + version + " - Bitmex version");
                log("by Matheus Grijo");
                log("GITHUB http://github.com/matheusgrijo");
                log(" ******* DONATE ********* ");
                log("BTC 39DWjHHGXJh9q82ZrxkA8fiZoE37wL8jgh");
                log("BCH qqzwkd4klrfafwvl7ru7p7wpyt5z3sjk6y909xq0qk");
                log("ETH 0x3017E79f460023435ccD285Ff30Bd10834D20777");
                log("ETC 0x088E7E67af94293DB55D61c7B55E2B098d2258D9");
                log("LTC MVT8fxU4WBzdfH5XgvRPWkp7pE4UyzG9G5");
                log("Load config...");

                String jsonConfig = System.IO.File.ReadAllText(location + "key.txt");
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
                intervalCapture = int.Parse(jCointaner["webserverIntervalCapture"].ToString());
                profit = double.Parse(jCointaner["profit"].ToString());
                fee = double.Parse(jCointaner["fee"].ToString());

                if (jCointaner["webserver"].ToString() == "enable")
                {
                    WebServer ws = new WebServer(SendResponse, jCointaner["webserverConfig"].ToString());
                    ws.Run();
                    System.Threading.Thread tCapture = new Thread(captureDataJob);
                    tCapture.Start();
                }

                bitMEXApi = new BitMEX.BitMEXApi(bitmexKey, bitmexSecret, bitmexDomain);

                log("OK!");

                log("Total open orders: " + bitMEXApi.GetOpenOrders(pair).Count);
                log("");
                log("Wallet: " + bitMEXApi.GetWallet());



                lstIndicatorsAll.Add(new IndicatorMFI());
                lstIndicatorsAll.Add(new IndicatorBBANDS());
                lstIndicatorsAll.Add(new IndicatorCCI());
                lstIndicatorsAll.Add(new IndicatorCMO());
                lstIndicatorsAll.Add(new IndicatorDI());
                lstIndicatorsAll.Add(new IndicatorDM());
                lstIndicatorsAll.Add(new IndicatorMA());
                lstIndicatorsAll.Add(new IndicatorMACD());
                lstIndicatorsAll.Add(new IndicatorMOM());
                lstIndicatorsAll.Add(new IndicatorPPO());
                lstIndicatorsAll.Add(new IndicatorROC());
                lstIndicatorsAll.Add(new IndicatorRSI());
                lstIndicatorsAll.Add(new IndicatorSAR());
                lstIndicatorsAll.Add(new IndicatorSTOCH());
                lstIndicatorsAll.Add(new IndicatorSTOCHRSI());
                lstIndicatorsAll.Add(new IndicatorTRIX());
                lstIndicatorsAll.Add(new IndicatorULTOSC());
                lstIndicatorsAll.Add(new IndicatorWILLR());

                foreach (var item in jCointaner["indicatorsEntry"])
                {
                    foreach (var item2 in lstIndicatorsAll)
                    {
                        if (item["name"].ToString().Trim().ToUpper() == item2.getName().Trim().ToUpper())
                        {
                            item2.setPeriod(int.Parse((item["period"].ToString().Trim().ToUpper())));
                            lstIndicatorsEntry.Add(item2);
                        }
                    }
                }

                foreach (var item in jCointaner["indicatorsEntryCross"])
                {
                    foreach (var item2 in lstIndicatorsAll)
                    {
                        if (item["name"].ToString().Trim().ToUpper() == item2.getName().Trim().ToUpper())
                        {
                            item2.setPeriod(int.Parse((item["period"].ToString().Trim().ToUpper())));
                            lstIndicatorsEntryCross.Add(item2);
                        }
                    }
                }

                while (true)
                {

                    getCandles();


                    string operation = "buy";

                    foreach (var item in lstIndicatorsEntry)
                    {
                        if (item.GetOperation(arrayPriceOpen, arrayPriceClose, arrayPriceLow, arrayPriceHigh, arrayPriceVolume) != Operation.buy)
                        {
                            operation = "nothing";
                            break;
                        }
                    }

                    if (operation == "buy")
                    {
                        //Prepare to long
                        getCandles();
                        while (true)
                        {
                            getCandles();
                            foreach (var item in lstIndicatorsEntryCross)
                            {
                                if (item.GetOperation(arrayPriceOpen, arrayPriceClose, arrayPriceLow, arrayPriceHigh, arrayPriceVolume) != Operation.buy)
                                {
                                    operation = "long";
                                    break;
                                }
                            }
                            log("wait " + interval + "ms");
                            Thread.Sleep(interval);
                        }
                    }


                    operation = "sell";

                    foreach (var item in lstIndicatorsEntry)
                    {
                        if (item.GetOperation(arrayPriceOpen, arrayPriceClose, arrayPriceLow, arrayPriceHigh, arrayPriceVolume) != Operation.sell)
                        {
                            operation = "nothing";
                            break;
                        }
                    }

                    if (operation == "sell")
                    {
                        //Prepare to long
                        getCandles();
                        while (true)
                        {
                            getCandles();
                            foreach (var item in lstIndicatorsEntryCross)
                            {
                                if (item.GetOperation(arrayPriceOpen, arrayPriceClose, arrayPriceLow, arrayPriceHigh, arrayPriceVolume) != Operation.sell)
                                {
                                    operation = "short";
                                    break;
                                }
                            }
                            log("wait " + interval + "ms");
                            Thread.Sleep(interval);
                        }
                    }


                    //Strategy cross EMA and RSI
                    if (operation == "long")
                    {
                        makeOrder("Buy");
                    }
                    if (operation == "short")
                    {
                        makeOrder("Sell");
                    }

                    log("wait " + interval + "ms");
                    Thread.Sleep(interval);

                }



            }
            catch (Exception ex)
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



        static void log(string value)
        {
            try
            {
                value = "[" + DateTime.Now.ToString() + "] - " + value;
                System.IO.StreamWriter w = new StreamWriter(location + DateTime.Now.ToString("yyyyMMdd") + "_log.txt", true);
                w.WriteLine(value);
                Console.WriteLine(value);
                w.Close();
                w.Dispose();
            }
            catch { }
        }



    }
}
