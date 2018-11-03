using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GustafsGalleryStore
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                        //.UseKestrel(options => {
                        //    //options.Listen(IPAddress.Loopback, 5000);
                        //    //options.Listen(IPAddress.Any, 80);
                        //    options.Listen(IPAddress.Loopback, 5001, listenOptions => listenOptions.UseHttps("localhost.pfx", "Nofear01!"));
                        //})
                        .UseStartup<Startup>();
        }
    }
}
