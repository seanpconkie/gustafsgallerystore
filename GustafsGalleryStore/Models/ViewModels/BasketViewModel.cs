using System;
using GustafsGalleryStore.Models.DataModels;

namespace GustafsGalleryStore.Models.ViewModels
{
    public class BasketViewModel
    {

        public Order Basket { get; set; }
        public string StatusMessage { get; set; }
        public string FailureMessage { get; set; }
        public string SuccessMessage { get; set; }

    }
}
