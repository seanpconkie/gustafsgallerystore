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
    [Authorize]
    public class CheckoutController : Controller
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

        public CheckoutController(
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

        // GET: /<controller>/
        public IActionResult Index(long id, string statusMessage = null, string successMessage = null, string failureMessage = null)
        {

            //return ControllerHelper.RedirectToLocal(this, "/Home/ComingSoon");

            var userId = _userManager.GetUserId(User);
            var order = _context.Orders.
                                Where(x => x.Id == id).
                                Where(x => x.OrderStatusId == StatusId(MasterStrings.Basket)).
                                SingleOrDefault();

            if (order == null)
            {
                return ControllerHelper.RedirectToLocal(this, "/Home?failureMessage=No basket found");
            }

            if (order.UserId == null)
            {
                // set basket user id
                order.UserId = userId;

                _context.Update(order);
                _context.SaveChanges();
            }

            var viewModel = new CheckoutViewModel()
            {
                Basket = OrderHelper.GetOrder(order.Id,_context),
                DeliveryTypes = _context.DeliveryTypes.
                                    Where(x => x.Id > 0).
                                    Include(x => x.DeliveryCompany).
                                    ToList(),
                Contacts = _context.CustomerContacts.
                                    Where(x => x.UserId == userId).
                                    ToList(),
                StatusMessage = statusMessage,
                FailureMessage = failureMessage,
                SuccessMessage = successMessage
            };

            return View(viewModel);

        }

        [HttpPost]
        public async Task<IActionResult> Index(CheckoutViewModel model)
        {
            bool isValid = true;
            var user = await _userManager.GetUserAsync(User);

            // stripe token?
            if (string.IsNullOrWhiteSpace(model.StripeToken) && 
               string.IsNullOrWhiteSpace(model.PayPal))
            {
                isValid = false;
                model.StatusMessage += "Payment details required.";
            }

            // DeliveryType?
            if (model.Basket.DeliveryType.Id == 0)
            {
                isValid = false;
                model.StatusMessage += "Delivery method required.";
            }

            // address
            if (string.IsNullOrWhiteSpace(model.Basket.CustomerContact.AddressLine1) ||
                string.IsNullOrWhiteSpace(model.Basket.CustomerContact.PostTown) ||
                string.IsNullOrWhiteSpace(model.Basket.CustomerContact.Postcode))
            {
                isValid = false;
                model.StatusMessage += "Delivery address required. Please ensure Address Line 1, Post Town and Post Code are completed.";
            }

            if (isValid)
            {

                // update contact address and delivery type
                var inDb = OrderHelper.GetOrder(model.Basket.Id, _context);

                var deliveryType = _context.DeliveryTypes.
                                           Where(x => x.Id == model.Basket.DeliveryType.Id).
                                           SingleOrDefault();

                inDb.DeliveryType = deliveryType;

                var contact = new CustomerContact()
                {
                    BuildingNumber = model.Basket.CustomerContact.BuildingNumber,
                    AddressLine1 = model.Basket.CustomerContact.AddressLine1,
                    AddressLine2 = model.Basket.CustomerContact.AddressLine2,
                    PostTown = model.Basket.CustomerContact.PostTown,
                    County = model.Basket.CustomerContact.County,
                    Postcode = model.Basket.CustomerContact.Postcode,
                    Country = model.Basket.CustomerContact.Country,
                    MobilePhone = model.Basket.CustomerContact.MobilePhone,
                    WorkPhone = model.Basket.CustomerContact.WorkPhone,
                    OtherPhone = model.Basket.CustomerContact.OtherPhone,
                    UserId = user.Id
                };

                decimal orderTotalPrice = 0;

                foreach (var item in inDb.OrderItems)
                {
                    var product = _context.Products.
                                           Where(x => x.Id == item.ProductId).
                                           SingleOrDefault();

                    orderTotalPrice += product.Price;
                }

                inDb.CustomerContact = contact;
                inDb.OrderTotalPrice = orderTotalPrice + inDb.DeliveryType.Price;

                // create charge
                if (!string.IsNullOrWhiteSpace(model.StripeToken))
                {

                    // create charge
                    int amount = Convert.ToInt32(inDb.OrderTotalPrice * 100);

                    if (model.ThreeDSecure == MasterStrings.ThreeDSecureRequired || model.ThreeDSecure == MasterStrings.ThreeDSecureRecommended)
                    {
                        var source = StripeHelper.CreateSource(model.StripeToken, amount, "http://localhost:5000/Checkout/ThreeDSecure");

                        inDb.StripeSource = source.Id;

                        _context.Update(inDb);
                        _context.SaveChanges();

                        return Redirect(source.Redirect.Url);
                    }

                    var result = StripeHelper.CreateCharge(model.StripeToken, model.Basket.Id, "OrderId", amount);

                    inDb.SellerMessage = result.SellerMessage;
                    inDb.PaymentId = result.Id;
                    inDb.PaymentStatus = result.NetworkStatus;
                    inDb.StripeSource = model.StripeToken;

                    _context.Update(inDb);
                    _context.SaveChanges();


                    if (result.NetworkStatus == MasterStrings.StripeDeclined)
                    {
                        var messages = new DeclineMessageViewModel();
                        messages = StripeHelper.DeclineMessage(result.Reason);

                        model.FailureMessage += messages.StatusMessage;
                        model.Basket.PaymentMessage = messages.PaymentMessage;

                        return ControllerHelper.RedirectToLocal(this, "/Checkout?id=" + model.Basket.Id + "&&failureMessage=" + model.FailureMessage);

                    }

                    if (result.NetworkStatus == MasterStrings.StripeApproved)
                    {

                        await PlaceOrder(model.Basket.Id);

                        return ControllerHelper.RedirectToLocal(this, "/Checkout/OrderPlaced?id=" + model.Basket.Id);
                    }

                    return ControllerHelper.RedirectToLocal(this, "/Checkout?id=" + model.Basket.Id + "&&failureMessage=Something went wrong with your payment, please try again.");


                }
                if (!string.IsNullOrWhiteSpace(model.PayPal))
                {
                    // process paypal

                    // create charge
                    var createdPayment = PayPalHelper.CreatePayment(model.Basket.Id, inDb.OrderItems, inDb.DeliveryType.Price, _context);
                    var redirect = "";

                    inDb.PayPalPaymentId = createdPayment.id;
                    inDb.PaymentId = createdPayment.transactions[0].invoice_number;

                    _context.Update(inDb);
                    _context.SaveChanges();

                    var links = createdPayment.links.GetEnumerator();
                    while (links.MoveNext())
                    {
                        var link = links.Current;
                        if (link.rel.ToLower().Trim().Equals("approval_url"))
                        {
                            // Redirect the customer to link.href
                            redirect = link.href;
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(redirect))
                    {
                        return Redirect(redirect);
                    }
                }

            }

            model.Basket = OrderHelper.GetOrder(model.Basket.Id, _context);

            model.DeliveryTypes = _context.DeliveryTypes.
                                Where(x => x.Id > 0).
                                Include(x => x.DeliveryCompany).
                                ToList();
            model.Contacts = _context.CustomerContacts.
                                Where(x => x.UserId == user.Id).
                               ToList();

            return View(model);

        }

        public IActionResult OrderPlaced (long id)
        {
            var order = OrderHelper.GetOrder(id, _context);

            return View(order);
        }

        public async Task<IActionResult> ThreeDSecure(string source, bool livemode, string client_secret)
        {

            var order = _context.Orders.Where(x => x.StripeSource == source).Include(x => x.DeliveryType).SingleOrDefault();

            var userId = _userManager.GetUserId(User);
            var viewModel = new CheckoutViewModel()
                {
                    Basket = OrderHelper.GetOrder(order.Id, _context),
                    DeliveryTypes = _context.DeliveryTypes.
                                        Where(x => x.Id > 0).
                                        Include(x => x.DeliveryCompany).
                                        ToList(),
                    Contacts = _context.CustomerContacts.
                                        Where(x => x.UserId == userId).
                                        ToList()
                };

            var result = StripeHelper.ChargeSource(source,livemode,client_secret, order.Id, "OrderId");

            order.SellerMessage = result.SellerMessage;
            order.PaymentId = result.Id;
            order.PaymentStatus = result.NetworkStatus;

            try
            {
                _context.Update(order);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            if (result.NetworkStatus == MasterStrings.StripeDeclined)
            {
                var messages = new DeclineMessageViewModel();
                messages = StripeHelper.DeclineMessage(result.Reason);

                var statusMessage = messages.StatusMessage;
                order.PaymentMessage = messages.PaymentMessage;

                return ControllerHelper.RedirectToLocal(this,"/Checkout/Index?id=" + order.Id + "&&failureMessage=" + statusMessage);

            }

            if (result.NetworkStatus == MasterStrings.StripeApproved ||
                result.NetworkStatus == MasterStrings.StripeNotSent && result.Type == MasterStrings.StripeResultPending)
            {
                await PlaceOrder(order.Id);

                return ControllerHelper.RedirectToLocal(this, "/Checkout/OrderPlaced?id=" + order.Id);
            }

            if (result.NetworkStatus == MasterStrings.StripeReversed || 
                (result.NetworkStatus == MasterStrings.StripeNotSent && result.Type != MasterStrings.StripeResultPending))
            {

                var statusMessage = "The payment has been declined.<br/>Please contact your card issuer for more information.";
                order.PaymentMessage = "Status reason: " + result.Reason;

                return ControllerHelper.RedirectToLocal(this, "/Checkout/Index?id=" + order.Id + "&&failureMessage" + statusMessage);

            }

            return View();

        }

        public async Task<IActionResult> PayPalComplete(string paymentId, string token, string payerId)
        {

            var order = _context.Orders.
                                Where(x => x.PayPalPaymentId == paymentId).
                                Include(x => x.DeliveryType).
                                Include(x => x.OrderItems).
                                Include(x => x.CustomerContact).
                                SingleOrDefault();

            var userId = _userManager.GetUserId(User);
            var viewModel = new CheckoutViewModel()
            {
                Basket = OrderHelper.GetOrder(order.Id, _context),
                DeliveryTypes = _context.DeliveryTypes.
                                        Where(x => x.Id > 0).
                                        Include(x => x.DeliveryCompany).
                                        ToList(),
                Contacts = _context.CustomerContacts.
                                        Where(x => x.UserId == userId).
                                        ToList()
            };

            var result = PayPalHelper.ChargePayment(paymentId, token, payerId,order.PaymentId,order.Id,order.OrderItems,order.DeliveryType.Price,_context);

            order.PaymentId = result.id;
            order.PaymentStatus = result.state;
            order.PayPalCartId = result.cart;
            order.PayPalPayerId = result.payer.payer_info.payer_id;
            order.PayPalSaleId = result.transactions[0].related_resources[0].sale.id;

            if(result.payer.payer_info.shipping_address != null)
            {
                order.CustomerContact.BuildingNumber = "";
                order.CustomerContact.AddressLine1 = result.payer.payer_info.shipping_address.line1;
                order.CustomerContact.AddressLine2 = result.payer.payer_info.shipping_address.line2;
                order.CustomerContact.PostTown = result.payer.payer_info.shipping_address.city;
                order.CustomerContact.County = result.payer.payer_info.shipping_address.state;
                order.CustomerContact.Postcode = result.payer.payer_info.shipping_address.postal_code;
                order.CustomerContact.Country = result.payer.payer_info.shipping_address.country_code;
                order.CustomerContact.OtherPhone = result.payer.payer_info.shipping_address.phone;
            }

            try
            {
                _context.Update(order);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            if (result.state == MasterStrings.PayPalResultFailed)
            {
                var message = "The payment has been declined.<br/>Please contact your card issuer for more information.";
                var statusMessage = message;
                order.PaymentMessage = message;

                return ControllerHelper.RedirectToLocal(this, "/Checkout/Index?id=" + order.Id + "&&failureMessage" + statusMessage);

            }

            if (result.state == MasterStrings.PayPalResultApproved)
            {
                await PlaceOrder(order.Id);

                return ControllerHelper.RedirectToLocal(this, "/Checkout/OrderPlaced?id=" + order.Id);
            }

            return View();

        }

        #region private methods

        private long StatusId(string status)
        {

            var statusInDb = _context.OrderStatuses.Where(x => x.Status == status).SingleOrDefault();
            if (statusInDb != null)
            {
                return statusInDb.Id;
            }

            return 0;
        }

        private async Task<UpdateResult> PlaceOrder(long id)
        {

            var user = await _userManager.GetUserAsync(User);

            // update order status
            OrderHelper.UpdateOrderStatus(id, OrderHelper.StatusId(MasterStrings.OrderPlaced, _context), _context);

            var orderSummary = MasterStrings.OrderSummaryStart;

            var order = _context.Orders.
                                Where(x => x.Id == id).
                                Include(x => x.OrderItems).
                                SingleOrDefault();

            foreach (var item in order.OrderItems)
            {
                item.Product = OrderHelper.GetProduct(item, _context);
                orderSummary += "<tr><td>" + item.Product.Title + "</td></tr>";
                item.Product.Stock--;
                _context.Update(item.Product);
            }

            _context.SaveChanges();

            orderSummary += MasterStrings.OrderSummaryFinish;

            // send confirmation email
            var customerMessage = MasterStrings.Header;
            customerMessage += "<body>";
            customerMessage += string.Format(MasterStrings.CustomerNewOrder, user.Forename);
            customerMessage += MasterStrings.SpamMessage;
            customerMessage += string.Format(MasterStrings.CustomerOrderLink, id);
            customerMessage += orderSummary;
            customerMessage += "</body></html> ";

            await _emailSender.SendEmailAsync(user.Email, "Order Confirmation: " + id, customerMessage);

            // send shop notification
            var storeMessage = MasterStrings.Header;
            storeMessage += "<body>";
            storeMessage += string.Format(MasterStrings.StoreNewOrder,MasterStrings.StoreName);
            storeMessage += string.Format(MasterStrings.StoreOrderLink,order.Id);
            storeMessage += orderSummary;
            storeMessage += "</body></html> ";

            await _emailSender.SendEmailAsync(MasterStrings.AdminEmail, "New Order: " + id, storeMessage);

            return UpdateResult.Success;

        }

        private string ProcessPayPal(CheckoutViewModel model, string userId)
        {

            var inDb = OrderHelper.GetOrder(model.Basket.Id, _context);


            return "/Checkout?id=" + model.Basket.Id + "&&failureMessage=Something went wrong with your payment, please try again.";

        }
        #endregion
    }
}
