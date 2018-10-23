using System;
using System.Collections.Generic;
using GustafsGalleryStore.Models.DataModels;

namespace GustafsGalleryStore.Models.ViewModels
{
    public class ReturnOrderViewModel
    {
        public Order Order { get; set; }
        public Return Return { get; set; }
        public List<long> ReturnItems { get; set; }
    }
}
