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
                                    int pageItems = 20, int pageNumber = 1,
                                    string search = null, string orderBy = "createdate", string orderByModifier = "desc",
                                    string statusMessage = null, string successMessage = null, 
                                    string failureMessage = null)
        {
            var products = new List<Product>();
            var pageProducts = new List<Product>();
            var where = "";
            string searchTerm = null;
            var maxPages = 1;
            var pageStart = 1;
            var pageEnd = 1;

            //checked filter for brand and ensure no SQL injection
            if (filterBrand != null && filterBrand.Count > 0)
            {
                if (searchTerm == null)
                {
                    searchTerm = "select * from products where ";
                }

                foreach (var brand in filterBrand)
                {
                    if (!ProductHelper.CheckSQL(brand))
                    {
                        return ControllerHelper.RedirectToLocal(this, "/Products?failureMessage=Filter not available.");
                    }
                }
                where = ProductHelper.CreateFilterSQL("productbrandid", filterBrand, _context);
                searchTerm += where;
            }

            //if both brand and department filtered then add 'and' to where
            if (where.Length > 0 && filterDepartment != null && filterDepartment.Count > 0)
            {
                searchTerm += " and ";
            }

            //checked department filter and ensure no SQL injection
            if (filterDepartment != null && filterDepartment.Count > 0)
            {
                if (searchTerm == null)
                {
                    searchTerm = "select * from products where ";
                }

                foreach (var dept in filterDepartment)
                {
                    if (!ProductHelper.CheckSQL(dept))
                    {
                        return ControllerHelper.RedirectToLocal(this, "/Products?failureMessage=Filter not available.");
                    }
                }
                where = ProductHelper.CreateFilterSQL("departmentid", filterDepartment, _context);
                searchTerm += where;
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                if (where.Length > 0 && (filterDepartment != null || filterDepartment.Count > 0))
                {
                    searchTerm += " and ";
                }
                else
                {
                    searchTerm = "select * from products where ";
                }

                searchTerm += ProductHelper.CreateSearchSQL(search);

            }

            products = ProductHelper.GetProducts(_context, searchTerm, orderBy, orderByModifier);

            //select just the products relevant to selected page
            if (pageItems == 1)
            {
                maxPages = 1;
            }
            else
            {
                double pages = Convert.ToDouble(products.Count) / Convert.ToDouble(pageItems);
                maxPages = Convert.ToInt32(Math.Ceiling(pages));
            }

            if (pageNumber > 1)
            {
                pageStart = ((pageNumber - 1) * pageItems) + 1;
            }

            if (pageItems == 1 || products.Count < pageItems)
            {
                pageEnd = products.Count;
            }
            else
            {
                pageEnd = pageNumber * pageItems;
            }

            if (pageItems == 1)
            {
                pageProducts = products;
            }
            else
            {
                var i = 0;
                foreach (var product in products)
                {
                    i++;
                    if (i >= pageStart && i <= pageEnd)
                    {
                        pageProducts.Add(product);
                    }
                }
            }

            var previousUrl = string.Format("/Products?pageItems={0}&&pageNumber={1}&&orderBy={2}&&orderByModifier={3}&&search={4}",
                                        pageItems,pageNumber - 1,orderBy,orderByModifier,search);
            var nextUrl = string.Format("/Products?pageItems={0}&&pageNumber={1}&&orderBy={2}&&orderByModifier={3}&&search={4}",
                                        pageItems, pageNumber + 1, orderBy, orderByModifier,search);
            var page1Url = "";
            var page2Url = "";
            var page3Url = "";
            var page4Url = "";
            var page5Url = "";

            if (pageNumber == 1)
            {
                page1Url = string.Format("/Products?pageItems={0}&&pageNumber={1}&&orderBy={2}&&orderByModifier={3}&&search={4}",
                                            pageItems, pageNumber, orderBy, orderByModifier,search);
                page2Url = string.Format("/Products?pageItems={0}&&pageNumber={1}&&orderBy={2}&&orderByModifier={3}&&search={4}",
                                            pageItems, pageNumber + 1, orderBy, orderByModifier,search);
                page3Url = string.Format("/Products?pageItems={0}&&pageNumber={1}&&orderBy={2}&&orderByModifier={3}&&search={4}",
                                            pageItems, pageNumber + 2, orderBy, orderByModifier,search);
                page4Url = string.Format("/Products?pageItems={0}&&pageNumber={1}&&orderBy={2}&&orderByModifier={3}&&search={4}",
                                            pageItems, pageNumber + 3, orderBy, orderByModifier,search);
                page5Url = string.Format("/Products?pageItems={0}&&pageNumber={1}&&orderBy={2}&&orderByModifier={3}&&search={4}",
                                            pageItems, pageNumber + 4, orderBy, orderByModifier,search);
            }
            else if (pageNumber == 2)
            {
                page1Url = string.Format("/Products?pageItems={0}&&pageNumber={1}&&orderBy={2}&&orderByModifier={3}&&search={4}",
                                            pageItems, pageNumber - 1, orderBy, orderByModifier,search);
                page2Url = string.Format("/Products?pageItems={0}&&pageNumber={1}&&orderBy={2}&&orderByModifier={3}&&search={4}",
                                            pageItems, pageNumber, orderBy, orderByModifier,search);
                page3Url = string.Format("/Products?pageItems={0}&&pageNumber={1}&&orderBy={2}&&orderByModifier={3}&&search={4}",
                                            pageItems, pageNumber + 1, orderBy, orderByModifier,search);
                page4Url = string.Format("/Products?pageItems={0}&&pageNumber={1}&&orderBy={2}&&orderByModifier={3}&&search={4}",
                                            pageItems, pageNumber + 2, orderBy, orderByModifier,search);
                page5Url = string.Format("/Products?pageItems={0}&&pageNumber={1}&&orderBy={2}&&orderByModifier={3}&&search={4}",
                                            pageItems, pageNumber + 3, orderBy, orderByModifier,search);
            }
            else 
            {
                page1Url = string.Format("/Products?pageItems={0}&&pageNumber={1}&&orderBy={2}&&orderByModifier={3}&&search={4}",
                                            pageItems, pageNumber - 2, orderBy, orderByModifier,search);
                page2Url = string.Format("/Products?pageItems={0}&&pageNumber={1}&&orderBy={2}&&orderByModifier={3}&&search={4}",
                                            pageItems, pageNumber - 1, orderBy, orderByModifier,search);
                page3Url = string.Format("/Products?pageItems={0}&&pageNumber={1}&&orderBy={2}&&orderByModifier={3}&&search={4}",
                                            pageItems, pageNumber, orderBy, orderByModifier,search);
                page4Url = string.Format("/Products?pageItems={0}&&pageNumber={1}&&orderBy={2}&&orderByModifier={3}&&search={4}",
                                            pageItems, pageNumber + 1, orderBy, orderByModifier,search);
                page5Url = string.Format("/Products?pageItems={0}&&pageNumber={1}&&orderBy={2}&&orderByModifier={3}&&search={4}",
                                            pageItems, pageNumber + 2, orderBy, orderByModifier,search);
            }

            var viewModel = new ProductListViewModel
            {
                Products = pageProducts,
                Brands = _context.ProductBrands.OrderBy(x => x.Brand).ToList(),
                Departments = _context.Departments.OrderBy(x => x.DepartmentName).ToList(),
                PageItems = pageItems,
                PageNumber = pageNumber,
                MaxPages = maxPages,
                OrderBy = orderBy,
                PreviousUrl = previousUrl,
                NextUrl = nextUrl,
                PageURLs = new List<string> { page1Url, page2Url, page3Url, page4Url, page5Url },
                ItemCountUrl1 = string.Format("/Products?pageItems={0}&&pageNumber={1}&&orderBy={2}&&orderByModifier={3}&&search={4}",
                                        20, 1, orderBy, orderByModifier,search),
                ItemCountUrl2 = string.Format("/Products?pageItems={0}&&pageNumber={1}&&orderBy={2}&&orderByModifier={3}&&search={4}",
                                        40, 1, orderBy, orderByModifier,search),
                ItemCountUrl3 = string.Format("/Products?pageItems={0}&&pageNumber={1}&&orderBy={2}&&orderByModifier={3}&&search={4}",
                                        1, 1, orderBy, orderByModifier,search),
                CreateDateUrl = string.Format("/Products?pageItems={0}&&pageNumber={1}&&orderBy={2}&&orderByModifier={3}&&search={4}",
                                            pageItems, 1, "createDate", "desc", search),
                BrandUrl = string.Format("/Products?pageItems={0}&&pageNumber={1}&&orderBy={2}&&orderByModifier={3}&&search={4}",
                                            pageItems, 1, "brand", "asc", search),
                DepartmentUrl = string.Format("/Products?pageItems={0}&&pageNumber={1}&&orderBy={2}&&orderByModifier={3}&&search={4}",
                                            pageItems, 1, "department", "asc", search),
                PriceUrl = string.Format("/Products?pageItems={0}&&pageNumber={1}&&orderBy={2}&&orderByModifier={3}&&search={4}",
                                            pageItems, 1, "price", "asc", search),
                TitleUrl = string.Format("/Products?pageItems={0}&&pageNumber={1}&&orderBy={2}&&orderByModifier={3}&&search={4}",
                                            pageItems, 1, "title", "asc", search),
                ReturnUrl = string.Format("/Products?pageItems={0}&&pageNumber={1}&&orderBy={2}&&orderByModifier={3}&&search={4}",
                                            pageItems, pageNumber, orderBy, orderByModifier, search),
                OrderByModifier = orderByModifier,
                SuccessMessage = successMessage,
                FailureMessage = failureMessage,
                StatusMessage = statusMessage
            };

            if (filterBrand != null)
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
                                   string failureMessage = null, string returnUrl = null)
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
                FailureMessage = failureMessage
            };

            if (string.IsNullOrWhiteSpace(returnUrl))
            {
                viewModel.ReturnUrl = "~/Products";
            }
            else
            {
                viewModel.ReturnUrl = returnUrl.Replace(";amp;","&");
            }

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
                                      Include(p => p.Department).
                                      Include(p => p.ProductBrand).
                                      Include(p => p.ProductSizes).
                                      Include(p => p.ProductImages).
                                      Include(p => p.ProductColours).
                                      Where(x => x.Stock > 0).
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
