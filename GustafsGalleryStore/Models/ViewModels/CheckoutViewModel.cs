using System;
using System.Collections.Generic;
using GustafsGalleryStore.Models.DataModels;

namespace GustafsGalleryStore.Models.ViewModels
{
    public class CheckoutViewModel
    {

        public Order Basket { get; set; }
        public List<CustomerContact> Contacts { get; set; }
        public List<DeliveryType> DeliveryTypes { get; set; }
        public string StripeToken { get; set; }
        public string StatusMessage { get; set; }
        public string ThreeDSecure { get; set; }

    }
}
