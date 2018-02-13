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
    public partial class BitstampSocket : NetDimension.NanUI.Formium
    {
        public BitstampSocket() : base("https://www.bitstamp.net/", false)
        {
            InitializeComponent();

            this.LoadHandler.OnLoadEnd += (s, e) =>
            {
                //this.ShowDevTools();
            };

            this.GlobalObject.AddFunction("saveData").Execute += (s, e) =>
            {
                var price = e.Arguments[0].StringValue;
                var amt = e.Arguments[1].StringValue;
                var time = e.Arguments[2].StringValue;
                Console.WriteLine($"{price}|{amt}|{time}");
            };

            this.RequestHandler.GetResourceHandler += (sender, e) =>
            {
                if (e.Request.Url.Contains("https://www.bitstamp.net/s/js/bitstamp-front.js"))
                {
                    Chromium.CfxResourceHandler handler = new Chromium.CfxResourceHandler();
                    byte[] data = new byte[0];
                    handler.ProcessRequest += (o, ent) =>
                    {
                        data = System.IO.File.ReadAllBytes(Application.StartupPath + "/bitstampPusher.txt");
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

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            //this.Hide();
        }
    }
}
