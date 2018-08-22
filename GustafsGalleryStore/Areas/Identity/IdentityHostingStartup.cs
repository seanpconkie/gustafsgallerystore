using System;
using GustafsGalleryStore.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using GustafsGalleryStore.Areas.Identity.Data;

[assembly: HostingStartup(typeof(GustafsGalleryStore.Areas.Identity.IdentityHostingStartup))]
namespace GustafsGalleryStore.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) => {
                services.AddDbContext<GustafsGalleryStoreContext>(options =>
                    options.UseSqlServer(
                        context.Configuration.GetConnectionString("GustafsGalleryStoreContextConnection")));

                //services.AddDefaultIdentity<User>()
                    //.AddEntityFrameworkStores<GustafsGalleryStoreContext>();
            });
        }
    }
}