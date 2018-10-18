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
    [Authorize(Roles = "IsStaff")]
    public class ManageOrdersController : Controller
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

        public ManageOrdersController(
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


        #region properties
        public string ReturnUrl { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        #endregion

        // GET: /<controller>/
        public IActionResult Index()
        {
            var userId = _userManager.GetUserId(User);
            var viewModel = new OrdersViewModel()
            {
                Orders = _context.Orders.
                             Where(x => x.OrderStatusId != OrderHelper.StatusId("Basket", _context)).
                             ToList()
            };

            //clear old orders
            DateTime date = DateTime.Now.AddDays(-1);

            List<Order> orders = _context.Orders.
                                    Where(x => x.OpenedDate < date).
                                    Where(x => x.OrderStatusId != OrderHelper.StatusId("Basket", _context)).
                                    ToList();

            foreach (var item in orders)
            {
                _context.RemoveRange(_context.OrderItems.Where(x => x.OrderId == item.Id));

                _context.Remove(item);

            }

            _context.SaveChanges();

            return View(viewModel);
        }
    }
}
