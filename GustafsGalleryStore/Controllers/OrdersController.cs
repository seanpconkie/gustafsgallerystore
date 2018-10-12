using System;
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

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace GustafsGalleryStore.Controllers
{
    [Authorize]
    public class OrdersController : Controller
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
        private Order _basket;

        #endregion
        #region constructor

        public OrdersController(
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
        public long BasketId
        {
            get
            {
                var userId = _userManager.GetUserId(User);

                if (userId == null)
                {
                    return 0;
                }

                var basket = _context.Orders.Where(x => x.UserId == userId).Where(x => x.OrderStatusId == StatusId("Basket")).SingleOrDefault();

                if (basket == null)
                {
                    NewBasket();
                    basket = _basket;
                }

                return basket.Id;
            }
        }

        public string ReturnUrl { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        #endregion

        // GET: /<controller>/
        public IActionResult Index()
        {
            var userId = _userManager.GetUserId(User);
            var viewModel = new OrdersViewModel() { Orders = _context.Orders.Where(x => x.UserId == userId).Where(x => x.OrderStatusId != StatusId("Basket")).ToList() };
            return View(viewModel);
        }

        [HttpGet]
        [AllowAnonymous]
        public long GetBasketId()
        {
            long id = BasketId; //checks for user basket

            if (id ==0)
            {
                NewBasket();
                id = _basket.Id;
            }

            return id;

        }

        [HttpGet]
        [AllowAnonymous]
        public long GetBasketCount(long id)
        {

            _basket = _context.Orders.
                              Where(x => x.Id == id).
                              Where(x => x.OrderStatusId == StatusId("Basket")).
                              Include(x => x.OrderItems).
                              SingleOrDefault();

            if (_basket == null)
            {
                return 0;
            }

            long count = 0;
            foreach (var item in _basket.OrderItems)
            {
                count += item.Quantity;
            }

            return count;

        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult AddToBasket([FromBody]OrderItemDTO newItemDTO)
        {

            if (ModelState.IsValid)
            {
                _basket = _context.Orders.
                                     Where(x => x.Id == newItemDTO.OrderId).
                                     Where(x => x.OrderStatusId == StatusId("Basket")).
                                     Include(x => x.OrderItems).
                                     SingleOrDefault();

                if (_basket == null)
                {
                    NewBasket();
                }

                var newItem = TransformNewItemDTO(newItemDTO);

                bool isProductInBasket = false;

                foreach (var item in _basket.OrderItems)
                {
                    if (item.ProductId == newItem.ProductId)
                    {
                        isProductInBasket = true;
                        item.Quantity++;
                    }
                }

                if (isProductInBasket == false)
                {
                    _basket.OrderItems.Add(newItem);
                }

                _context.Update(_basket);

                try
                {
                    _context.SaveChanges();
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message, ex.InnerException);
                }

                return new OkObjectResult(_basket.Id);
            }

            return new BadRequestResult();
        }

        public IActionResult ViewBasket(long id)
        {
            var viewModel = new BasketViewModel()
            {
                Basket = _context.Orders.
                                    Where(x => x.Id == id).
                                    Where(x => x.OrderStatusId == StatusId("Basket")).
                                    Include(x => x.OrderItems).
                                    Include(x => x.CustomerContact).
                                    Include(x => x.DeliveryType).SingleOrDefault()
            };

            var userId = _userManager.GetUserId(User);

            if (viewModel.Basket.UserId == null)
            {
                // set basket user id
                viewModel.Basket.UserId = userId;

                _context.Update(viewModel.Basket);
                _context.SaveChanges();
            }
            else if (viewModel.Basket.UserId != userId)
            {
                // clear basket
                NewBasket();
                viewModel.Basket = _basket;
            }

            // expand orderitems
            List<OrderItem> orderItems = new List<OrderItem>();

            foreach (var item in viewModel.Basket.OrderItems)
            {

                var productId = item.ProductId;
                var product = _context.Products.Where(x => x.Id == productId).
                                  Include(c => c.ProductBrand).
                                  Include(c => c.ProductColours).
                                  Include(c => c.ProductSizes).
                                  Include(c => c.ProductImages).
                                  SingleOrDefault();

                item.Product = product;

                orderItems.Add(item);

            }

            viewModel.Basket.OrderItems = orderItems;

            return View(viewModel);

        }

        #region private methods

        private void NewBasket()
        {
            //create new basket
            Order basket = new Order() { 
                OrderStatus = _context.OrderStatuses.Where(x => x.Id == StatusId("Basket")).SingleOrDefault(),
                OpenedDate = DateTime.Now
            };
            var userId = _userManager.GetUserId(User);

            if (userId != null)
            {
                basket.UserId = userId;
            }

            _context.Add(basket);

            try
            {
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }

            _basket = basket;

        }

        private long StatusId(string status)
        {

            var statusInDb = _context.OrderStatuses.Where(x => x.Status == status).SingleOrDefault();
            if(statusInDb != null)
            {
                return statusInDb.Id;
            }

            return 0;
        }

        private OrderItem TransformNewItemDTO (OrderItemDTO input)
        {
            var output = new OrderItem();
            var size = _context.Sizes.Where(x => x.Value == input.Size).SingleOrDefault();
            var colour = _context.Colours.Where(x => x.Value == input.Colour).SingleOrDefault();

            output.ProductId = input.ProductId;
            output.SizeId = size.Id;
            output.ColourId = colour.Id;
            output.Quantity = 1;

            return output;

        }
        #endregion
    
    }

}
