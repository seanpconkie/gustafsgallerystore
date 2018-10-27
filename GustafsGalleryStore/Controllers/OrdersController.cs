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

                var basket = _context.Orders.
                                     Where(x => x.UserId == userId).
                                     Where(x => x.OrderStatusId == OrderHelper.StatusId(MasterStrings.Basket, _context)).
                                     SingleOrDefault();

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
        public IActionResult Index(string statusMessage = "", string successMessage = "", string failureMessage = "")
        {
            var userId = _userManager.GetUserId(User);
            var viewModel = new OrdersViewModel() 
            { 
                Orders = _context.Orders.
                                 Where(x => x.UserId == userId).
                                 Where(x => x.OrderStatusId != OrderHelper.StatusId(MasterStrings.Basket, _context)).
                                 Include(x => x.OrderStatus).
                                 Include(x => x.DeliveryType).
                                 OrderByDescending(x => x.OrderPlacedDate).
                                 ToList(),
                StatusMessage = statusMessage,
                SuccessMessage = successMessage,
                FailureMessage = failureMessage
            };

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
                              Where(x => x.OrderStatusId == OrderHelper.StatusId(MasterStrings.Basket, _context)).
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

        [HttpGet]
        [AllowAnonymous]
        public string GetBasketStatus(long id)
        {

            _basket = _context.Orders.
                              Where(x => x.Id == id).
                              Include(x => x.OrderStatus).
                              SingleOrDefault();

            if (_basket == null)
            {
                return "";
            }

            return _basket.OrderStatus.Status;

        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult AddToBasket([FromBody]OrderItemDTO newItemDTO)
        {

            if (ModelState.IsValid)
            {
                _basket = _context.Orders.
                                     Where(x => x.Id == newItemDTO.OrderId).
                                     Where(x => x.OrderStatusId == OrderHelper.StatusId(MasterStrings.Basket, _context)).
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
                    if (item.ProductId == newItem.ProductId &&
                        item.SizeId == newItem.SizeId &&
                        item.ColourId == newItem.ColourId
                       )
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

        [HttpPost]
        [AllowAnonymous]
        public IActionResult UpdateItem([FromBody]OrderItemDTO itemDTO)
        {

            if (ModelState.IsValid)
            {
                _basket = _context.Orders.
                                     Where(x => x.Id == itemDTO.OrderId).
                                     Where(x => x.OrderStatusId == OrderHelper.StatusId(MasterStrings.Basket, _context)).
                                     Include(x => x.OrderItems).
                                     SingleOrDefault();

                if (_basket == null)
                {
                    var paramName = "Basket";
                    var message = "Basket does not exist.";
                    throw new ArgumentNullException(paramName, message);
                }

                var updatedItem = TransformNewItemDTO(itemDTO);
                List<OrderItem> newList = new List<OrderItem>();
                decimal orderTotalPrice = 0;

                foreach (var item in _basket.OrderItems)
                {
                    if (item.Id == updatedItem.Id)
                    {
                        if (updatedItem.Quantity > 0)
                        {

                            item.Product = OrderHelper.GetProduct(item, _context);

                            item.Quantity = updatedItem.Quantity;

                            orderTotalPrice += (item.Product.Price * item.Quantity);

                            newList.Add(item);

                        }
                    }
                    else
                    {

                        item.Product = OrderHelper.GetProduct(item, _context);

                        orderTotalPrice += (item.Product.Price * item.Quantity);

                        newList.Add(item);
                    }
                }

                _basket.OrderItems.Clear();

                _basket.OrderItems = newList;

                _context.Update(_basket);

                try
                {
                    _context.SaveChanges();
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message, ex.InnerException);
                }

                return new OkObjectResult(orderTotalPrice);
            }

            return new BadRequestResult();
        }


        [AllowAnonymous]
        public IActionResult ViewBasket(long id)
        {
            var viewModel = new BasketViewModel()
            {
                Basket = OrderHelper.GetOrder(id, _context)
            };

            if(_signInManager.IsSignedIn(User))
            {
                var userId = _userManager.GetUserId(User);

                if (viewModel.Basket.UserId == null)
                {
                    // set basket user id
                    viewModel.Basket.UserId = userId;

                    OrderHelper.UpdateUser(id,userId,_context);
                }
            }

            return View(viewModel);

        }

        public IActionResult ViewOrder(long id)
        {
            var viewModel = new BasketViewModel()
            {
                Basket = OrderHelper.GetOrder(id, _context)
            };

            return View(viewModel);

        }

        public async Task<IActionResult> CancelOrder(long id)
        {
            var order = _context.Orders.Where(x => x.Id == id).SingleOrDefault();
            var user = await _userManager.GetUserAsync(User);

            OrderHelper.UpdateOrderStatus(id, OrderHelper.StatusId(MasterStrings.OrderCancelled, _context),_context);

            var customerMessage = MasterStrings.Header;
            customerMessage += "<body>";
            customerMessage += string.Format(MasterStrings.CustomerCancellation, user.Forename);
            customerMessage += MasterStrings.SpamMessage;
            customerMessage += string.Format(MasterStrings.CustomerOrderLink, id);
            customerMessage += "</body></html> ";

            var storeMessage = MasterStrings.Header;
            storeMessage += "<body>";
            storeMessage += string.Format(MasterStrings.StoreCancellation, MasterStrings.StoreName);
            storeMessage += MasterStrings.SpamMessage;
            storeMessage += string.Format(MasterStrings.StoreOrderLink, id);
            storeMessage += "</body></html> ";

            await _emailSender.SendEmailAsync(user.Email, "Order Confirmation: " + id, customerMessage);
            await _emailSender.SendEmailAsync(MasterStrings.AdminEmail, "Order Cancellation: " + id, storeMessage);

            return ControllerHelper.RedirectToLocal(this,"/Orders/Index?successMessage=Your order has been cancelled.");
        }

        [HttpGet]
        public IActionResult ReturnOrder(long id)
        {
            var model = new ReturnOrderViewModel()
            {
                Order = OrderHelper.GetOrder(id,_context),
                Return = new Return() {
                    OrderId = id
                }
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ReturnOrder(ReturnOrderViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);

            model.Return.ReturnOpenedDate = DateTime.Now;

            _context.Add(model.Return);

            foreach (var item in model.ReturnItems)
            {
                var newItem = new ReturnItem()
                {
                    ReturnId = model.Return.Id,
                    ItemId = item
                };

                _context.Add(newItem);

                model.Return.ReturnItems.Add(newItem);
            }

            _context.SaveChanges();

            OrderHelper.UpdateOrderStatus(model.Return.OrderId, OrderHelper.StatusId(MasterStrings.AwaitingReturn, _context), _context);

            var customerMessage = MasterStrings.Header;
            customerMessage += "<body>";
            customerMessage += string.Format(MasterStrings.CustomerReturn, user.Forename);
            customerMessage += MasterStrings.SpamMessage;
            customerMessage += string.Format(MasterStrings.CustomerOrderLink, model.Return.OrderId);
            customerMessage += "</body></html> ";

            var storeMessage = MasterStrings.Header;
            storeMessage += "<body>";
            storeMessage += string.Format(MasterStrings.StoreReturn, MasterStrings.StoreName);
            storeMessage += MasterStrings.SpamMessage;
            storeMessage += string.Format(MasterStrings.StoreOrderLink, model.Return.OrderId);
            storeMessage += "</body></html> ";

            await _emailSender.SendEmailAsync(user.Email, "Return Confirmation: " + model.Return.OrderId, customerMessage);
            await _emailSender.SendEmailAsync(MasterStrings.AdminEmail, "Order Returned: " + model.Return.OrderId, storeMessage);

            return ControllerHelper.RedirectToLocal(this, "/Orders/Index?successMessage=Your return has been requested.");
        }

        #region private methods

        private void NewBasket()
        {
            var userId = _userManager.GetUserId(User);

            //create new basket
            Order basket = new Order()
            {
                OrderStatus = _context.OrderStatuses.
                                      Where(x => x.Id == OrderHelper.StatusId(MasterStrings.Basket, _context)).
                                      SingleOrDefault(),
                OpenedDate = DateTime.Now
            };

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

        private OrderItem TransformNewItemDTO (OrderItemDTO input)
        {
            var output = new OrderItem();
            var size = _context.Sizes.Where(x => x.Value == input.Size).SingleOrDefault();
            var colour = _context.Colours.Where(x => x.Value == input.Colour).SingleOrDefault();

            output.Id = input.Id;

            output.ProductId = input.ProductId;

            if (size != null)
            {
                output.SizeId = size.Id;
            }

            if (colour != null)
            {
                output.ColourId = colour.Id;
            }

            output.Quantity = input.Quantity;

            return output;

        }

        #endregion

    }

}
