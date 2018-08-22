using System;
using System.ComponentModel.DataAnnotations;
namespace GustafsGalleryStore.Models.DataModels
{
    public class OrderHistory
    {
       
        public long Id { get; set; }
        public long OrderId { get; set; }

        public ProductBrand OrderStatus { get; set; }

        [Display(Name = "Order Status")]
        public long OrderStatusId { get; set; }

        [Display(Name = "Date")]
        public DateTime? DateStamp { get; set; }
    }
}
