using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using mentula_manducare.Objects;
using MentulaManducare;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Hosting;
using Newtonsoft.Json;
using Owin;

namespace mentula_manducare
{
    public static class WebSocketThread
    {
        public static UserCollection Users;
        public static void Run()
        {
            string url = "http://+:9922";
            //Start WebService
            WebApp.Start(url);
            
            MainThread.WriteLine($"SignalR Server running on {url}");
            Users = new UserCollection();
            while (true)
            {
                //Any Background tasks go here
                Thread.Sleep(60000);
            }
        }
    }
    class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseCors(CorsOptions.AllowAll);
            app.MapSignalR();
            
        }
    }
   
}
