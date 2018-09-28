using System;
using System.Collections.Generic;
using GustafsGalleryStore.Models.DataModels;
namespace GustafsGalleryStore.Models.ViewModels
{
    public class ProductListViewModel
    {

        public List<Product> Products { get; set; }
        public string StatusMessage { get; set; }
    }
}
