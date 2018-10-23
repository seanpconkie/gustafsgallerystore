using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
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
        public IActionResult Index(string statusMessage = null)
        {
            var userId = _userManager.GetUserId(User);
            var viewModel = new ManageOrdersViewModel()
            {
                NewOrders = _context.Orders.
                             Where(x => x.OrderStatusId == OrderHelper.StatusId("Order Placed", _context)).
                             Include(x => x.OrderStatus).
                             OrderByDescending(x => x.OrderPlacedDate).
                             ToList(),
                AwaitingStock = _context.Orders.
                             Where(x => x.OrderStatusId == OrderHelper.StatusId("Awaiting Stock", _context)).
                             Include(x => x.OrderStatus).
                             OrderByDescending(x => x.OrderPlacedDate).
                             ToList(),
                CancelledOrders = _context.Orders.
                             Where(x => x.OrderStatusId == OrderHelper.StatusId("Order Cancelled", _context)).
                             Include(x => x.OrderStatus).
                             ToList(),
                OpenReturns = _context.Orders.
                             Where(x => x.OrderStatusId == OrderHelper.StatusId("Awaiting Return", _context)).
                             Include(x => x.OrderStatus).
                             ToList(),
                StatusMessage = statusMessage
            };

            var searchTerm = "select * from dbo.orders where orderstatusid in (" + 
                               OrderHelper.StatusId("Order Dispatched", _context) + "," + 
                               OrderHelper.StatusId("Order Returned", _context) + "," + 
                               OrderHelper.StatusId("Cancellation Completed",_context) + ")";

            var completed = _context.Orders.FromSql(searchTerm).ToList();

            viewModel.CompletedOrders = completed;

            //clear old orders
            DateTime date = DateTime.Now.AddDays(-30);

            List<Order> orders = _context.Orders.
                                    Where(x => x.OpenedDate < date).
                                    Where(x => x.OrderStatusId == OrderHelper.StatusId("Basket", _context)).
                                    ToList();

            foreach (var item in orders)
            {
                _context.RemoveRange(_context.OrderItems.Where(x => x.OrderId == item.Id));

                _context.Remove(item);

            }

            _context.SaveChanges();

            return View(viewModel);
        }

        public async Task<IActionResult> ViewOrder(long id, string statusMessage = "", string successMessage = "", string failureMessage = "")
        {
            var viewModel = new ProcessOrderViewModel()
            {
                Order = OrderHelper.GetOrder(id, _context),
                StatusMessage = statusMessage,
                FailureMessage = failureMessage,
                SuccessMessage = successMessage,
                Returns = _context.Returns.
                                  Where(x => x.OrderId == id).
                                  Include(x => x.ReturnItems).
                                  ToList(),
                OrderHistories = _context.OrderHistories.
                                         Where(x => x.OrderId == id).
                                         Include(x => x.OrderStatus).
                                         ToList()
            };

            foreach (var returnItem in viewModel.Returns)
            {
                var productList = new List<Product>();

                foreach(var itemId in returnItem.ReturnItems)
                {
                    var product = _context.Products.
                                          Where(x => x.Id == itemId.ItemId).
                                          Include(x => x.ProductImages).
                                          SingleOrDefault();
                    productList.Add(product);
                }

                returnItem.Products = productList;
            }

            viewModel.User = await _userManager.FindByIdAsync(viewModel.Order.UserId);

            return View(viewModel);
        }

        public async Task<IActionResult> AwaitingStock(long id)
        {

            string statusMessage = null;
            string failureMessage = null;
            string successMessage = null;

            await UpdateOrder(id, "Awaiting Stock");

            successMessage = "Order updated.";

            return ControllerHelper.RedirectToLocal(this, string.Format("/ManageOrders/ViewOrder?id={0}&&statusMessage={1}&&failureMessage={2}&&successMessage={3}", id, statusMessage, failureMessage, successMessage));
        }


        public async Task<IActionResult> CancelOrder(long id)
        {
            var order = _context.Orders.
                                Where(x => x.Id == id).
                                Include(x => x.DeliveryType).
                                SingleOrDefault();
            UpdateResult result = UpdateResult.Error;

            string statusMessage = null;
            string failureMessage = null;
            string successMessage = null;

            int amount = Convert.ToInt32((order.OrderTotalPrice) * 100);

            if (!string.IsNullOrWhiteSpace(order.StripeSource))
            {
                var stripeResult = StripeHelper.RefundCharge(order.PaymentId, amount);

                order.RefundStatus = stripeResult.Status;
                order.RefundCreatedDate = stripeResult.Created;

                if (stripeResult.Status == "succeeded" || stripeResult.Status == "pending")
                {
                    order.RefundId = stripeResult.BalanceTransactionId;
                    result = UpdateResult.Success;
                }

                if (stripeResult.Status == "failed" || stripeResult.Status == "canceled")
                {
                    order.RefundId = stripeResult.FailureBalanceTransactionId;
                    order.RefundMessage = stripeResult.FailureReason;

                    failureMessage = "Refund failed: " + stripeResult.FailureReason;

                    result = UpdateResult.Error;

                }
            }
            else
            {
                // paypal
                failureMessage = "Refund couldn't be completed.  Please complete this via payment handler.";
            }

            if (result == UpdateResult.Success)
            {
                order.CancellationCompletedDate = DateTime.Now;
                await UpdateOrder(id, "Cancellation Completed");
                successMessage = "Order updated.";
            }

            return ControllerHelper.RedirectToLocal(this, string.Format("/ManageOrders/ViewOrder?id={0}&&statusMessage={1}&&failureMessage={2}&&successMessage={3}", id, statusMessage, failureMessage, successMessage));
        }

        public async Task<IActionResult> OrderDispatched(long id, string packageReference)
        {
            var order = _context.Orders.Where(x => x.Id == id).SingleOrDefault();

            string statusMessage = null;
            string failureMessage = null;
            string successMessage = null;

            if (string.IsNullOrWhiteSpace(packageReference))
            {
                failureMessage = "Please enter Package Reference.";

                return ControllerHelper.RedirectToLocal(this, string.Format("/ManageOrders/ViewOrder?id={0}&&statusMessage={1}&&failureMessage={2}&&successMessage={3}", id, statusMessage, failureMessage, successMessage));
            }

            order.PackageReference = packageReference;

            _context.Update(order);
            _context.SaveChanges();

            await UpdateOrder(id, "Order Dispatched");

            successMessage = "Order updated.";

            return ControllerHelper.RedirectToLocal(this, string.Format("/ManageOrders/ViewOrder?id={0}&&statusMessage={1}&&failureMessage={2}&&successMessage={3}", id, statusMessage, failureMessage, successMessage));
        }

        public async Task<IActionResult> CompleteReturn(long id, decimal returnAmount)
        {
            var returnItem = _context.Returns.
                                     Where(x => x.OrderId == id).
                                     Where(x => x.ReturnCompleteDate == null).
                                     SingleOrDefault();
            var order = _context.Orders.Where(x => x.Id == returnItem.OrderId).SingleOrDefault();
            UpdateResult result = UpdateResult.Error;

            string statusMessage = null;
            string failureMessage = null;
            string successMessage = null;

            int amount = Convert.ToInt32(returnAmount * 100);

            if (!string.IsNullOrWhiteSpace(order.StripeSource) && amount > 0)
            {
                var stripeResult = StripeHelper.RefundCharge(order.PaymentId, amount);

                returnItem.RefundStatus = stripeResult.Status;
                returnItem.RefundCreatedDate = stripeResult.Created;

                if (stripeResult.Status == "succeeded" || stripeResult.Status == "pending")
                {
                    returnItem.RefundId = stripeResult.BalanceTransactionId;
                    result = UpdateResult.Success;
                }

                if (stripeResult.Status == "failed" || stripeResult.Status == "canceled")
                {
                    returnItem.RefundId = stripeResult.FailureBalanceTransactionId;
                    returnItem.RefundMessage = stripeResult.FailureReason;

                    failureMessage = "Refund failed: " + stripeResult.FailureReason;

                    result = UpdateResult.Error;

                }
            }
            else if(amount == 0)
            {
                result = UpdateResult.Success;
            }
            else
            {
                // paypal
                failureMessage = "Refund couldn't be completed.  Please complete this via payment handler.";
            }

            if (result == UpdateResult.Success)
            {
                returnItem.ReturnCompleteDate = DateTime.Now;
                await UpdateOrder(order.Id, "Order Returned");
                successMessage = "Order updated.";
            }

            _context.Update(returnItem);
            _context.SaveChanges();


            return ControllerHelper.RedirectToLocal(this, string.Format("/ManageOrders/ViewOrder?id={0}&&statusMessage={1}&&failureMessage={2}&&successMessage={3}", id, statusMessage, failureMessage, successMessage));
        }

        #region private methods
        private async Task<UpdateResult> UpdateOrder(long id, string statusType)
        {

            var order = _context.Orders.
                                Where(x => x.Id == id).
                                Include(x => x.OrderItems).
                                SingleOrDefault();

            var user = await _userManager.FindByIdAsync(order.UserId);

            // update order status
            OrderHelper.UpdateOrderStatus(id, OrderHelper.StatusId(statusType, _context), _context);

            // send confirmation email
            switch (statusType)
            {
                case "Awaiting Stock":
                    await _emailSender.SendEmailAsync(user.Email, "News about your order: " + id, AwaitingStockMessage(order, user));
                    break;
                case "Order Dispatched":
                    await _emailSender.SendEmailAsync(user.Email, "Dispatch Confirmation: " + id, OrderDispatchedMessage(order, user));
                    break;
                default:
                    break;
            }

            return UpdateResult.Success;

        }

        private string AwaitingStockMessage(Order order, ApplicationUser user)
        {
            var message = "<!DOCTYPE html><html lang='en'><head><style>body,h1,h2,h3,h4,h5,h6 {font-family: 'Lato', sans-serif;} body, html {height: 100%;color: #777;line-height: 1.8;} .w3-button:hover{color:#000!important;background-color:#ccc!important} .w3-button{border:none;display:inline-block;padding:8px 16px;vertical-align:middle;overflow:hidden;text-decoration:none;color:inherit;background-color:inherit;text-align:center;cursor:pointer;white-space:nowrap} .w3-round,.w3-round-medium{border-radius:4px} .w3-dark-grey,.w3-hover-dark-grey:hover,.w3-dark-gray,.w3-hover-dark-gray:hover{color:#fff!important;background-color:#616161!important} .w3-table,.w3-table-all{border-collapse:collapse;border-spacing:0;width:100%;display:table}.w3-table-all{border:1px solid #ccc} </style></head><body><p>Hi ";
            message += user.Forename + ", Some of the items in your order are out of stock. We will notify you when they are back in stock and your order has been sent.</p><p>Please could you check your emails (including your Spam box) once you have placed your order, as we may have to contact you regarding your order. This is especially important for items requiring personalisation.</p><p><a href='https://gustafsgallery.co.uk/Orders/YourOrders?id=10' class='w3-button w3-dark-grey w3-round'>View Order</a></p><h4>Order Summary</h4><hr><table class='w3-table' style='width: 100%'><tbody>";

            foreach (var item in order.OrderItems)
            {
                item.Product = OrderHelper.GetProduct(item, _context);
                message += "<tr><td>" + item.Product.Title + "</td></tr>";
            }

            message += "</tbody></table></body></html> ";

            return message;
        }

        string OrderDispatchedMessage(Order order, ApplicationUser user)
        {
            var message = "<!DOCTYPE html><html lang='en'><head><style>body,h1,h2,h3,h4,h5,h6 {font-family: 'Lato', sans-serif;} body, html {height: 100%;color: #777;line-height: 1.8;} .w3-button:hover{color:#000!important;background-color:#ccc!important} .w3-button{border:none;display:inline-block;padding:8px 16px;vertical-align:middle;overflow:hidden;text-decoration:none;color:inherit;background-color:inherit;text-align:center;cursor:pointer;white-space:nowrap} .w3-round,.w3-round-medium{border-radius:4px} .w3-dark-grey,.w3-hover-dark-grey:hover,.w3-dark-gray,.w3-hover-dark-gray:hover{color:#fff!important;background-color:#616161!important} .w3-table,.w3-table-all{border-collapse:collapse;border-spacing:0;width:100%;display:table}.w3-table-all{border:1px solid #ccc} </style></head><body><p>Hi ";
            message += user.Forename + ", Your order has been sent.</p><p>View your order details to see postage details and track your package.</p><p>Please could you check your emails (including your Spam box) once you have placed your order, as we may have to contact you regarding your order. This is especially important for items requiring personalisation.</p><p><a href='https://gustafsgallery.co.uk/Orders/YourOrders?id=10' class='w3-button w3-dark-grey w3-round'>View Order</a></p><h4>Order Summary</h4><hr><table class='w3-table' style='width: 100%'><tbody>";

            foreach (var item in order.OrderItems)
            {
                item.Product = OrderHelper.GetProduct(item, _context);
                message += "<tr><td>" + item.Product.Title + "</td></tr>";
            }

            message += "</tbody></table></body></html> ";

            return message;
        }
        #endregion
    }
}
