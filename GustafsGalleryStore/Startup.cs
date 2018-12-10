using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GustafsGalleryStore.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity.UI.Services;
using GustafsGalleryStore.Areas.Identity.Data;
using GustafsGalleryStore.Services;
using GustafsGalleryStore.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using IEmailSender = GustafsGalleryStore.Services.IEmailSender;
using GustafsGalleryStore.Helpers;
using Newtonsoft.Json;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Logging;
using ElmahCore.Mvc;

namespace GustafsGalleryStore
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddDbContext<GustafsGalleryStoreContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("GustafsGalleryStoreContextConnection")));

            services.AddIdentity<ApplicationUser, IdentityRole>()
            //services.AddDefaultIdentity<User>()
                .AddEntityFrameworkStores<GustafsGalleryStoreContext>()
                .AddDefaultTokenProviders();

            services.AddAuthentication()
                    //.AddFacebook(facebookOptions => {
                        //facebookOptions.AppId = MasterStrings.FacebookApiId;
                        //facebookOptions.AppSecret = MasterStrings.FacebookSecretKey;
                        //})
                    .AddTwitter(twitterOptions => {
                        twitterOptions.ConsumerKey = MasterStrings.TwitterApiId;
                        twitterOptions.ConsumerSecret = MasterStrings.TwitterSecrectKey;
                        })
                    .AddGoogle(googleOptions => {
                        googleOptions.ClientId = MasterStrings.GoogleApiId;
                        googleOptions.ClientSecret = MasterStrings.GoogleSecretKey;
                        })
                    .AddMicrosoftAccount(microsoftOptions => {
                        microsoftOptions.ClientId = MasterStrings.MicrosoftApiId;
                        microsoftOptions.ClientSecret = MasterStrings.MicrosoftSecretKey;
                        })
                    .AddGitHub(githubOptions => { 
                        githubOptions.ClientId = MasterStrings.GitHubApiId;
                        githubOptions.ClientSecret = MasterStrings.GithubSecretKey;
                        })
                    .AddLinkedIn(linkedinOptions => {
                        linkedinOptions.ClientId = MasterStrings.LinkedInApiId;
                        linkedinOptions.ClientSecret = MasterStrings.LinkedInSecretKey;
                        });

            services.AddMvc(config => config.Filters.Add(new ValidateModelAttribute())).SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
                .AddRazorPagesOptions(options =>
                {
                    options.AllowAreas = true;
                    options.Conventions.AuthorizeAreaFolder("Identity", "/Account/Manage");
                    options.Conventions.AuthorizeAreaPage("Identity", "/Account/Logout");
            });

            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressConsumesConstraintForFormFileParameters = true;
                options.SuppressInferBindingSourcesForParameters = true;
                options.SuppressModelStateInvalidFilter = true;
            });

            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = $"/Identity/Account/Login";
                options.LogoutPath = $"/Identity/Account/Logout";
                options.AccessDeniedPath = $"/Identity/Account/AccessDenied";
                options.Cookie.Name = "GustafsGalleryAuth";
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
                // ReturnUrlParameter requires 
                //using Microsoft.AspNetCore.Authentication.Cookies;
                options.ReturnUrlParameter = CookieAuthenticationDefaults.ReturnUrlParameter;
                options.SlidingExpiration = true;
            });

             //using Microsoft.AspNetCore.Identity.UI.Services;
            services.AddSingleton<GustafsGalleryStore.Services.IEmailSender, EmailSender>();
            services.Configure<AuthMessageSenderOptions>(Configuration);
            services.AddElmah(options =>
            {
                options.CheckPermissionAction = context => context.User.IsInRole(MasterStrings.StaffRole);
            });

            //services.Configure<ForwardedHeadersOptions>(options =>
            //{
            //    options.ForwardedHeaders =
            //        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
            //});

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            //app.Use((context, next) =>
            //{
            //    context.Request.Scheme = "https";
            //    return next();
            //});

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedProto
            });

            //app.Run(async (context) =>
            //{
            //    context.Response.ContentType = "text/plain";

            //    // Request method, scheme, and path
            //    await context.Response.WriteAsync(
            //        $"Request Method: {context.Request.Method}{Environment.NewLine}");
            //    await context.Response.WriteAsync(
            //        $"Request Scheme: {context.Request.Scheme}{Environment.NewLine}");
            //    await context.Response.WriteAsync(
            //        $"Request Path: {context.Request.Path}{Environment.NewLine}");

            //    // Headers
            //    await context.Response.WriteAsync($"Request Headers:{Environment.NewLine}");

            //    foreach (var header in context.Request.Headers)
            //    {
            //        await context.Response.WriteAsync($"{header.Key}: " +
            //            $"{header.Value}{Environment.NewLine}");
            //    }

            //    await context.Response.WriteAsync(Environment.NewLine);

            //    // Connection: RemoteIp
            //    await context.Response.WriteAsync(
            //        $"Request RemoteIp: {context.Connection.RemoteIpAddress}");
            //});

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            //app.UseForwardedHeaders();
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseAuthentication();
            app.UseElmah();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
