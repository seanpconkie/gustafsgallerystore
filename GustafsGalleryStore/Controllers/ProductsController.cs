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

        [TempData]
        public string StatusMessage { get; set; }

        // GET: /<controller>/
        public IActionResult Index()
        {
            var viewModel = new ProductListViewModel
            {
                Products = _context.Products.
                                   Include(x => x.Department).
                                   Include(x => x.ProductBrand).
                                   Include(x => x.ProductImages).
                                   ToList(),
                StatusMessage = StatusMessage
            };

            return View(viewModel);
        }

        // GET: /<controller>/
        public IActionResult Product(long id)
        {
            var viewModel = new ProductViewModel()
            {
                Product = _context.Products.Where(x => x.Id == id).
                                  Include(c => c.ProductBrand).
                                  Include(c => c.ProductColours).
                                  Include(c => c.ProductSizes).
                                  Include(c => c.ProductImages).
                                  SingleOrDefault(),
                StatusMessage = StatusMessage,
                ReturnUrl = "~/Products"
            };

            viewModel.Sizes = ProductSize.GetList(viewModel.Product.ProductSizes);
            viewModel.Colours = ProductColour.GetList(viewModel.Product.ProductColours);

            return View(viewModel);
        }
    }
}
