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

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace GustafsGalleryStore.Controllers
{
    [AllowAnonymous]
    public class ProductsController : Controller
    {
        private IHostingEnvironment _hostingEnvironment;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ManageStaffController> _logger;
        private readonly IEmailSender _emailSender;
        private readonly GustafsGalleryStoreContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;

        public ProductsController(
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

        // GET: /<controller>/
        public IActionResult Index(List<string> filterBrand = null, List<string> filterDepartment = null, 
                                   string statusMessage = null, string successMessage = null, 
                                   string failureMessage = null)
        {
            var products = new List<Product>();
            var where = "";
            var searchTerm = "select * from products where ";

            if (filterBrand != null && filterBrand.Count > 0)
            {
                foreach (var brand in filterBrand)
                {
                    if (brand.ToLower().IndexOf("delete", System.StringComparison.CurrentCulture) > -1 ||
                        brand.ToLower().IndexOf("update", System.StringComparison.CurrentCulture) > -1 ||
                        brand.ToLower().IndexOf("insert", System.StringComparison.CurrentCulture) > -1 ||
                        brand.ToLower().IndexOf("create", System.StringComparison.CurrentCulture) > -1 ||
                        brand.ToLower().IndexOf("drop", System.StringComparison.CurrentCulture) > -1
                       )
                    {
                        return ControllerHelper.RedirectToLocal(this, "/Products?failureMessage=Filter not available.");
                    }

                    if (where.Length == 0)
                    {
                        where = " productbrandid in ( ";
                    }
                    else
                    {
                        where += ",";
                    }

                    var inDb = _context.ProductBrands.Where(x => x.Brand == brand).SingleOrDefault();
                    if (inDb != null)
                    {
                        where += inDb.Id;
                    }
                }

                searchTerm += where + ")";
            }

            if (where.Length > 0 && filterDepartment != null && filterDepartment.Count > 0)
            {
                searchTerm += " and ";
            }

            if (filterDepartment != null && filterDepartment.Count > 0)
            {
                foreach (var dept in filterDepartment)
                {
                    if (dept.ToLower().IndexOf("delete", System.StringComparison.CurrentCulture) > -1 ||
                        dept.ToLower().IndexOf("update", System.StringComparison.CurrentCulture) > -1 ||
                        dept.ToLower().IndexOf("insert", System.StringComparison.CurrentCulture) > -1 ||
                        dept.ToLower().IndexOf("create", System.StringComparison.CurrentCulture) > -1 ||
                        dept.ToLower().IndexOf("drop", System.StringComparison.CurrentCulture) > -1
                       )
                    {
                        return ControllerHelper.RedirectToLocal(this, "/Products?failureMessage=Filter not available.");
                    }

                    if (where.Length == 0)
                    {
                        where = " departmentid in ( ";
                    }
                    else
                    {
                        where += ",";
                    }

                    var inDb = _context.Departments.Where(x => x.DepartmentName == dept).SingleOrDefault();
                    if (inDb != null)
                    {
                        where += inDb.Id;
                    }
                }

                searchTerm += where + ")";
            }

            if(where.Length > 0)
            {
                products = _context.Products.
                                   FromSql(searchTerm).
                                   Include(p => p.Department).
                                   Include(p => p.ProductBrand).
                                   Include(p => p.ProductSizes).
                                   Include(p => p.ProductImages).
                                   Include(p => p.ProductColours).
                                   Where(x => x.Stock > 0).
                                   ToList();
            }
            else
            {
                products = _context.Products.
                                   Include(p => p.Department).
                                   Include(p => p.ProductBrand).
                                   Include(p => p.ProductSizes).
                                   Include(p => p.ProductImages).
                                   Include(p => p.ProductColours).
                                   Where(x => x.Stock > 0).
                                   ToList();
            }

            var viewModel = new ProductListViewModel
            {
                Products = products,
                Brands = _context.ProductBrands.OrderBy(x => x.Brand).ToList(),
                Departments = _context.Departments.OrderBy(x => x.DepartmentName).ToList(),
                SuccessMessage = successMessage,
                FailureMessage = failureMessage,
                StatusMessage = statusMessage
            };

            if(filterBrand != null)
            {
                viewModel.FilteredBrands = filterBrand;
            }
            else
            {
                viewModel.FilteredBrands = new List<string>();
            }

            if (filterDepartment != null)
            {
                viewModel.FilteredDepartments = filterDepartment;
            }
            else
            {
                viewModel.FilteredDepartments = new List<string>();
            }

            return View(viewModel);
            //return ControllerHelper.RedirectToLocal(this,"/Home/ComingSoon");
        }

        // GET: /<controller>/
        public IActionResult Product(long id, string statusMessage = null, string successMessage = null,
                                   string failureMessage = null)
        {
            var viewModel = new ProductViewModel()
            {
                Product = _context.Products.Where(x => x.Id == id).
                                  Include(c => c.ProductBrand).
                                  Include(c => c.ProductColours).
                                  Include(c => c.ProductSizes).
                                  Include(c => c.ProductImages).
                                  SingleOrDefault(),
                StatusMessage = statusMessage,
                SuccessMessage = successMessage,
                FailureMessage = failureMessage,
                ReturnUrl = "~/Products"
            };

            viewModel.Sizes = ProductSize.GetList(viewModel.Product.ProductSizes);
            viewModel.Colours = ProductColour.GetList(viewModel.Product.ProductColours);

            viewModel.Product.Description = viewModel.Product.Description.Replace(System.Environment.NewLine, "<br />");

            if (viewModel.Product.ProductImages.Count == 0)
            {
                viewModel.Product.ProductImages.Add(new ProductImage() { Uri = "https://farm5.staticflickr.com/4705/40336899591_bdc86eddb2_o.png" });
            }

            // get related products
            viewModel.RelatedProducts = _context.Products.
                                      Where(x => x.ProductBrandId == viewModel.Product.ProductBrandId).
                                      Include(x => x.ProductImages).
                                      ToList();

            return View(viewModel);
            //return ControllerHelper.RedirectToLocal(this, "/Home/ComingSoon");
        }

        // GET: /<controller>/
        public IActionResult ProductByName(string product, string statusMessage = null, string successMessage = null,
                                   string failureMessage = null)
        {

            string[] inputList = product.Split("_by_");

            string name = inputList[0].Replace('_',' ');
            string brand = inputList[1].Replace('_',' ');

            var brandId = _context.ProductBrands.Where(x => x.Brand == brand).SingleOrDefault().Id;

            var productId = _context.Products.
                                Where(x => x.ProductBrandId == brandId).
                                Where(x => x.Title == name).
                                SingleOrDefault().
                                Id;

            var redirectUri = string.Format("/Products/Product?id={0}", productId);
            if (string.IsNullOrWhiteSpace(statusMessage))
            {
                redirectUri += string.Format("&&statusMessage={0}", statusMessage);
            }

            if (string.IsNullOrWhiteSpace(successMessage))
            {
                redirectUri += string.Format("&&successMessage={0}", successMessage);
            }

            if (string.IsNullOrWhiteSpace(failureMessage))
            {
                redirectUri += string.Format("&&failureMessage={0}", failureMessage);
            }

            return ControllerHelper.RedirectToLocal(this,redirectUri);
            //return ControllerHelper.RedirectToLocal(this, "/Home/ComingSoon");
        }
    }
}
