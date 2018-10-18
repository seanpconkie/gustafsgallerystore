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
    [Authorize(Roles = "IsStaff")]
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

        [TempData]
        public string StatusMessage { get; set; }

        // GET: /<controller>/
        public IActionResult Index()
        {
            return View();
        }

        // GET: /<controller>/
        public IActionResult Brand()
        {
            var viewModel = new BrandViewModel() { Brands = _context.ProductBrands.OrderBy(x => x.Brand).ToList() };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Brand(BrandViewModel model)
        {

            if (ModelState.IsValid)
            {
                try
                {

                    var result = ManageSiteHelper.AddBrand(model,_context);

                    if (result == UpdateResult.Error)
                    {
                        throw new Exception("Brand couldn't be updated.");
                    }

                    return ControllerHelper.RedirectToLocal(this,"/ManageSite/Brand");
                }
                catch (System.Exception ex)
                {
                    StatusMessage = "An Error occured; " + ex.Message;
                }

            }

            model.StatusMessage = StatusMessage;

            return View(model);

        }

        // GET: /<controller>/
        public IActionResult Colour()
        {
            var viewModel = new ColourViewModel() { Colours = _context.Colours.OrderBy(x => x.Value).ToList() };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Colour(ColourViewModel model)
        {

            if (ModelState.IsValid)
            {
                try
                {

                    var result = ManageSiteHelper.AddColour(model, _context);

                    if (result == UpdateResult.Error)
                    {
                        throw new Exception("Colour couldn't be updated.");
                    }

                    return ControllerHelper.RedirectToLocal(this,"/ManageSite/Colour");
                }
                catch (System.Exception ex)
                {
                    StatusMessage = "An Error occured; " + ex.Message;
                }

            }

            model.StatusMessage = StatusMessage;

            return View(model);

        }

        // GET: /<controller>/
        public IActionResult Delivery()
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
        public IActionResult DeliveryType()
        {
            var viewModel = new DeliveryViewModel() { DeliveryCompanies = _context.DeliveryCompanies.ToList()};

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeliveryType(DeliveryViewModel model)
        {

            if (ModelState.IsValid)
            {
                try
                {

                    var result = ManageSiteHelper.AddDeliveryType(model, _context);

                    if (result == UpdateResult.Error)
                    {
                        throw new Exception("Delivery method couldn't be updated.");
                    }

                    return ControllerHelper.RedirectToLocal(this, "/ManageSite/Delivery");
                }
                catch (Exception ex)
                {
                    StatusMessage = "An Error occured; " + ex.Message;
                }

            }

            model.StatusMessage = StatusMessage;

            return View(model);

        }

        // GET: /<controller>/
        public IActionResult DeliveryCompany()
        {
            var viewModel = new DeliveryViewModel() { DeliveryCompanies = _context.DeliveryCompanies.ToList() };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeliveryCompany(DeliveryViewModel model)
        {

            if (ModelState.IsValid)
            {
                try
                {

                    var result = ManageSiteHelper.AddDeliveryCompany(model, _context);

                    if (result == UpdateResult.Error)
                    {
                        throw new Exception("Company couldn't be updated.");
                    }

                    return ControllerHelper.RedirectToLocal(this, "/ManageSite/DeliveryCompany");
                }
                catch (Exception ex)
                {
                    StatusMessage = "An Error occured; " + ex.Message;
                }

            }

            model.StatusMessage = StatusMessage;

            return View(model);

        }

        // GET: /<controller>/
        public IActionResult Department()
        {
            var viewModel = new DepartmentViewModel() { Departments = _context.Departments.OrderBy(x => x.DepartmentName).ToList() };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Department(DepartmentViewModel model)
        {

            if (ModelState.IsValid)
            {
                try
                {

                    var result = ManageSiteHelper.AddDepartment(model, _context);

                    if (result == UpdateResult.Error)
                    {
                        throw new Exception("Department couldn't be updated.");
                    }

                    return ControllerHelper.RedirectToLocal(this,"/ManageSite/Department");
                }
                catch (System.Exception ex)
                {
                    StatusMessage = "An Error occured; " + ex.Message;
                }

            }

            model.StatusMessage = StatusMessage;

            return View(model);

        }

        // GET: /<controller>/
        public IActionResult Size()
        {
            var viewModel = new SizeViewModel() { Sizes = _context.Sizes.OrderBy(x => x.Value).ToList() };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Size(SizeViewModel model)
        {

            if (ModelState.IsValid)
            {
                try
                {

                    var result = ManageSiteHelper.AddSize(model, _context);

                    if (result == UpdateResult.Error)
                    {
                        throw new Exception("Size couldn't be updated.");
                    }

                    return ControllerHelper.RedirectToLocal(this,"/ManageSite/Size");
                }
                catch (Exception ex)
                {
                    StatusMessage = "An Error occured; " + ex.Message;
                }

            }

            model.StatusMessage = StatusMessage;

            return View(model);

        }

    }
}
