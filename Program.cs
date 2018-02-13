using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace NetDimension.NanUI
{
    static class Program
    {
		[STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("zh-CN");

            if (Bootstrap.Load(PlatformArch.Auto, System.IO.Path.Combine(Application.StartupPath, "fx"), System.IO.Path.Combine(Application.StartupPath, "fx\\Resources"), System.IO.Path.Combine(Application.StartupPath, "fx\\Resources\\locales")))
            {
                //Register html/css/javascript/image resources in current executing assembly. 
                //If you want to embed any kind of resource in your app, just add it to your project and set the Build Action to Embedded Resource.
                //System.Reflection.Assembly.GetExecutingAssembly();


                Bootstrap.RegisterAssemblyResources(System.Reflection.Assembly.GetExecutingAssembly());

                Application.Run(new HuobiSocket());
            }
        }
    }
}
