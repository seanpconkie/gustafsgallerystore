using System;
using System.ComponentModel.DataAnnotations;
namespace GustafsGalleryStore
{
    public class OrderHistory
    {
        public long OrderId { get; set; }

        public OrderStatus OrderStatus { get; set; }

        [Display(Name = "Order Status")]
        public long OrderStatusId { get; set; }

        [Display(Name = "Date")]
        public DateTime? DateStamp { get; set; }
    }
}
