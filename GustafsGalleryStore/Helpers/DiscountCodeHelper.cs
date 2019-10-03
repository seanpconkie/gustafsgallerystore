using System;
using System.Linq;
using GustafsGalleryStore.Areas.Identity.Data;
using GustafsGalleryStore.Models.DataModels;

namespace GustafsGalleryStore.Helpers
{
    public static class DiscountCodeHelper
    {
        public static bool ValidateDiscountCode(string discountCode, GustafsGalleryStoreContext context)
        {
            // check is code valid
            Discount discount = context.Discounts.Where(x => x.Code == discountCode).SingleOrDefault();
            DateTime? now = DateTime.Now;

            if (discount == null)
            {
                return false;
            }

            // check is live
            if (!discount.IsLive)
            {
                return false;
            }

            // check has started
            if (discount.StartDate > now)
            {
                return false;
            }

            // check has ended
            if (discount.EndDate < now)
            {
                return false;
            }

            return true;
        }

        public static Discount GetDiscount(string discountCode, GustafsGalleryStoreContext context)
        {
            return context.Discounts.Where(x => x.Code == discountCode).SingleOrDefault();
        }

        public static Discount GetDiscount(long id, GustafsGalleryStoreContext context)
        {
            return context.Discounts.Where(x => x.Id == id).SingleOrDefault();
        }

        public static bool AddDiscountItem (long basketId, string discountCode, GustafsGalleryStoreContext context)
        {

            Order order = OrderHelper.GetOrder(basketId, context);
            Discount discount = DiscountCodeHelper.GetDiscount(discountCode, context);
            DiscountItem discountItem = new DiscountItem() { OrderId = order.Id, DiscountId = discount.Id };

            if (!order.Discounts.Contains(discountItem))
            {
                context.Add(discountItem);
                context.SaveChanges();
                return true;
            }

            return false;

        }
    }
}
