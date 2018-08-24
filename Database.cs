using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Database
{
    public static void captureDataJob()
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
            System.Threading.Thread.Sleep(MainClass.intervalCapture);
        }
    }

    public static void captureData()
    {
        lock (MainClass.data)
        {
            try
            {
                System.Data.DataSet ds = null;
                bool create = false;
                if (!System.IO.File.Exists(MainClass.location + "bd.xml"))
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
                    ds.WriteXml(MainClass.location + "bd.xml");
                    create = true;
                }

                ds = new System.Data.DataSet();
                ds.ReadXml(MainClass.location + "bd.xml");

                BitMEX.BitMEXApi bitMEXApi = new BitMEX.BitMEXApi(MainClass.bitmexKeyWeb, MainClass.bitmexSecretWeb, MainClass.bitmexDomain);
                string json = bitMEXApi.GetWallet();
                JContainer jCointaner = (JContainer)JsonConvert.DeserializeObject(json, (typeof(JContainer)));

                if (create)
                    ds.Tables[0].Rows.Clear();

                ds.Tables[0].Rows.Add(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), MainClass.pair, jCointaner[0]["walletBalance"].ToString());

                ds.Tables[1].Rows.Clear();
                ds.Tables[1].Rows.Add("OpenOrders", bitMEXApi.GetOpenOrders(MainClass.pair).Count);
                ds.Tables[1].Rows.Add("Amount", jCointaner[0]["walletBalance"].ToString());


                System.IO.File.Delete(MainClass.location + "bd.xml");
                ds.WriteXml(MainClass.location + "bd.xml");

            }
            catch
            {

            }
        }
    }
}
