using System;
using System.Collections.Generic;
using System.Linq;
using GustafsGalleryStore.Areas.Identity.Data;
using GustafsGalleryStore.Helpers;
using GustafsGalleryStore.Models.DataModels;
using PayPal.Api;

namespace GustafsGalleryStore.Services
{
    public class PayPalHelper
    {

        public static Payment CreatePayment(long id, List<OrderItem> products, decimal shipping, GustafsGalleryStoreContext context)
        {
            var clientId = MasterStrings.payPalClientId;
            var secretKey = MasterStrings.payPalSecretKey;

            var payer = new Payer()
            {
                payment_method = "paypal"
            };

            var guid = Guid.NewGuid();

            var redirectUrls = new RedirectUrls()
            {
                cancel_url = "http://localhost:5000/Checkout?id=" + id,
                return_url = "http://localhost:5000/Checkout/PayPalComplete"
            };


            var payment = new Payment()
            {
                intent = "sale",
                payer = payer,
                redirect_urls = redirectUrls,
                transactions = GetTransactions(id,products,shipping,guid.ToString(),context)
            };

            var createdPayment = payment.Create(GetAPIContext(clientId, secretKey));

            return createdPayment;
        }

        public static Payment ChargePayment(string paymentId, string token, string payerId, string guid, long id, List<OrderItem> products, decimal shipping, GustafsGalleryStoreContext context)
        {
            var clientId = MasterStrings.payPalClientId;
            var secretKey = MasterStrings.payPalSecretKey;

            // Using the information from the redirect, setup the payment to execute.
            var payment = new Payment() { id = paymentId };

            var transactionList = GetTransactions(id, products, shipping, guid.ToString(), context);
            var transactions = new List<Transaction>();
            var transaction = new Transaction() {
                amount = transactionList[0].amount
            };

            transactions.Add(transaction);

            var paymentExecution = new PaymentExecution() { 
                payer_id = payerId,
                transactions = transactions
            };

            try
            {
            // Execute the payment.
            var executedPayment = payment.Execute(GetAPIContext(clientId, secretKey, null), paymentExecution);

            return executedPayment;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }

        }

        public static Refund RefundPayment(string saleId, decimal refundAmount)
        {
            var clientId = MasterStrings.payPalClientId;
            var secretKey = MasterStrings.payPalSecretKey;

            var amount = new Amount() { 
                currency = "GBP",
                total = refundAmount.ToString()
            };

            var refund = new Refund() { 
                amount = amount,
                reason = "requested_by_customer",
                description = "GUSTAFS GALLERY ONLINE"
            };

            var sale = new Sale();

            var apiContext = GetAPIContext(clientId, secretKey);

            sale = Sale.Get(apiContext, saleId);

            try
            {
                var executedRefund = Sale.Refund(apiContext, sale.id, refund);

                return executedRefund;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }

            //var executedRefund = sale.Refund(GetAPIContext(clientId,secretKey),refund);

        }

        #region Private Methods
        private static APIContext GetAPIContext(string clientId, string secretKey, string token = null)
        {

            APIContext apiContext = null;

            if (string.IsNullOrWhiteSpace(token))
            {
                var accessToken = new OAuthTokenCredential(clientId, secretKey);

                apiContext = new APIContext(accessToken.GetAccessToken());

            }
            else
            {
                apiContext = new APIContext(token);
            }

            var config = new Dictionary<string, string>();
            config.Add("mode", "sandbox");
            config.Add("clientId", clientId);
            config.Add("clientSecret", secretKey);

            apiContext.Config = config;

            return apiContext;
        }

        private static List<Transaction> GetTransactions(long id, List<OrderItem> products, decimal shipping, string guid, GustafsGalleryStoreContext context)
        {

            var list = new List<Item>();
            decimal subtotal = 0;

            foreach (var item in products)
            {
                var product = context.Products.
                                     Where(x => x.Id == item.ProductId).
                                     SingleOrDefault();

                var newItem = new Item()
                {
                    name = product.Title,
                    currency = "GBP",
                    price = product.Price.ToString(),
                    quantity = item.Quantity.ToString()
                };

                list.Add(newItem);
                subtotal += product.Price;

            }

            var itemList = new ItemList()
            {
                items = list
            };

            var details = new Details()
            {
                shipping = shipping.ToString(),
                subtotal = subtotal.ToString()
            };

            var amount = new Amount()
            {
                currency = "GBP",
                total = (subtotal + shipping).ToString(), // Total must be equal to sum of shipping, tax and subtotal.
                details = details
            };

            var transactionList = new List<Transaction>();

            transactionList.Add(new Transaction()
            {
                description = "GUSTAFS GALLERY ONLINE - " + id.ToString(),
                invoice_number = guid,
                amount = amount,
                item_list = itemList
            });

            return transactionList;
        }
        #endregion


    }
}
