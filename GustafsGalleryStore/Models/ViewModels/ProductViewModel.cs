using System;
using System.Collections.Generic;
using GustafsGalleryStore.Models.DataModels;
using Microsoft.AspNetCore.Mvc.Rendering;
namespace GustafsGalleryStore.Models.ViewModels
{
    public class ProductViewModel
    {
        public Product Product { get; set; }
        public string ReturnUrl { get; set; }
        public string StatusMessage { get; set; }
        public string FailureMessage { get; set; }
        public string SuccessMessage { get; set; }

        public string Colour { get; set; }
        public string Size { get; set; }

        public List<SelectListItem> Colours { get; set; }
        public List<SelectListItem> Sizes { get; set; }

    }
}
