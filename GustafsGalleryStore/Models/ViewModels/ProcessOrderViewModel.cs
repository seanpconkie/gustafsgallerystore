using System;
using System.Collections.Generic;
using GustafsGalleryStore.Areas.Identity.Data;
using GustafsGalleryStore.Models.DataModels;

namespace GustafsGalleryStore.Models.ViewModels
{
    public class ProcessOrderViewModel
    {

        public Order Order { get; set; }
        public ApplicationUser User { get; set; }
        public string StatusMessage { get; set; }
        public string FailureMessage { get; set; }
        public string SuccessMessage { get; set; }
        public decimal ReturnAmount { get; set; }
        public List<Return> Returns { get; set; }
        public List<OrderHistory> OrderHistories { get; set; }
    }
}
