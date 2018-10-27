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
    [Authorize(Roles = MasterStrings.StaffRole)]
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


        #endregion

        // GET: /<controller>/
        public IActionResult Index(string statusMessage = null, string successMessage = null, string failureMessage = null)
        {
            var userId = _userManager.GetUserId(User);
            var viewModel = new ManageOrdersViewModel()
            {
                NewOrders = _context.Orders.
                                Where(x => x.OrderStatusId == OrderHelper.StatusId(MasterStrings.OrderPlaced, _context)).
                                Include(x => x.OrderStatus).
                                Include(x => x.DeliveryType).
                                OrderByDescending(x => x.OrderPlacedDate).
                                ToList(),
                AwaitingStock = _context.Orders.
                                    Where(x => x.OrderStatusId == OrderHelper.StatusId(MasterStrings.AwaitingStock, _context)).
                                    Include(x => x.OrderStatus).
                                    Include(x => x.DeliveryType).
                                    OrderByDescending(x => x.OrderPlacedDate).
                                    ToList(),
                CancelledOrders = _context.Orders.
                                    Where(x => x.OrderStatusId == OrderHelper.StatusId(MasterStrings.OrderCancelled, _context)).
                                    Include(x => x.OrderStatus).
                                    Include(x => x.DeliveryType).
                                    ToList(),
                OpenReturns = _context.Orders.
                                Where(x => x.OrderStatusId == OrderHelper.StatusId(MasterStrings.AwaitingReturn, _context)).
                                Include(x => x.OrderStatus).
                                Include(x => x.DeliveryType).
                                ToList(),
                StatusMessage = statusMessage,
                SuccessMessage = successMessage,
                FailureMessage = failureMessage
            };

            var searchTerm = "select * from dbo.orders where orderstatusid in (" + 
                               OrderHelper.StatusId(MasterStrings.OrderDispatched, _context) + "," + 
                               OrderHelper.StatusId(MasterStrings.OrderReturned, _context) + "," + 
                               OrderHelper.StatusId(MasterStrings.CancellationCompleted,_context) + ")";

            var completed = _context.Orders.FromSql(searchTerm).ToList();

            viewModel.CompletedOrders = completed;

            //clear old orders
            DateTime date = DateTime.Now.AddDays(-30);

            List<Order> orders = _context.Orders.
                                    Where(x => x.OpenedDate < date).
                                    Where(x => x.OrderStatusId == OrderHelper.StatusId(MasterStrings.Basket, _context)).
                                    ToList();

            foreach (var item in orders)
            {
                _context.RemoveRange(_context.OrderItems.Where(x => x.OrderId == item.Id));

                _context.Remove(item);

            }

            _context.SaveChanges();

            return View(viewModel);
        }

        public async Task<IActionResult> ViewOrder(long id, string statusMessage = null, string successMessage = null, string failureMessage = null)
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

            await UpdateOrder(id, MasterStrings.AwaitingStock);

            var redirectUrl = string.Format("/ManageOrders/ViewOrder?id={0}&&successMessage={1}.", id, "Order updated");

            return ControllerHelper.RedirectToLocal(this, redirectUrl);
        }


        public async Task<IActionResult> CancelOrder(long id)
        {
            var order = _context.Orders.
                                Where(x => x.Id == id).
                                Include(x => x.DeliveryType).
                                SingleOrDefault();
            UpdateResult result = UpdateResult.Error;
            string failureMessage = null;
            string successMessage = null;

            if (!string.IsNullOrWhiteSpace(order.StripeSource))
            {

                int amount = Convert.ToInt32((order.OrderTotalPrice) * 100);

                var stripeResult = StripeHelper.RefundCharge(order.PaymentId, amount);

                order.RefundStatus = stripeResult.Status;
                order.RefundCreatedDate = stripeResult.Created;

                if (stripeResult.Status == MasterStrings.StripeResultSucceeded || stripeResult.Status == MasterStrings.StripeResultPending)
                {
                    order.RefundId = stripeResult.BalanceTransactionId;
                    result = UpdateResult.Success;
                }

                if (stripeResult.Status == MasterStrings.StripeResultFailed || stripeResult.Status == MasterStrings.StripeResultCancelled)
                {
                    order.RefundId = stripeResult.FailureBalanceTransactionId;
                    order.RefundMessage = stripeResult.FailureReason;

                    failureMessage = "Refund failed: " + stripeResult.FailureReason;

                    result = UpdateResult.Error;

                }
            }
            else if (!string.IsNullOrWhiteSpace(order.PayPalSaleId))
            {
                var payPalResult = PayPalHelper.RefundPayment(order.PayPalSaleId, order.OrderTotalPrice);

                order.RefundStatus = payPalResult.state;
                order.RefundCreatedDate = Convert.ToDateTime(payPalResult.create_time);
                order.RefundId = payPalResult.id;
                order.RefundMessage = payPalResult.reason_code;

                if (payPalResult.state == MasterStrings.PayPalResultCompleted || payPalResult.state == MasterStrings.PayPalResultPending)
                {
                    result = UpdateResult.Success;
                }

                if (payPalResult.state == MasterStrings.PayPalResultFailed || payPalResult.state == MasterStrings.PayPalResultCancelled)
                {
                    failureMessage = "Refund failed";
                    result = UpdateResult.Error;
                }
            }
            else
            {
                failureMessage = "Refund couldn't be completed.  Please complete this via payment handler.";
            }

            if (result == UpdateResult.Success)
            {
                order.CancellationCompletedDate = DateTime.Now;
                await UpdateOrder(id, MasterStrings.CancellationCompleted);
                successMessage = "Order updated.";
            }

            _context.Update(order);
            _context.SaveChanges();
            
            var redirectUrl = string.Format("/ManageOrders/ViewOrder?id={0}", id);

            if (string.IsNullOrWhiteSpace(failureMessage))
            {
                redirectUrl += string.Format("&&failureMessage={0}", failureMessage);
            }
            if (string.IsNullOrWhiteSpace(successMessage))
            {
                redirectUrl += string.Format("&&successMessage={0}", successMessage);
            }

            return ControllerHelper.RedirectToLocal(this, redirectUrl);
        }

        public async Task<IActionResult> OrderDispatched(long id, string packageReference)
        {
            var order = _context.Orders.Where(x => x.Id == id).SingleOrDefault();
            var redirectUrl = string.Format("/ManageOrders/ViewOrder?id={0}", id);
            string failureMessage = null;
            string successMessage = null;

            if (string.IsNullOrWhiteSpace(packageReference))
            {
                failureMessage = "Please enter Package Reference.";

                redirectUrl += string.Format("&&failureMessage={0}", failureMessage);

                return ControllerHelper.RedirectToLocal(this, redirectUrl);
            }

            order.PackageReference = packageReference;

            _context.Update(order);
            _context.SaveChanges();

            await UpdateOrder(id, MasterStrings.OrderDispatched);

            successMessage = "Order updated.";


            if (string.IsNullOrWhiteSpace(successMessage))
            {
                redirectUrl += string.Format("&&successMessage={0}", successMessage);
            }

            return ControllerHelper.RedirectToLocal(this, redirectUrl);
        }

        public async Task<IActionResult> CompleteReturn(long id, decimal returnAmount)
        {
            var returnItem = _context.Returns.
                                     Where(x => x.OrderId == id).
                                     Where(x => x.ReturnCompleteDate == null).
                                     SingleOrDefault();
            var order = _context.Orders.Where(x => x.Id == returnItem.OrderId).SingleOrDefault();
            UpdateResult result = UpdateResult.Error;
            string failureMessage = null;
            string successMessage = null;

            int amount = Convert.ToInt32(returnAmount * 100);

            if (!string.IsNullOrWhiteSpace(order.StripeSource) && amount > 0)
            {
                var stripeResult = StripeHelper.RefundCharge(order.PaymentId, amount);

                returnItem.RefundStatus = stripeResult.Status;
                returnItem.RefundCreatedDate = stripeResult.Created;

                if (stripeResult.Status == MasterStrings.StripeResultSucceeded || stripeResult.Status == MasterStrings.StripeResultPending)
                {
                    returnItem.RefundId = stripeResult.BalanceTransactionId;
                    result = UpdateResult.Success;
                }

                if (stripeResult.Status == MasterStrings.StripeResultFailed || stripeResult.Status == MasterStrings.StripeResultCancelled)
                {
                    returnItem.RefundId = stripeResult.FailureBalanceTransactionId;
                    returnItem.RefundMessage = stripeResult.FailureReason;

                    failureMessage = "Refund failed: " + stripeResult.FailureReason;

                    result = UpdateResult.Error;

                }
            }
            else if (!string.IsNullOrWhiteSpace(order.PayPalSaleId) && amount > 0)
            {
                var payPalResult = PayPalHelper.RefundPayment(order.PayPalSaleId, amount);

                order.RefundStatus = payPalResult.state;
                order.RefundCreatedDate = Convert.ToDateTime(payPalResult.create_time);
                order.RefundId = payPalResult.id;
                order.RefundMessage = payPalResult.reason_code;

                if (payPalResult.state == MasterStrings.PayPalResultCompleted || payPalResult.state == MasterStrings.PayPalResultPending)
                {
                    result = UpdateResult.Success;
                }

                if (payPalResult.state == MasterStrings.PayPalResultFailed || payPalResult.state == MasterStrings.PayPalResultCancelled)
                {
                    failureMessage = "Refund failed";
                    result = UpdateResult.Error;
                }
            }
            else if (amount == 0)
            {
                result = UpdateResult.Success;
            }
            else
            {
                failureMessage = "Refund couldn't be completed.  Please complete this via payment handler.";
            }

            if (result == UpdateResult.Success)
            {
                returnItem.ReturnCompleteDate = DateTime.Now;
                await UpdateOrder(order.Id, MasterStrings.OrderReturned);
                successMessage = "Order updated.";
            }

            _context.Update(returnItem);
            _context.SaveChanges();


            var redirectUrl = string.Format("/ManageOrders/ViewOrder?id={0}", id);

            if (string.IsNullOrWhiteSpace(failureMessage))
            {
                redirectUrl += string.Format("&&failureMessage={0}", failureMessage);
            }
            if (string.IsNullOrWhiteSpace(successMessage))
            {
                redirectUrl += string.Format("&&successMessage={0}", successMessage);
            }

            return ControllerHelper.RedirectToLocal(this, redirectUrl);
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

            var orderSummary = MasterStrings.OrderSummaryStart;

            foreach (var item in order.OrderItems)
            {
                item.Product = OrderHelper.GetProduct(item, _context);
                orderSummary += "<tr><td>" + item.Product.Title + "</td></tr>";
            }

            orderSummary += MasterStrings.OrderSummaryFinish;


            // send confirmation email
            switch (statusType)
            {
                case MasterStrings.AwaitingStock:
                    await _emailSender.SendEmailAsync(user.Email, "News about your order: " + id, AwaitingStockMessage(order.Id, user, orderSummary));
                    break;
                case MasterStrings.OrderDispatched:
                    await _emailSender.SendEmailAsync(user.Email, "Dispatch Confirmation: " + id, OrderDispatchedMessage(order.Id, user, orderSummary));
                    break;
                case MasterStrings.CancellationCompleted:
                    await _emailSender.SendEmailAsync(user.Email, "Order Cancellation: " + id, OrderCancelledMessage(order.Id, user, orderSummary));
                    break;
                case MasterStrings.OrderReturned:
                    await _emailSender.SendEmailAsync(user.Email, "Return: " + id, OrderReturnMessage(order.Id, user, orderSummary));
                    break;
                default:
                    break;
            }

            return UpdateResult.Success;

        }

        private string OrderReturnMessage(long id, ApplicationUser user, string orderSummary)
        {

            var message = MasterStrings.Header;
            message += "<body>";
            message += string.Format(MasterStrings.OrderReturnMessage, user.Forename);
            message += MasterStrings.SpamMessage;
            message += string.Format(MasterStrings.CustomerOrderLink, id);
            message += orderSummary;
            message += "</body></html> ";

            return message;
        }
        private string OrderCancelledMessage(long id, ApplicationUser user, string orderSummary)
        {

            var message = MasterStrings.Header;
            message += "<body>";
            message += string.Format(MasterStrings.OrderCancelledMessage, user.Forename, id);
            message += MasterStrings.SpamMessage;
            message += string.Format(MasterStrings.CustomerOrderLink, id);
            message += orderSummary;
            message += "</body></html> ";

            return message;
        }

        private string AwaitingStockMessage(long id, ApplicationUser user, string orderSummary)
        {

            var message = MasterStrings.Header;
            message += "<body>";
            message += string.Format(MasterStrings.AwaitingStockMessage, user.Forename);
            message += MasterStrings.SpamMessage;
            message += string.Format(MasterStrings.CustomerOrderLink, id);
            message += orderSummary;
            message += "</body></html> ";

            return message;
        }

        private string OrderDispatchedMessage(long id, ApplicationUser user, string orderSummary)
        {

            var message = MasterStrings.Header;
            message += "<body>";
            message += string.Format(MasterStrings.OrderDispatchedMessage, user.Forename);
            message += MasterStrings.SpamMessage;
            message += string.Format(MasterStrings.CustomerOrderLink, id);
            message += orderSummary;
            message += "</body></html> ";

            return message;
        }
        #endregion
    }
}
