using System;
using System.Linq;
using GustafsGalleryStore.Areas.Identity.Data;
using GustafsGalleryStore.Models.DataModels;

namespace GustafsGalleryStore.Helpers
{
    public static class DiscountCodeHelper
    {
        public static string ValidateDiscountCode(string discountCode, GustafsGalleryStoreContext context)
        {
            // check is code valid
            Discount discount = context.Discounts.Where(x => x.Code == discountCode).SingleOrDefault();
            DateTime? now = DateTime.Now;

            if (discount == null)
            {
                return string.Format("Discount code ({0}) does not exist.",discountCode);
            }

            // check is live
            if (!discount.IsLive ||
            // check has started
                discount.StartDate > now ||
            // check if it has been used too often
                discount.UsageCount >= discount.MaxUsage
                )
            {
                return string.Format("Discount code ({0}) is not valid.", discountCode);
            }

            // check has ended
            if (discount.EndDate < now)
            {
                return string.Format("Discount code ({0}) is expired.", discountCode);
            }

            return "";
        }

        public static Discount GetDiscount(string discountCode, GustafsGalleryStoreContext context)
        {
            return context.Discounts.Where(x => x.Code == discountCode).SingleOrDefault();
        }

        public static Discount GetDiscount(long id, GustafsGalleryStoreContext context)
        {
            return context.Discounts.Where(x => x.Id == id).SingleOrDefault();
        }

        public static decimal GetMinSpend(string discountCode, GustafsGalleryStoreContext context)
        {
            return context.Discounts.Where(x => x.Code == discountCode).SingleOrDefault().MinSpend;
        }

        public static decimal GetMinSpend(long id, GustafsGalleryStoreContext context)
        {
            return context.Discounts.Where(x => x.Id == id).SingleOrDefault().MinSpend;
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

        public static bool IncreaseUsageCount(long orderId, GustafsGalleryStoreContext context)
        {
            // get order and increase usage of each discount
            Order order = OrderHelper.GetOrder(orderId, context);
            try
            {

                foreach (var discount in order.Discounts)
                {
                    var inDb = context.Discounts.Where(x => x.Id == discount.Discount.Id).SingleOrDefault();

                    inDb.UsageCount++;

                    context.Update(inDb);
                    context.SaveChanges();

                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
