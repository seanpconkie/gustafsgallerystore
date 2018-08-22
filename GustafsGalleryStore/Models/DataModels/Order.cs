using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace GustafsGalleryStore.Models.DataModels
{
    public class Order
    {
        public long Id { get; set; }
        public float OrderTotalPrice { get; set; }
        public float OrderTotalPostagePrice { get; set; }

        public DateTime? OpenedDate { get; set; }
        [Display(Name = "Order Placed Date")]
        public DateTime? OrderPlacedDate { get; set; }
        [Display(Name = "Order Complete Date")]
        public DateTime? OrderCompleteDate { get; set; }
        public List<OrderItem> OrderItems { get; set; }

        public ProductBrand OrderStatus { get; set; }
        [Display(Name = "Order Status")]
        public long OrderStatusId { get; set; }

    }
}
