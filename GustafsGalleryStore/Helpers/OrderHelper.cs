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
                if (status.Status == "Order Placed")
                {
                    inDb.OrderPlacedDate = DateTime.Now;
                }
                else if (status.Status == "Order Dispatched")
                {
                    inDb.OrderCompleteDate = DateTime.Now;
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
                                Include(x => x.CustomerContact).
                                Include(x => x.DeliveryType).
                                SingleOrDefault();

            order.OrderItems = context.OrderItems.Where(x => x.OrderId == id).ToList();

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

                    orderItems.Add(item);

                    totalPrice += (product.Price * item.Quantity);

                }
            }

            order.OrderTotalPrice = totalPrice;
            order.OrderItems = orderItems;

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
                               Include(x => x.OrderItems).
                               SingleOrDefault();

            var userOrders = context.Orders.
                                    Where(x => x.UserId == userId).
                                    Where(x => x.Id != id).
                                    Include(x => x.OrderItems).
                                    ToList();

            foreach (var order in userOrders)
            {

                if (order.OrderItems.Count > 0)
                {
                    foreach (var item in order.OrderItems)
                    {
                        order.OrderItems.Add(item);

                        context.Remove(item);
                    }
                }

                context.Remove(order);

            }

            context.SaveChanges();

        }
    }
}
