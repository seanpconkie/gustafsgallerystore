using System;
namespace GustafsGalleryStore.Models.DataModels
{
    public class DiscountItem
    {
        public long Id { get; set; }
        public long DiscountId { get; set; }
        public long OrderId { get; set; }
    }
}
