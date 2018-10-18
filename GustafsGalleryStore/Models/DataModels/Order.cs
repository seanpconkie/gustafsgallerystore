using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace GustafsGalleryStore.Models.DataModels
{
    public class Order
    {
        public long Id { get; set; }
        public decimal OrderTotalPrice { get; set; }
        public decimal OrderTotalPostagePrice { get; set; }

        public DateTime? OpenedDate { get; set; }
        [Display(Name = "Order Placed Date")]
        public DateTime? OrderPlacedDate { get; set; }
        [Display(Name = "Order Complete Date")]
        public DateTime? OrderCompleteDate { get; set; }
        public List<OrderItem> OrderItems { get; set; }

        public OrderStatus OrderStatus { get; set; }
        [Display(Name = "Order Status")]
        public long OrderStatusId { get; set; }

        public long CustomerContactId { get; set; }
        public CustomerContact CustomerContact { get; set; }

        public long DeliveryTypeId { get; set; }
        [Display(Name = "Delivery Method")]
        public DeliveryType DeliveryType { get; set; }

        public string PackageReference { get; set; }

        public string UserId { get; set; }

        public string PaymentId { get; set; }
        public string PaymentMessage { get; set; }
        public string PaymentStatus { get; set; }
        public string SellerMessage { get; set; }
        public string StripeSource { get; set; }

    }
}
