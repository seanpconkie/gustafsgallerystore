using System.Linq;
using GustafsGalleryStore.Areas.Identity.Data;
using GustafsGalleryStore.Models.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using GustafsGalleryStore.Models.DataModels;
using IEmailSender = GustafsGalleryStore.Services.IEmailSender;
using Microsoft.AspNetCore.Authorization;
using GustafsGalleryStore.Helpers;
using System.Collections.Generic;
using System;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace GustafsGalleryStore.Controllers
{
    public class SitemapController : Controller
    {
        #region private variables

        private IHostingEnvironment _hostingEnvironment;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ManageStaffController> _logger;
        private readonly IEmailSender _emailSender;
        private readonly GustafsGalleryStoreContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;
        private const string _storeName = "GustafsGalleryStore";

        #endregion

        #region constructor

        public SitemapController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<ManageStaffController> logger,
            IEmailSender emailSender,
            IHostingEnvironment hostingEnvironment,
            GustafsGalleryStoreContext context,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _context = context;
            _roleManager = roleManager;
            _hostingEnvironment = hostingEnvironment;
        }

        #endregion

        [Route("sitemap")]
        public IActionResult Sitemap()
        {
            string baseUrl = "https://gustafsgallery.co.uk";

            // get a list of products
            var products = _context.Products.
                                   Include(x => x.ProductBrand).
                                   ToList();

            // get last modified date of the home page
            var siteMapBuilder = new SitemapBuilder();

            // add the home page to the sitemap
            siteMapBuilder.AddUrl(baseUrl, modified: DateTime.Now, changeFrequency: ChangeFrequency.Weekly, priority: 1.0);

            // add the blog posts to the sitemap
            foreach (var product in products)
            {
                siteMapBuilder.AddUrl(string.Format("{0}/Products/Product?id={1}",baseUrl,product.Id), modified: product.CreateDate, changeFrequency: null, priority: 0.9);
                siteMapBuilder.AddUrl(string.Format("{0}/Products/ProductByName?product={1}", baseUrl, product.Title.Replace(' ','_') + "_by_" + product.ProductBrand.Brand.Replace(' ','_')), modified: product.CreateDate, changeFrequency: null, priority: 0.9);
            }

            siteMapBuilder.AddUrl(string.Format("{0}/Products", baseUrl), modified: DateTime.Now, changeFrequency: ChangeFrequency.Weekly, priority: 0.9);
            siteMapBuilder.AddUrl(string.Format("{0}/Orders", baseUrl), modified: DateTime.Now, changeFrequency: ChangeFrequency.Weekly, priority: 0.9);
            siteMapBuilder.AddUrl(string.Format("{0}/Home/{1}", baseUrl,"Privacy"), modified: DateTime.Now, changeFrequency: ChangeFrequency.Weekly, priority: 0.9);
            siteMapBuilder.AddUrl(string.Format("{0}/Home/{1}", baseUrl, "TermsAndConditions"), modified: DateTime.Now, changeFrequency: ChangeFrequency.Weekly, priority: 0.9);
            siteMapBuilder.AddUrl(string.Format("{0}/Home/{1}", baseUrl, "CookiePolicy"), modified: DateTime.Now, changeFrequency: ChangeFrequency.Weekly, priority: 0.9);
            siteMapBuilder.AddUrl(string.Format("{0}/Home/{1}", baseUrl, "AnimalFayreDesigns"), modified: DateTime.Now, changeFrequency: ChangeFrequency.Weekly, priority: 0.9);
            siteMapBuilder.AddUrl(string.Format("{0}/Home/{1}", baseUrl, "JackConkieIllustrations"), modified: DateTime.Now, changeFrequency: ChangeFrequency.Weekly, priority: 0.9);
            siteMapBuilder.AddUrl(string.Format("{0}/Home/{1}", baseUrl, "SilverBoughJewellery"), modified: DateTime.Now, changeFrequency: ChangeFrequency.Weekly, priority: 0.9);
            siteMapBuilder.AddUrl(string.Format("{0}/Home/{1}", baseUrl, "Contact"), modified: DateTime.Now, changeFrequency: ChangeFrequency.Weekly, priority: 0.9);

            // generate the sitemap xml
            string xml = siteMapBuilder.ToString();
            return Content(xml, "text/xml");
        }
    }
}
