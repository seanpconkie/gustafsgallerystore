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

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace GustafsGalleryStore.Controllers
{
    [Authorize(Roles = MasterStrings.StaffRole)]
    public class ManageProductsController : Controller
    {
        private IHostingEnvironment _hostingEnvironment;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ManageStaffController> _logger;
        private readonly IEmailSender _emailSender;
        private readonly GustafsGalleryStoreContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;
        private const string _storeName = "GustafsGalleryStore";

        public ManageProductsController(
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

        // GET: /<controller>/
        public IActionResult Index()
        {
            var viewModel = new ProductListViewModel
            {
                Products = _context.Products.Include(x => x.ProductBrand).Include(x => x.Department).ToList(),
                StatusMessage = StatusMessage
            };

            return View(viewModel);
        }

        [HttpGet]
        public IActionResult AddProduct()
        {

            var viewModel = new NewEditProductViewModel
            {
                ReturnUrl = "~/ManageProducts/AddProduct",
                Brands = ProductBrand.GetList(_context.ProductBrands.OrderBy(b => b.Brand).ToList()),
                Colours = Colour.GetList(_context.Colours.OrderBy(c => c.Value).ToList()),
                Sizes = Size.GetList(_context.Sizes.OrderBy(s => s.Value).ToList()),
                Departments = Department.GetList(_context.Departments.OrderBy(d => d.DepartmentName).ToList())
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProduct(NewEditProductViewModel input, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            bool modelValid = true;
            decimal priceComparison = 0;

            if (string.IsNullOrWhiteSpace(input.Product.Title) ||
                input.Product.Price == priceComparison ||
                string.IsNullOrWhiteSpace(input.Department) ||
                string.IsNullOrWhiteSpace(input.Brand))
            {
                modelValid = false;
            }

            if (modelValid)
            {
                try
                {
                    //Prepare Product
                    input.Product.CreateDate = DateTime.Now;

                    //Department
                    var department = new Department();
                    department = _context.Departments.Where(x => x.DepartmentName == input.Department).SingleOrDefault();
                    input.Product.Department = department;

                    //brand
                    var brand = new ProductBrand();
                    brand = _context.ProductBrands.Where(x => x.Brand == input.Brand).SingleOrDefault();
                    input.Product.ProductBrand = brand;

                    //add sizes
                    List<ProductSize> productSizes = new List<ProductSize>();
                    foreach (var item in input.Size)
                    {
                        var size = new ProductSize()
                        {
                            Size = item.ToString()
                        };

                        productSizes.Add(size);

                    }

                    input.Product.ProductSizes = productSizes;

                    //add sizes
                    List<ProductColour> productColours = new List<ProductColour>();
                    foreach (var item in input.Colour)
                    {
                        var colour = new ProductColour()
                        {
                            Colour = item.ToString()
                        };

                        productColours.Add(colour);

                    }

                    input.Product.ProductColours = productColours;

                    //images
                    List<ProductImage> productImages = new List<ProductImage>();
                    if (input.ImageFiles != null)
                    {
                        foreach (var item in input.ImageFiles)
                        {

                            var filePath = Path.GetTempPath();

                            var filename = filePath + _storeName + '_' + $@"{item.FileName}";

                            using (var stream = new FileStream(filename, FileMode.Create))
                            {
                                await item.CopyToAsync(stream);
                            }

                            S3Helper.UploadToS3(filename);

                            var image = new ProductImage()
                            {
                                Uri = "https://s3.amazonaws.com/sbt-solutions.imagestore/" + _storeName + '_' + item.FileName.ToString() + "?Authorization"
                            };

                            productImages.Add(image);
                        }
                    }

                    input.Product.ProductImages = productImages;

                    //Create product code
                    var producCode = string.Concat(input.Product.ProductBrand.BrandCode.ToUpper(), '_', input.Product.Department.DepartmentCode, '_', input.Product.Title.Substring(0, 3).ToUpper());
                    input.Product.ProductCode = GenerateProductCode(producCode);

                    //Create Product
                    _context.Add(input.Product);

                    StatusMessage = "Product added";

                    _context.SaveChanges();

                    return ControllerHelper.RedirectToLocal(this,"/ManageProducts");
                }
                catch (System.Exception ex)
                {
                    StatusMessage = "An Error occured; " + ex.Message;
                }
            }
            input.Brands = ProductBrand.GetList(_context.ProductBrands.OrderBy(b => b.Brand).ToList());
            input.Colours = Colour.GetList(_context.Colours.OrderBy(c => c.Value).ToList());
            input.Sizes = Size.GetList(_context.Sizes.OrderBy(s => s.Value).ToList());
            input.Departments = Department.GetList(_context.Departments.OrderBy(d => d.DepartmentName).ToList());
            input.StatusMessage = StatusMessage;

            return View(input);
        }

        public IActionResult EditProduct(long id)
        {

            var viewModel = new NewEditProductViewModel()
            {
                Product = _context.Products.Where(x => x.Id == id).
                                  Include(c => c.ProductBrand).
                                  Include(c => c.ProductColours).
                                  Include(c => c.ProductSizes).
                                  Include(c => c.ProductImages).
                                  Include(c => c.Department).
                                  SingleOrDefault(),
                StatusMessage = StatusMessage,
                ReturnUrl = "~/ManageProducts"
            };

            viewModel.ProductSizes = ProductSize.GetList(viewModel.Product.ProductSizes);
            viewModel.ProductColours = ProductColour.GetList(viewModel.Product.ProductColours);
            viewModel.ProductImages = ProductImage.GetList(viewModel.Product.ProductImages);
            viewModel.Brands = ProductBrand.GetList(_context.ProductBrands.OrderBy(b => b.Brand).OrderBy(x => x.Brand).ToList());
            viewModel.Colours = Colour.GetList(_context.Colours.OrderBy(c => c.Value).ToList());
            viewModel.Departments = Department.GetList(_context.Departments.OrderBy(d => d.DepartmentName).ToList());
            viewModel.Sizes = Size.GetList(_context.Sizes.OrderBy(s => s.Value).ToList());
            viewModel.Brand = viewModel.Product.ProductBrand.Brand;
            viewModel.Department = viewModel.Product.Department.DepartmentName;

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(NewEditProductViewModel input, string returnUrl = null)
        {

            ViewData["ReturnUrl"] = returnUrl;

            bool modelValid = true;
            decimal priceComparison = 0;


            if (string.IsNullOrWhiteSpace(input.Product.Title) ||
                input.Product.Price == priceComparison ||
                string.IsNullOrWhiteSpace(input.Department) ||
                string.IsNullOrWhiteSpace(input.Brand))
            {
                modelValid = false;
            }

            if (modelValid)
            {
                try
                {
                    //extract from db
                    var inDb = new Product();
                    inDb = _context.Products.Where(x => x.Id == input.Product.Id).
                                    Include(c => c.ProductBrand).
                                    Include(c => c.ProductColours).
                                    Include(c => c.ProductSizes).
                                    Include(c => c.ProductImages).
                                    Include(c => c.Department).
                                    SingleOrDefault();

                    //update product
                    inDb.Price = input.Product.Price;

                    inDb.Description = input.Product.Description;

                    inDb.Stock = input.Product.Stock;

                    inDb.Title = input.Product.Title;

                    //Prepare Product
                    if (inDb.Department.DepartmentName != input.Department)
                    {
                        //Department
                        var department = new Department();
                        department = _context.Departments.Where(x => x.DepartmentName == input.Department).SingleOrDefault();
                        inDb.Department = department;
                    }

                    if (inDb.ProductBrand.Brand != input.Brand)
                    {
                        //brand
                        var brand = new ProductBrand();
                        brand = _context.ProductBrands.Where(x => x.Brand == input.Brand).SingleOrDefault();
                        inDb.ProductBrand = brand;
                    }

                    //add sizes
                    List<ProductSize> productSizes = new List<ProductSize>();
                    foreach (var item in input.Size)
                    {
                        var size = new ProductSize()
                        {
                            Size = item.ToString()
                        };

                        productSizes.Add(size);

                    }

                    inDb.ProductSizes = productSizes;

                    //add sizes
                    List<ProductColour> productColours = new List<ProductColour>();
                    foreach (var item in input.Colour)
                    {
                        var colour = new ProductColour()
                        {
                            Colour = item.ToString()
                        };

                        productColours.Add(colour);

                    }

                    inDb.ProductColours = productColours;

                    //images
                    List<ProductImage> productImages = new List<ProductImage>();

                    //remove old images
                    foreach (var image in inDb.ProductImages)
                    {
                        bool deleteImage = true;
                        if (input.Image != null)
                        {
                            foreach (var item in input.Image)
                            {
                                if (image.Uri.Replace("https://s3.amazonaws.com/sbt-solutions.imagestore/", "").Replace("?Authorization", "") == item)
                                {
                                    deleteImage = false;
                                }
                            }
                        }

                        if (deleteImage)
                        {
                            S3Helper.DeleteFromS3Async(image.Uri);
                            _context.Remove(image);
                        }
                        else
                        {
                            //add image to new list
                            productImages.Add(image);
                        }
                    }

                    //add new images
                    if (input.ImageFiles != null)
                    {
                        foreach (var item in input.ImageFiles)
                        {

                            var filePath = Path.GetTempPath();

                            var filename = filePath + _storeName + '_' + $@"{item.FileName}";

                            using (var stream = new FileStream(filename, FileMode.Create))
                            {
                                await item.CopyToAsync(stream);
                            }

                            S3Helper.UploadToS3(filename);

                            var image = new ProductImage()
                            {
                                Uri = "https://s3.amazonaws.com/sbt-solutions.imagestore/" + _storeName + '_' + item.FileName.ToString() + "?Authorization"
                            };

                            productImages.Add(image);
                        }
                    }

                    inDb.ProductImages = productImages;

                    //update Product
                    _context.Update(inDb);

                    StatusMessage = "Product updated";

                    _context.SaveChanges();

                    return ControllerHelper.RedirectToLocal(this,"/ManageProducts");
                }
                catch (System.Exception ex)
                {
                    StatusMessage = "An Error occured; " + ex.Message;
                }
            }

            input.Brands = ProductBrand.GetList(_context.ProductBrands.OrderBy(b => b.Brand).ToList());
            input.Colours = Colour.GetList(_context.Colours.OrderBy(c => c.Value).ToList());
            input.Sizes = Size.GetList(_context.Sizes.OrderBy(s => s.Value).ToList());
            input.Departments = Department.GetList(_context.Departments.OrderBy(d => d.DepartmentName).ToList());
            input.StatusMessage = StatusMessage;

            return View(input);

        }

        [HttpGet]
        public IActionResult Stock()
        {
            var viewModel = new ProductListViewModel()
            {
                Products = _context.Products.
                                                                            Include(p => p.ProductBrand).
                                                                            Include(p => p.Department).
                                                                            ToList()
            };
            return View(viewModel);
        }


        [HttpGet]
        public IEnumerable<Product> GetStock()
        {
            return _context.Products.
                            Include(p => p.ProductBrand).
                            Include(p => p.Department).
                            ToList();
        }


        [HttpPost]
        public IActionResult UpdateStock([FromBody]ProductListViewModel input)
        {
            try
            {

                foreach (var product in input.Products)
                {

                    var inDb = _context.Products.Where(p => p.Id == product.Id).SingleOrDefault();

                    inDb.Price = product.Price;
                    inDb.Stock = product.Stock;

                    _context.Update(inDb);

                }

                _context.SaveChanges();

                return ControllerHelper.ReturnResult(UpdateResult.Success);
            }
            catch (System.Exception ex)
            {
                return ControllerHelper.ReturnResult(UpdateResult.Error,ex.Message);
            }

        }

        [HttpPost]
        public IActionResult UpdateBrand([FromBody]BrandViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {

                    var result = ManageSiteHelper.AddBrand(model, _context);

                    if (result == UpdateResult.Error)
                    {
                        throw new Exception("Brand couldn't be updated.");
                    }
                    else if (result == UpdateResult.Duplicate)
                    {
                        throw new Exception("Brand already exists.");
                    }

                    return ControllerHelper.ReturnResult(UpdateResult.Success);
                }
                catch (System.Exception ex)
                {
                    StatusMessage = "An Error occured; " + ex.Message;
                }

            }

            model.StatusMessage = StatusMessage;

            return ControllerHelper.ReturnResult(UpdateResult.Error, StatusMessage);

        }

        [HttpPost]
        public IActionResult UpdateColour([FromBody]ColourViewModel model)
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
                    else if (result == UpdateResult.Duplicate)
                    {
                        throw new Exception("Colour already exists.");
                    }

                    return ControllerHelper.ReturnResult(UpdateResult.Success);
                }
                catch (System.Exception ex)
                {
                    StatusMessage = "An Error occured; " + ex.Message;
                }

            }

            model.StatusMessage = StatusMessage;

            return ControllerHelper.ReturnResult(UpdateResult.Error, StatusMessage);

        }

        [HttpPost]
        public IActionResult UpdateDepartment([FromBody]DepartmentViewModel model)
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
                    else if (result == UpdateResult.Duplicate)
                    {
                        throw new Exception("Department already exists.");
                    }

                    return ControllerHelper.ReturnResult(UpdateResult.Success);
                }
                catch (Exception ex)
                {
                    StatusMessage = "An Error occured; " + ex.Message;
                }

            }

            model.StatusMessage = StatusMessage;

            return ControllerHelper.ReturnResult(UpdateResult.Error, StatusMessage);

        }

        [HttpPost]
        public IActionResult UpdateSize([FromBody]SizeViewModel model)
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
                    else if (result == UpdateResult.Duplicate)
                    {
                        throw new Exception("Size already exists.");
                    }

                    return ControllerHelper.ReturnResult(UpdateResult.Success);
                }
                catch (Exception ex)
                {
                    StatusMessage = "An Error occured; " + ex.Message;
                }

            }

            model.StatusMessage = StatusMessage;

            return ControllerHelper.ReturnResult(UpdateResult.Error, StatusMessage);

        }

        #region Helpers

        private string GenerateProductCode(string productCode)
        {

            List<Product> products = new List<Product>();

            products = _context.Products.Where(x => x.ProductCode == productCode).ToList();

            if (products.Count > 0)
            {
                return string.Concat(productCode, '_', products.Count);
            }

            return productCode;
        }

        #endregion

    }
}
