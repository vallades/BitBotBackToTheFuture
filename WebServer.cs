using System;
using System.Net;
using System.Threading;
using System.Linq;
using System.Text;


public class WebServer
{
    public static string SendResponse(HttpListenerRequest request)
    {
        lock (MainClass.data)
        {
            try
            {
                System.Data.DataSet ds = new System.Data.DataSet();
                ds.ReadXml(MainClass.location + "bd.xml");

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("<html><head><title>BITMEX DASHBOARD</title>");
                sb.AppendLine("<link rel='stylesheet' href='https://stackpath.bootstrapcdn.com/bootstrap/4.1.3/css/bootstrap.min.css' integrity='sha384-MCw98/SFnGE8fJT3GXwEOngsV7Zt27NXFoaoApmYm81iuXoPkFOJwJ8ERdknLPMO' crossorigin='anonymous'>");
                sb.AppendLine("<script type='text/javascript' src='https://www.gstatic.com/charts/loader.js'></script>");
                sb.AppendLine("<meta http-equiv='refresh' content='120' ></head><body><nav class='navbar navbar-expand-md navbar-dark bg-dark fixed-top'><a class='navbar-brand' href='#'>Botmex.Ninja</a></nav>");
                sb.AppendLine(" <main role='main' class='container'><br/><br/><br/> <div class='row'><div class='col-sm'><img src='http://botmex.ninja/img/logo.png' /></div><div class='col-sm'>Deleron - <b>Back to the future</b> - v" + MainClass.version + " - <b>BITMEX</b> version");
                sb.AppendLine(" - by <b>Matheus Grijo</b> | ");
                sb.AppendLine("<b>GITHUB</b> http://github.com/matheusgrijo<br/><i>Auto refresh every 120 seconds</i></div></div>");
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

                    double perc = 0;

                    try { perc = ((double.Parse(ds.Tables[0].Rows[ds.Tables[0].Rows.Count - 1][2].ToString()) * 100) / double.Parse(ds.Tables[0].Rows[0][2].ToString())) - 100; }
                    catch { }

                    graph = graph.Substring(0, graph.Length - 1);

                    sb.AppendLine("Profit: <b>" + perc + "%</b><br/>");
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
                String indicatorsEntryDecision= "";

                foreach (var item in MainClass.lstIndicatorsEntry)
                    indicatorsEntry += item.getName() + " ";

                foreach (var item in MainClass.lstIndicatorsEntryCross)
                    indicatorsEntryCross += item.getName() + " ";
                foreach (var item in MainClass.lstIndicatorsEntryDecision)
                    indicatorsEntryDecision += item.getName() + " ";

                sb.AppendLine("Indicators Entry: <b>" + indicatorsEntry + "</b><br/>");
                sb.AppendLine("Indicators Entry Cross: <b>" + indicatorsEntryCross + "</b><br/>");
                sb.AppendLine("Indicators Entry Decision: <b>" + indicatorsEntryDecision + "</b><br/>");


                sb.AppendLine(" </main> <footer class='footer'>" +
  "<div class='container'>");


                sb.AppendLine("<br/> <div class='row'><div class='col-sm'><b>DONATE</b> <br/>");
                sb.AppendLine("<b>BTC</b> 39DWjHHGXJh9q82ZrxkA8fiZoE37wL8jgh<br/>");
                sb.AppendLine("<b>BCH</b> qqzwkd4klrfafwvl7ru7p7wpyt5z3sjk6y909xq0qk<br/>");
                sb.AppendLine("<b>ETH</b> 0x3017E79f460023435ccD285Ff30Bd10834D20777<br/>");
                sb.AppendLine("<b>ETC</b> 0x088E7E67af94293DB55D61c7B55E2B098d2258D9<br/>");
                sb.AppendLine("<b>LTC</b> MVT8fxU4WBzdfH5XgvRPWkp7pE4UyzG9G5</div><div class='col-sm'><center><img src='http://botmex.ninja/img/donate.png'width='200px' heigth='200px' /></center></div></div></div></footer>");

                sb.AppendLine("</body></html>");

                return sb.ToString();
            }
            catch(Exception ex)
            {
                return "<html><head><meta http-equiv='refresh' content='5' ><title>Wait...</title></head><body><center><img src='http://botmex.ninja/img/logo.png' /><br/>Wait...</center></body></html> ";
            }
        }
    }

    private readonly HttpListener _listener = new HttpListener();
    private readonly Func<HttpListenerRequest, string> _responderMethod;

    public WebServer(string[] prefixes, Func<HttpListenerRequest, string> method)
    {
        if (!HttpListener.IsSupported)
            throw new NotSupportedException(
                "Needs Windows XP SP2, Server 2003 or later.");

        // URI prefixes are required, for example 
        // "http://localhost:8080/index/".
        if (prefixes == null || prefixes.Length == 0)
            throw new ArgumentException("prefixes");

        // A responder method is required
        if (method == null)
            throw new ArgumentException("method");

        foreach (string s in prefixes)
            _listener.Prefixes.Add(s);

        _responderMethod = method;
        _listener.Start();
    }

    public WebServer(Func<HttpListenerRequest, string> method, params string[] prefixes)
        : this(prefixes, method) { }

    public void Run()
    {
        ThreadPool.QueueUserWorkItem((o) =>
        {
            Console.WriteLine("Webserver running...");
            try
            {
                while (_listener.IsListening)
                {
                    ThreadPool.QueueUserWorkItem((c) =>
                    {
                        var ctx = c as HttpListenerContext;
                        try
                        {
                            string rstr = _responderMethod(ctx.Request);
                            byte[] buf = Encoding.UTF8.GetBytes(rstr);
                            ctx.Response.ContentLength64 = buf.Length;
                            ctx.Response.OutputStream.Write(buf, 0, buf.Length);
                        }
                        catch { } // suppress any exceptions
                            finally
                        {
                                // always close the stream
                                ctx.Response.OutputStream.Close();
                        }
                    }, _listener.GetContext());
                }
            }
            catch { } // suppress any exceptions
            });
    }

    public void Stop()
    {
        _listener.Stop();
        _listener.Close();
    }
}
