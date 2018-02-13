using Chromium;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace NetDimension.NanUI
{
    public partial class HuobiSocket : NetDimension.NanUI.Formium
    {
        private bool isFirst = true;
        public HuobiSocket() : base(null, false)
        {
            InitializeComponent();

            this.LoadHandler.OnLoadEnd += (s, e) =>
            {
                if (isFirst)
                {
                    isFirst = false;
                    ShowDevTools();
                    this.ExecuteJavascript("location.href = 'https://www.huobi.pro/zh-cn/btc_usdt/exchange/'");
                }
            };

            this.GlobalObject.AddFunction("saveData").Execute += (s, e) =>
            {
                try
                {
                    var price = e.Arguments[0].StringValue;
                    var amt = e.Arguments[1].StringValue;
                    var time = e.Arguments[2].StringValue;
                    //Console.WriteLine($"{price}|{amt}|{time}");
                    Insert(price, time);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            };



            this.RequestHandler.GetResourceHandler += (sender, e) =>
            {
                if (e.Request.Url.Contains("https://www.huobi.pro/assets/scripts/exchange"))
                {
                    using (var web = new System.Net.WebClient()
                    {
                        Encoding = Encoding.UTF8
                    })
                    {
                        var html = web.DownloadString(e.Request.Url);

                        var index = html.IndexOf("var e=t.rep?\"rep\":t.unsubbed?\"unsubbed\":\"ch\",");

                        html = html.Insert(index, @"
                        if(t.ch == ""market.btcusdt.trade.detail""){
                        	for(var index = 0;index < t.tick.data.length;index++)
                        	{
                        		var date = new Date(t.tick.data[index].ts)
                        		var dateStr = date.getFullYear() + ""-"" + (date.getMonth() + 1) + ""-"" + date.getDate() + "" "" + date.getHours() + "":"" + date.getMinutes() + "":"" + date.getSeconds() + ""."" + date.getMilliseconds();
                        		window.top.saveData(t.tick.data[index].price + """",t.tick.data[index].amount + """",dateStr)
                        	}
                        }
                        ");

                        //System.IO.File.WriteAllText("C:\\1.js", html);

                        Chromium.CfxResourceHandler handler = new Chromium.CfxResourceHandler();
                        byte[] data = new byte[0];
                        handler.ProcessRequest += (o, ent) =>
                                            {
                                                //data = System.IO.File.ReadAllBytes(@"C:\Users\Cheney\Desktop\exchange_8532b9a8.js");
                                                data = System.Text.Encoding.UTF8.GetBytes(html);
                                                ent.Callback.Continue();
                                                ent.SetReturnValue(true);
                                            };
                        handler.GetResponseHeaders += (o, ent) =>
                                                {
                                                    ent.ResponseLength = -1;
                                                    ent.Response.MimeType = "text/plain";
                                                    ent.Response.Status = 200;
                                                };
                        int readResponseStreamOffset = 0;
                        handler.ReadResponse += (o, ent) =>
                                                {
                                                    if (readResponseStreamOffset >= data.Length)
                                                    {
                                                        ent.SetReturnValue(false);
                                                        return;
                                                    }
                                                    int bytesToCopy = data.Length - readResponseStreamOffset;
                                                    if (bytesToCopy > ent.BytesToRead)
                                                        bytesToCopy = ent.BytesToRead;
                                                    System.Runtime.InteropServices.Marshal.Copy(data, readResponseStreamOffset, ent.DataOut, bytesToCopy);
                                                    ent.BytesRead = bytesToCopy;
                                                    readResponseStreamOffset += bytesToCopy;
                                                    ent.SetReturnValue(true);
                                                };

                        e.SetReturnValue(handler);
                    }
                }

            };
        }


        public void ShowDevTools()
        {
            CfxWindowInfo windowInfo = new CfxWindowInfo();

            windowInfo.Style = WindowStyle.WS_OVERLAPPEDWINDOW | WindowStyle.WS_CLIPCHILDREN | WindowStyle.WS_CLIPSIBLINGS | WindowStyle.WS_VISIBLE & ~WindowStyle.WS_CAPTION;
            windowInfo.ParentWindow = IntPtr.Zero;
            windowInfo.WindowName = "Dev Tools";
            windowInfo.X = 200;
            windowInfo.Y = 200;
            windowInfo.Width = 720;
            windowInfo.Height = 480;

            this.BrowserHost.ShowDevTools(windowInfo, new CfxClient(), new CfxBrowserSettings(), null);
        }

        public static string CONNSTR => System.Configuration.ConfigurationManager.ConnectionStrings["CONN"].ConnectionString;

        public static void Insert(string price, string createtime)
        {
            try
            {
                using (var conn = new MySql.Data.MySqlClient.MySqlConnection(CONNSTR))
                using (var cmd = new MySql.Data.MySqlClient.MySqlCommand($"INSERT INTO N(PLATFORM,TYPE,PRICE,UNIT,DEAL_TIME) VALUE('Huobi','BTC','{price}','USDT','{createtime}')", conn))
                {
                    conn.Open();
                    //cmd.CommandText = ;
                    cmd.ExecuteNonQuery();
                }
                Console.WriteLine($"[DB]PRICE:{price};TIME{createtime}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DB][ERROR]{ex.Message.Replace("\n", " ").Replace("\r", " ")}");
                try
                {
                    System.IO.File.AppendAllText($"{System.Environment.CurrentDirectory}/error{DateTime.Now.ToString("yyyyMMdd")}.log", $"[DB][ERROR]{ex.Message.Replace("\n", " ").Replace("\r", " ")}\r\n");
                }
                catch
                {
                }
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            //new BitstampSocket().Show();
            //this.Hide();
            //this.Invoke(new Action(() =>
            //{
            //    new BitstampSocket().Show();
            //}));
        }

    }
}
