using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using GustafsGalleryStore.Areas.Identity.Data;
using GustafsGalleryStore.Models.DataModels;
using GustafsGalleryStore.Models.ViewModels;
using GustafsGalleryStore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using IEmailSender = GustafsGalleryStore.Services.IEmailSender;
using GustafsGalleryStore.Helpers;
using System;

namespace GustafsGalleryStore.Controllers
{
    public class HomeController : Controller
    {
        private IHostingEnvironment _hostingEnvironment;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ManageStaffController> _logger;
        private readonly IEmailSender _emailSender;
        private readonly GustafsGalleryStoreContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;
        private const string _storeName = "GustafsGalleryStore";

        public HomeController(
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

        public string ReturnUrl { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        public IActionResult Index(string statusMessage = null, string successMessage = null, string failureMessage = null)
        {
            var viewModel = new ProductListViewModel() {
                Products = _context.Products.
                                OrderByDescending(p => p.CreateDate).
                                Include(b => b.ProductBrand).
                                Include(i => i.ProductImages).
                                ToList(),
                StatusMessage = statusMessage,
                SuccessMessage = successMessage,
                FailureMessage = failureMessage
            };

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult CookiePolicy()
        {
            return View();
        }

        public IActionResult TermsAndConditions()
        {
            return View();
        }
        public IActionResult Contact(string statusMessage = null, string successMessage = null, string failureMessage = null)
        {
            var viewModel = new ContactViewModel() { StatusMessage = statusMessage, SuccessMessage = successMessage, FailureMessage = failureMessage};
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Contact(ContactViewModel model)
        {

            if (string.IsNullOrWhiteSpace(model.Name) ||
               string.IsNullOrWhiteSpace(model.Email) ||
               string.IsNullOrWhiteSpace(model.Subject) ||
               string.IsNullOrWhiteSpace(model.Message))
            {
                model.FailureMessage = "Please complete all fields.";

                return View(model);
            }

            var messageText = string.Format("Message from {0}\n{1}", model.Email, model.Message);
            await _emailSender.SendEmailAsync(MasterStrings.AdminEmail, model.Subject, messageText);

            var redirectUrl = string.Format("/Home/Contact?successMessage=Your message '{0}' has been sent.", model.Subject);
            return ControllerHelper.RedirectToLocal(this, redirectUrl);

        }

        public IActionResult ComingSoon()
        {
            return View();
        }

        public IActionResult AnimalFayreDesigns()
        {
            return View();
        }

        public IActionResult SilverBoughJewellery()
        {
            return View();
        }

        public IActionResult JackConkieIllustrations()
        {
            return View();
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }

    }
}
