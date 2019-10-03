using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using GustafsGalleryStore.Areas.Identity.Data;
using GustafsGalleryStore.Models.DataModels;

namespace GustafsGalleryStore.Helpers
{
    public static class OrderHelper
    {
        public static UpdateResult UpdateOrderStatus(long id, long statusId, GustafsGalleryStoreContext context)
        {
            var inDb = context.Orders.Where(x => x.Id == id).SingleOrDefault();

            if (inDb != null)
            {
                var status = context.OrderStatuses.Where(x => x.Id == statusId).SingleOrDefault();

                var history = new OrderHistory()
                {
                    OrderId = id,
                    OrderStatus = status,
                    DateStamp = DateTime.Now
                };

                context.Add(history);

                inDb.OrderStatus = status;

                switch (status.Status)
                {
                    case "Order Placed":
                        inDb.OrderPlacedDate = DateTime.Now;
                        break;
                    case "Order Dispatched":
                        inDb.OrderCompleteDate = DateTime.Now;
                        break;
                    case "Order Cancelled":
                        inDb.CancellationRequestedDate = DateTime.Now;
                        break;
                    case "Cancellation Completed":
                        inDb.CancellationCompletedDate = DateTime.Now;
                        break;
                    case "Return Completed":
                        inDb.OrderCompleteDate = DateTime.Now;
                        break;
                    case "Awaiting Return":
                        inDb.ReturnRequestedDate = DateTime.Now;
                        break;
                    case "Order Returned":
                        inDb.ReturnReceivedDate = DateTime.Now;
                        inDb.OrderCompleteDate = DateTime.Now;
                        break;
                    default:
                        break;
                }

                context.Update(inDb);

                try
                {
                    context.SaveChanges();
                    return UpdateResult.Success;
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }

            }

            return UpdateResult.Error;
        }


        public static Order GetOrder(long id, GustafsGalleryStoreContext context)
        {
            var order = context.Orders.
                            Where(x => x.Id == id).
                            SingleOrDefault();
            if (order == null)
            {
                return order;
            }

            order.OrderItems = context.OrderItems.
                                    Where(x => x.OrderId == id).
                                    ToList();

            // expand orderitems
            List<OrderItem> orderItems = new List<OrderItem>();
            decimal totalPrice = 0;

            if (order.OrderItems != null)
            {
                foreach (var item in order.OrderItems)
                {

                    var productId = item.ProductId;
                    var product = context.Products.Where(x => x.Id == productId).
                                      Include(c => c.ProductBrand).
                                      Include(c => c.ProductColours).
                                      Include(c => c.ProductSizes).
                                      Include(c => c.ProductImages).
                                      SingleOrDefault();

                    product.ProductBrand = context.ProductBrands.
                        Where(x => x.Id == product.ProductBrandId).
                        SingleOrDefault();

                    product.ProductSizes = context.ProductSizes.
                        Where(x => x.ProductId == item.ProductId).
                        ToList();

                    product.ProductColours = context.ProductColours.
                        Where(x => x.ProductId == item.ProductId).
                        ToList();

                    product.ProductImages = context.ProductImages.
                        Where(x => x.ProductId == item.ProductId).
                        ToList();

                    item.Product = product;
                    item.Colour = context.Colours.
                                    Where(x => x.Id == item.ColourId).
                                    SingleOrDefault();

                    item.Size = context.Sizes.
                                    Where(x => x.Id == item.SizeId).
                                    SingleOrDefault();

                    orderItems.Add(item);

                    totalPrice += (product.Price * item.Quantity);

                }
            }

            order.OrderSubTotalPrice = totalPrice;
            order.OrderItems = orderItems;

            order.CustomerContact = context.CustomerContacts.
                                        Where(x => x.Id == order.CustomerContactId).
                                        SingleOrDefault();

            order.DeliveryType = context.DeliveryTypes.
                                    Where(x => x.Id == order.DeliveryTypeId).
                                    SingleOrDefault();

            if (order.DeliveryType != null)
            {
                order.OrderTotalPostagePrice = order.DeliveryType.Price;
            }
            else
            {
                order.OrderTotalPostagePrice = 0;
            }

            List<DiscountItem> discountItems = context.DiscountItems.
                                Where(x => x.OrderId == order.Id).
                                ToList();

            decimal discountTotal = 0;

            foreach (var discountItem in discountItems)
            {
                Discount inDb = context.Discounts.Where(x => x.Id == discountItem.DiscountId).SingleOrDefault();
                if (inDb.Percentage > 0)
                {
                    inDb.Value = order.OrderSubTotalPrice * (inDb.Percentage / 100);
                }
                discountItem.Discount = inDb;
                discountTotal += inDb.Value;
            }

            order.OrderDiscountSubTotalPrice = discountTotal;

            order.Discounts = discountItems;

            order.OrderTotalPrice = (order.OrderSubTotalPrice + order.OrderTotalPostagePrice - order.OrderDiscountSubTotalPrice);

            order.DeliveryType.DeliveryCompany = context.DeliveryCompanies.
                                                    Where(x => x.Id == order.DeliveryType.DeliveryCompanyId).
                                                    SingleOrDefault();

            order.OrderStatus = context.OrderStatuses.
                                    Where(x => x.Id == order.OrderStatusId).
                                    SingleOrDefault();

            return order;
        }

        public static long StatusId(string status, GustafsGalleryStoreContext context)
        {

            var statusInDb = context.OrderStatuses.Where(x => x.Status == status).SingleOrDefault();
            if (statusInDb != null)
            {
                return statusInDb.Id;
            }

            return 0;
        }

        public static Product GetProduct(OrderItem item, GustafsGalleryStoreContext context)
        {
            var productId = item.ProductId;
            var product = context.Products.Where(x => x.Id == productId).
                                Include(x => x.ProductColours).
                                Include(x => x.ProductSizes).
                                SingleOrDefault();

            product.ProductBrand = context.ProductBrands.Where(x => x.Id == product.ProductBrandId).SingleOrDefault();
            product.ProductImages = context.ProductImages.Where(x => x.ProductId == item.ProductId).ToList();

            return product;
        }

        public static void UpdateUser(long id, string userId, GustafsGalleryStoreContext context)
        {
            var newOrder = context.Orders.
                                Where(x => x.Id == id).
                                SingleOrDefault();

            var userOrders = context.Orders.
                                Where(x => x.UserId == userId).
                                Where(x => x.Id != id).
                                Where(x => x.OrderStatusId == StatusId("Basket", context)).
                                ToList();

            foreach (var order in userOrders)
            {
                var orderItems = context.OrderItems.
                                        Where(x => x.OrderId == order.Id)
                                        .ToList();

                if (orderItems.Count > 0)
                {
                    foreach (var item in orderItems)
                    {

                        context.Remove(item);
                    }
                }

                context.Remove(order);

            }

            context.SaveChanges();

        }
    }
}
