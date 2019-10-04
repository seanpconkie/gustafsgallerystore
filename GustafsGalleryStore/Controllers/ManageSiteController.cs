using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using GustafsGalleryStore.Areas.Identity.Data;
using GustafsGalleryStore.Helpers;
using GustafsGalleryStore.Models.DataModels;
using GustafsGalleryStore.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using IEmailSender = GustafsGalleryStore.Services.IEmailSender;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace GustafsGalleryStore.Controllers
{
    [Authorize(Roles = MasterStrings.StaffRole)]
    public class ManageSiteController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ManageStaffController> _logger;
        private readonly IEmailSender _emailSender;
        private readonly GustafsGalleryStoreContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;

        public ManageSiteController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<ManageStaffController> logger,
            IEmailSender emailSender,
            GustafsGalleryStoreContext context,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _context = context;
            _roleManager = roleManager;
        }

        public string ReturnUrl { get; set; }

        // GET: /<controller>/
        public IActionResult Index(string statusMessage = null, string successMessage = null, string failureMessage = null)
        {
            var viewModel = new SiteViewModel() { 
                SuccessMessage = successMessage,
                StatusMessage = statusMessage,
                FailureMessage = failureMessage
            };
            return View(viewModel);
        }

        // GET: /<controller>/
        public IActionResult Brand(string statusMessage = null, string successMessage = null, string failureMessage = null)
        {
            var viewModel = new BrandViewModel() { 
                Brands = _context.ProductBrands.OrderBy(x => x.Brand).ToList(),
                SuccessMessage = successMessage,
                StatusMessage = statusMessage,
                FailureMessage = failureMessage
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Brand(BrandViewModel model)
        {

            string failureMessage = null;
            string successMessage = null;
            var redirectUrl = "/ManageSite/Brand";

            if (ModelState.IsValid)
            {
               
                var result = ManageSiteHelper.AddBrand(model,_context);

                if (result == UpdateResult.Error)
                {
                    failureMessage = "Brand couldn't be updated.";
                }
                else if (result == UpdateResult.Success)
                {
                    successMessage = "Brand updated.";
                }
                else if (result == UpdateResult.Duplicate)
                {
                    failureMessage = "Brand already exists.";
                }

            }

            if (!string.IsNullOrWhiteSpace(failureMessage))
            {
                redirectUrl += string.Format("?failureMessage={0}", failureMessage);
            }
            if (!string.IsNullOrWhiteSpace(successMessage))
            {
                redirectUrl += string.Format("?successMessage={0}", successMessage);
            }

            return ControllerHelper.RedirectToLocal(this, redirectUrl);

        }

        // GET: /<controller>/
        public IActionResult Colour(string statusMessage = null, string successMessage = null, string failureMessage = null)
        {
            var viewModel = new ColourViewModel() { 
                Colours = _context.Colours.OrderBy(x => x.Value).ToList(),
                SuccessMessage = successMessage,
                StatusMessage = statusMessage,
                FailureMessage = failureMessage
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Colour(ColourViewModel model)
        {

            string failureMessage = null;
            string successMessage = null;
            var redirectUrl = "/ManageSite/Colour";

            if (ModelState.IsValid)
            {

                var result = ManageSiteHelper.AddColour(model, _context);


                if (result == UpdateResult.Error)
                {
                    failureMessage = "Colour couldn't be updated.";
                }
                else if (result == UpdateResult.Success)
                {
                    successMessage = "Colour updated.";
                }
                else if (result == UpdateResult.Duplicate)
                {
                    failureMessage = "Colour already exists.";
                }
            }
        

            if (!string.IsNullOrWhiteSpace(failureMessage))
            {
                redirectUrl += string.Format("?failureMessage={0}", failureMessage);
            }
            if (!string.IsNullOrWhiteSpace(successMessage))
            {
                redirectUrl += string.Format("?successMessage={0}", successMessage);
            }

            return ControllerHelper.RedirectToLocal(this, redirectUrl);

        }

        // GET: /<controller>/
        public IActionResult Delivery(string statusMessage = null, string successMessage = null, string failureMessage = null)
        {
            var viewModel = new DeliveryViewModel(){};

            List<DeliveryType> types = _context.DeliveryTypes.Where(x => x.Id > 0).Include(x => x.DeliveryCompany).ToList();

            foreach (var type in types)
            {
                type.DeliveryCompany = _context.DeliveryCompanies.Where(x => x.Id == type.DeliveryCompanyId).SingleOrDefault();
            }

            viewModel.DeliveryTypes = types;

            return View(viewModel);
        }

        // GET: /<controller>/
        public IActionResult DeliveryType(long id = 0, string statusMessage = null, 
                                          string successMessage = null, 
                                          string failureMessage = null)
        {
            var viewModel = new DeliveryViewModel() { 
                DeliveryCompanies = _context.DeliveryCompanies.ToList(),
                SuccessMessage = successMessage,
                StatusMessage = statusMessage,
                FailureMessage = failureMessage
            };

            if (id > 0)
            {
                var inDb = _context.DeliveryTypes.
                                   Where(x => x.Id == id).
                                   Include(x => x.DeliveryCompany).
                                   SingleOrDefault();

                viewModel.Id = inDb.Id;
                viewModel.Time = inDb.Time;
                viewModel.Type = inDb.Type;
                viewModel.Price = inDb.Price;
                viewModel.Company = inDb.DeliveryCompany.Company;
                viewModel.CompanyId = inDb.DeliveryCompany.Id;
            }

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeliveryType(DeliveryViewModel model)
        {

            string failureMessage = null;
            string successMessage = null;
            var redirectUrl = "/ManageSite/DeliveryType";

            if (ModelState.IsValid)
            {

                var result = ManageSiteHelper.AddDeliveryType(model, _context);


                if (result == UpdateResult.Error)
                {
                    failureMessage = "Delivery Method couldn't be updated.";
                }
                else if (result == UpdateResult.Success)
                {
                    successMessage = "Delivery Method updated.";
                }
                else if (result == UpdateResult.Duplicate)
                {
                    failureMessage = "Delivery Method already exists.";
                }


            }

            if (!string.IsNullOrWhiteSpace(failureMessage))
            {
                redirectUrl += string.Format("?failureMessage={0}", failureMessage);
            }
            if (!string.IsNullOrWhiteSpace(successMessage))
            {
                redirectUrl += string.Format("?successMessage={0}", successMessage);
            }

            return ControllerHelper.RedirectToLocal(this, redirectUrl);

        }

        // GET: /<controller>/
        public IActionResult DeliveryCompany(string statusMessage = null, string successMessage = null, string failureMessage = null)
        {
            var viewModel = new DeliveryViewModel() { 
                DeliveryCompanies = _context.DeliveryCompanies.ToList(),
                SuccessMessage = successMessage,
                StatusMessage = statusMessage,
                FailureMessage = failureMessage
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeliveryCompany(DeliveryViewModel model)
        {

            string failureMessage = null;
            string successMessage = null;
            var redirectUrl = "/ManageSite/DeliveryCompany";
            if (ModelState.IsValid)
            {

                var result = ManageSiteHelper.AddDeliveryCompany(model, _context);


                if (result == UpdateResult.Error)
                {
                    failureMessage = "Delivery Company couldn't be updated.";
                }
                else if (result == UpdateResult.Success)
                {
                    successMessage = "Delivery Company updated.";
                }
                else if (result == UpdateResult.Duplicate)
                {
                    failureMessage = "Delivery Company already exists.";
                }

            }

            if (!string.IsNullOrWhiteSpace(failureMessage))
            {
                redirectUrl += string.Format("?failureMessage={0}", failureMessage);
            }
            if (!string.IsNullOrWhiteSpace(successMessage))
            {
                redirectUrl += string.Format("?successMessage={0}", successMessage);
            }

            return ControllerHelper.RedirectToLocal(this, redirectUrl);

        }

        // GET: /<controller>/
        public IActionResult Department(string statusMessage = null, string successMessage = null, string failureMessage = null)
        {
            var viewModel = new DepartmentViewModel() { 
                Departments = _context.Departments.OrderBy(x => x.DepartmentName).ToList(),
                SuccessMessage = successMessage,
                StatusMessage = statusMessage,
                FailureMessage = failureMessage
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Department(DepartmentViewModel model)
        {

            string failureMessage = null;
            string successMessage = null;
            var redirectUrl = "/ManageSite/Department";
            if (ModelState.IsValid)
            {
                var result = ManageSiteHelper.AddDepartment(model, _context);


                if (result == UpdateResult.Error)
                {
                    failureMessage = "Department couldn't be updated.";
                }
                else if (result == UpdateResult.Success)
                {
                    successMessage = "Department updated.";
                }
                else if (result == UpdateResult.Duplicate)
                {
                    failureMessage = "Department already exists.";
                }

            }

            if (!string.IsNullOrWhiteSpace(failureMessage))
            {
                redirectUrl += string.Format("?failureMessage={0}", failureMessage);
            }
            if (!string.IsNullOrWhiteSpace(successMessage))
            {
                redirectUrl += string.Format("?successMessage={0}", successMessage);
            }

            return ControllerHelper.RedirectToLocal(this, redirectUrl);

        }

        // GET: /<controller>/
        public IActionResult Size(string statusMessage = null, string successMessage = null, string failureMessage = null)
        {
            var viewModel = new SizeViewModel() { 
                Sizes = _context.Sizes.OrderBy(x => x.Value).ToList(),
                SuccessMessage = successMessage,
                StatusMessage = statusMessage,
                FailureMessage = failureMessage
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Size(SizeViewModel model)
        {

            string failureMessage = null;
            string successMessage = null;
            var redirectUrl = "/ManageSite/Size";

            if (ModelState.IsValid)
            {
               
                var result = ManageSiteHelper.AddSize(model, _context);


                if (result == UpdateResult.Error)
                {
                    failureMessage = "Size couldn't be updated.";
                }
                else if (result == UpdateResult.Success)
                {
                    successMessage = "Size updated.";
                }
                else if (result == UpdateResult.Duplicate)
                {
                    failureMessage = "Size already exists.";
                }

            }


            if (!string.IsNullOrWhiteSpace(failureMessage))
            {
                redirectUrl += string.Format("?failureMessage={0}", failureMessage);
            }
            if (!string.IsNullOrWhiteSpace(successMessage))
            {
                redirectUrl += string.Format("?successMessage={0}", successMessage);
            }

            return ControllerHelper.RedirectToLocal(this, redirectUrl);

        }

        // GET: /<controller>/
        public IActionResult Discount(string statusMessage = null, string successMessage = null, string failureMessage = null)
        {
            var viewModel = new DiscountViewModel()
            {
                Discounts = _context.Discounts.OrderBy(x => x.Value).ToList(),
                SuccessMessage = successMessage,
                StatusMessage = statusMessage,
                FailureMessage = failureMessage
            };

            return View(viewModel);
        }

        // GET: /<controller>/
        public IActionResult EditDiscount(long id, string statusMessage = null, string successMessage = null, string failureMessage = null)
        {
            var viewModel = new DiscountViewModel()
            {
                SuccessMessage = successMessage,
                StatusMessage = statusMessage,
                FailureMessage = failureMessage
            };

            if (id > 0)
            {
                var inDb = _context.Discounts.Where(x => x.Id == id).SingleOrDefault();

                viewModel.Code = inDb.Code;
                viewModel.EndDate = inDb.EndDate;
                viewModel.Id = inDb.Id;
                viewModel.IsLive = inDb.IsLive;
                viewModel.Percentage = inDb.Percentage;
                viewModel.StartDate = inDb.StartDate;
                viewModel.Value = inDb.Value;
                viewModel.MaxUsage = inDb.MaxUsage;
                viewModel.HasMaxUse = inDb.HasMaxUse;
                viewModel.HasMinValue = inDb.HasMinValue;
                viewModel.MinSpend = inDb.MinSpend;
                viewModel.UsageCount = inDb.UsageCount;
            }

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditDiscount(DiscountViewModel model)
        {

            string failureMessage = null;
            string statusMessage = null;
            string successMessage = null;
            var redirectUrl = "/ManageSite/Discount";
            var isModelValid = true;

            if ((model.Value == 0 && model.Percentage == 0) ||
                (model.StartDate != null && model.EndDate == null) ||
                (model.StartDate == null && model.EndDate != null) ||
                (string.IsNullOrWhiteSpace(model.Code))
                )
            {
                isModelValid = false;
                failureMessage = "Discount Code couldn't be updated.  Please check the fields and try again.";
            }

            List<Discount> discounts = new List<Discount>();

            discounts = _context.Discounts.Where(x => x.Code == model.Code).ToList();

            if (discounts.Count > 0 && model.Id == 0)
            {
                isModelValid = false;
                failureMessage = "Discount Code couldn't be updated.  Code already exists.";
            }

            if (model.HasMaxUse && model.MaxUsage == 0)
            {
                isModelValid = false;
                failureMessage = "Max usage of discount code cannot be 0.";
            }

            if (model.HasMinValue && model.MinSpend == 0)
            {
                isModelValid = false;
                failureMessage = "Minimun spend of discount code cannot be 0.";
            }

            if (!model.IsLive)
            {
                statusMessage = "Discount Code is not live so cannot be used.";
            }

            if (isModelValid)
            {

                var result = ManageSiteHelper.AddDiscount(model, _context);


                if (result == UpdateResult.Error)
                {
                    failureMessage = "Discount Code couldn't be updated.";
                }
                else if (result == UpdateResult.Success)
                {
                    successMessage = "Discount Code updated.";
                }
                else if (result == UpdateResult.Duplicate)
                {
                    failureMessage = "Discount Code already exists.";
                }

            }
            else
            {
                model.FailureMessage = failureMessage;
                model.StatusMessage = statusMessage;
                model.SuccessMessage = successMessage;
                return View(model);
            }

            if (!string.IsNullOrWhiteSpace(failureMessage))
            {
                redirectUrl += string.Format("?failureMessage={0}&", failureMessage);
            }
            if (!string.IsNullOrWhiteSpace(statusMessage))
            {
                redirectUrl += string.Format("?statusMessage={0}&", statusMessage);
            }
            if (!string.IsNullOrWhiteSpace(successMessage))
            {
                redirectUrl += string.Format("?successMessage={0}&", successMessage);
            }

            return ControllerHelper.RedirectToLocal(this, redirectUrl);

        }

    }
}
