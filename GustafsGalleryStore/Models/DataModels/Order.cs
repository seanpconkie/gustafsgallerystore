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

        [Display(Name = "Return Requested Date")]
        public DateTime? ReturnRequestedDate { get; set; }
        [Display(Name = "Return Recieved Date")]
        public DateTime? ReturnReceivedDate { get; set; }

        [Display(Name = "Return Recieved Date")]
        public DateTime? RefundCreatedDate { get; set; }

        [Display(Name = "Return Recieved Date")]
        public DateTime? CancellationRequestedDate { get; set; }
        [Display(Name = "Return Completed Date")]
        public DateTime? CancellationCompletedDate { get; set; }

        public List<OrderItem> OrderItems { get; set; }

        public OrderStatus OrderStatus { get; set; }
        [Display(Name = "Order Status")]
        public long OrderStatusId { get; set; }

        public long CustomerContactId { get; set; }
        public CustomerContact CustomerContact { get; set; }

        public long DeliveryTypeId { get; set; }
        [Display(Name = "Delivery Method")]
        public DeliveryType DeliveryType { get; set; }
        [Display(Name = "Package Reference")]
        public string PackageReference { get; set; }

        public string UserId { get; set; }

        public string PaymentId { get; set; }
        [Display(Name = "Payment Message")]
        public string PaymentMessage { get; set; }
        [Display(Name = "Payment Status")]
        public string PaymentStatus { get; set; }
        [Display(Name = "Stripe Seller Message")]
        public string SellerMessage { get; set; }
        public string StripeSource { get; set; }

        public string RefundId { get; set; }
        public string RefundMessage { get; set; }
        public string RefundStatus { get; set; }


    }
}
