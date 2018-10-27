using System;
using System.Collections.Generic;
using GustafsGalleryStore.Models.DataModels;

namespace GustafsGalleryStore.Models.ViewModels
{
    public class ManageOrdersViewModel
    {

        public List<Order> NewOrders { get; set; }
        public List<Order> AwaitingStock { get; set; }
        public List<Order> OpenReturns { get; set; }
        public List<Order> CancelledOrders { get; set; }
        public List<Order> CompletedOrders { get; set; }
        public string StatusMessage { get; set; }
        public string FailureMessage { get; set; }
        public string SuccessMessage { get; set; }

    }
}
