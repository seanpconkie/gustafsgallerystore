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
        public IActionResult Index(long id, string statusMessage = null)
        {

            //return ControllerHelper.RedirectToLocal(this, "/Home/ComingSoon");

            var userId = _userManager.GetUserId(User);
            var order = _context.Orders.
                                Where(x => x.Id == id).
                                Where(x => x.OrderStatusId == StatusId("Basket")).
                                SingleOrDefault();

            if (order == null)
            {
                var paramName = "Basket";
                var message = "No basket found";
                throw new ArgumentNullException(paramName,message);
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
                StatusMessage = statusMessage
            };

            if (viewModel.Basket == null)
            {
                throw new System.Exception("No basket found.");
            }

            return View(viewModel);

        }

        [HttpPost]
        public async Task<IActionResult> Index(CheckoutViewModel model)
        {
            bool isValid = true;
            var user = await _userManager.GetUserAsync(User);

            // stripe token?
            if (string.IsNullOrWhiteSpace(model.StripeToken))
            {
                isValid = false;
                model.StatusMessage += "Payment details required.<br/>";
            }

            // DeliveryType?
            if (model.Basket.DeliveryType.Id == 0)
            {
                isValid = false;
                model.StatusMessage += "Delivery method required.<br/>";
            }

            // address
            if (string.IsNullOrWhiteSpace(model.Basket.CustomerContact.AddressLine1) ||
                string.IsNullOrWhiteSpace(model.Basket.CustomerContact.PostTown) ||
                string.IsNullOrWhiteSpace(model.Basket.CustomerContact.Postcode))
            {
                isValid = false;
                model.StatusMessage += "Delivery address required.<br/>Please ensure Address Line 1, Post Town and Post Code are completed.<br/>";
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

                inDb.CustomerContact = contact;
                inDb.OrderTotalPrice = inDb.OrderTotalPrice + inDb.DeliveryType.Price;

                try
                {
                    _context.Update(inDb);
                    _context.SaveChanges();
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }

                // create charge
                int amount = Convert.ToInt32(inDb.OrderTotalPrice * 100);

                if (model.ThreeDSecure == "required" || model.ThreeDSecure == "recommended")
                {
                    var source = StripeHelper.CreateSource(model.StripeToken, amount, "http://localhost:5000/Checkout/ThreeDSecure");

                    inDb.StripeSource = source.Id;

                    try
                    {
                        _context.Update(inDb);
                        _context.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.Message);
                    }

                    return Redirect(source.Redirect.Url);
                }

                var result = StripeHelper.CreateCharge(model.StripeToken,model.Basket.Id,"OrderId",amount);

                inDb.SellerMessage = result.SellerMessage;
                inDb.PaymentId = result.Id;
                inDb.PaymentStatus = result.NetworkStatus;
                inDb.StripeSource = model.StripeToken;

                try
                {
                    _context.Update(inDb);
                    _context.SaveChanges();
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }


                if (result.NetworkStatus == "declined_by_network" )
                {
                    var messages = new DeclineMessageViewModel();
                    messages = StripeHelper.DeclineMessage(result.Reason);

                    model.StatusMessage += messages.StatusMessage;
                    model.Basket.PaymentMessage = messages.PaymentMessage;

                }

                if (result.NetworkStatus == "approved_by_network")
                {

                    await PlaceOrder(model.Basket.Id);

                    return ControllerHelper.RedirectToLocal(this, "/Checkout/OrderPlaced?id=" + model.Basket.Id);
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
            //return ControllerHelper.RedirectToLocal(this, "/Home/ComingSoon");
            try
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

                if (result.NetworkStatus == "declined_by_network")
                {
                    var messages = new DeclineMessageViewModel();
                    messages = StripeHelper.DeclineMessage(result.Reason);

                    var statusMessage = messages.StatusMessage;
                    order.PaymentMessage = messages.PaymentMessage;

                    return ControllerHelper.RedirectToLocal(this,"/Checkout/Index?statusMessage" + statusMessage);

                }

                if (result.NetworkStatus == "approved_by_network" ||
                   result.NetworkStatus == "not_sent_to_network" && result.Type == "pending")
                {
                    await PlaceOrder(order.Id);

                    return ControllerHelper.RedirectToLocal(this, "/Checkout/OrderPlaced?id=" + order.Id);
                }

                if (result.NetworkStatus == "reversed_after_approval" || 
                    (result.NetworkStatus == "not_sent_to_network" && result.Type != "pending"))
                {

                    var statusMessage = "The payment has been declined.<br/>Please contact your card issuer for more information.";
                    order.PaymentMessage = "Status reason: " + result.Reason;

                    return ControllerHelper.RedirectToLocal(this, "/Checkout/Index?statusMessage" + statusMessage);

                }

                return View();

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

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
            OrderHelper.UpdateOrderStatus(id, OrderHelper.StatusId("Order Placed", _context), _context);

            var message = "<!DOCTYPE html><html lang='en'><head><style>body,h1,h2,h3,h4,h5,h6 {font-family: 'Lato', sans-serif;} body, html {height: 100%;color: #777;line-height: 1.8;} .w3-button:hover{color:#000!important;background-color:#ccc!important} .w3-button{border:none;display:inline-block;padding:8px 16px;vertical-align:middle;overflow:hidden;text-decoration:none;color:inherit;background-color:inherit;text-align:center;cursor:pointer;white-space:nowrap} .w3-round,.w3-round-medium{border-radius:4px} .w3-dark-grey,.w3-hover-dark-grey:hover,.w3-dark-gray,.w3-hover-dark-gray:hover{color:#fff!important;background-color:#616161!important} .w3-table,.w3-table-all{border-collapse:collapse;border-spacing:0;width:100%;display:table}.w3-table-all{border:1px solid #ccc} </style></head><body><h4>Thanks you for your purchase!</h4><hr><p>Hi ";
            message += user.Forename + string.Format(", We are processing your order. We will notify you when it has been sent.</p><p>Please could you check your emails (including your Spam box) once you have placed your order, as we may have to contact you regarding your order. This is especially important for items requiring personalisation.</p><p><a href='https://gustafsgallery.co.uk/Orders/YourOrders?id={0}' class='w3-button w3-dark-grey w3-round'>View Order</a></p><h4>Order Summary</h4><hr><table class='w3-table' style='width: 100%'><tbody>",id);

            var order = _context.Orders.
                                Where(x => x.Id == id).
                                Include(x => x.OrderItems).
                                SingleOrDefault();

            foreach (var item in order.OrderItems)
            {
                item.Product = OrderHelper.GetProduct(item, _context);
                message += "<tr><td>" + item.Product.Title + "</td></tr>";
                item.Product.Stock--;
                _context.Update(item.Product);
            }

            _context.SaveChanges();

            message += "</tbody></table></body></html> ";

            // send confirmation email
            await _emailSender.SendEmailAsync(user.Email, "Order Confirmation: " + id, message);
            // send shop notification
            message = "<!DOCTYPE html><html lang='en'><head><style>body,h1,h2,h3,h4,h5,h6 {font-family: 'Lato', sans-serif;} body, html {height: 100%;color: #777;line-height: 1.8;} .w3-button:hover{color:#000!important;background-color:#ccc!important} .w3-button{border:none;display:inline-block;padding:8px 16px;vertical-align:middle;overflow:hidden;text-decoration:none;color:inherit;background-color:inherit;text-align:center;cursor:pointer;white-space:nowrap} .w3-round,.w3-round-medium{border-radius:4px} .w3-dark-grey,.w3-hover-dark-grey:hover,.w3-dark-gray,.w3-hover-dark-gray:hover{color:#fff!important;background-color:#616161!important} .w3-table,.w3-table-all{border-collapse:collapse;border-spacing:0;width:100%;display:table}.w3-table-all{border:1px solid #ccc} </style></head><body><h4>You've had a new order!</h4><hr><p>Hi ";
            message += string.Format("Hi Gustaf and friends, you've received a new order.</p><p><a href='https://gustafsgallery.co.uk/Orders/YourOrders?id={0}' class='w3-button w3-dark-grey w3-round'>View Order</a></p><h4>Order Summary</h4><hr><table class='w3-table' style='width: 100%'><tbody>",order.Id);

            foreach (var item in order.OrderItems)
            {
                item.Product = OrderHelper.GetProduct(item, _context);
                message += "<tr><td>" + item.Product.Title + "</td></tr>";
            }

            message += "</tbody></table></body></html> ";

            await _emailSender.SendEmailAsync(user.Email, "New Order: " + id, message);

            return UpdateResult.Success;

        }

        #endregion
    }
}
