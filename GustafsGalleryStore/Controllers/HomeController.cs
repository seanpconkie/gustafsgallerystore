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
                StatusMessage = statusMessage,
                SuccessMessage = successMessage,
                FailureMessage = failureMessage
            };

            var productList = new List<Product>();
            int maxId = Convert.ToInt32(_context.Products.Max(x => x.Id));

            for (int i = 0; i < 10; i++)
            {
                Random rnd = new Random();
                long productId = rnd.Next(maxValue: maxId);

                var product = _context.Products.
                                      Where(x => x.Id == productId).
                                      Include(x => x.ProductImages).
                                      SingleOrDefault();

                while (product == null || productList.Contains(product))
                {

                    productId = rnd.Next(maxValue: maxId);

                    product = _context.Products.
                                      Where(x => x.Id == productId).
                                      Include(x => x.ProductImages).
                                      SingleOrDefault();
                }

                if (product.ProductImages.Count == 0)
                {
                    product.ProductImages.Add(new ProductImage() { Uri = "https://farm5.staticflickr.com/4705/40336899591_bdc86eddb2_o.png" });
                }

                productList.Add(product);

            }

            viewModel.Products = productList;

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
               string.IsNullOrWhiteSpace(model.Message)
               )
            {
                model.FailureMessage = "Please complete all fields.";

                return View(model);
            }

            if (string.IsNullOrWhiteSpace(model.RecaptchaResponse))
            {
                model.FailureMessage = "Something went wrong, please try again.";

                return View(model);
            }

            if (!RecaptchaHelper.IsReCaptchValid(model.RecaptchaResponse))
            {
                model.FailureMessage = "Something went wrong, please try again.";

                return View(model);
            }

            var messageText = string.Format("Message from {0}{1}{2}", model.Email,System.Environment.NewLine, model.Message);
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
