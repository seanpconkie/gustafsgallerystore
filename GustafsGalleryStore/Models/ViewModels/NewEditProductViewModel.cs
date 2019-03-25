using System;
using System.Collections.Generic;
using GustafsGalleryStore.Models.DataModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;

namespace GustafsGalleryStore.Models.ViewModels
{
    public class NewEditProductViewModel
    {
        [Required]
        public Product Product { get; set; }
        public string ReturnUrl { get; set; }
        public string StatusMessage { get; set; }
        public string FailureMessage { get; set; }
        public string SuccessMessage { get; set; }

        [Required]
        public List<string> Colour { get; set; }
        [Required]
        public List<string> Size { get; set; }
        public List<string> Image { get; set; }
        [Required]
        public string Brand { get; set; }
        [Required]
        public string Department { get; set; }

        public List<IFormFile> ImageFiles { get; set; }
        //lists of possible brands/colours/departments/sizes
        public List<SelectListItem> Brands { get; set; }
        public List<SelectListItem> Colours { get; set; }
        public List<SelectListItem> Departments { get; set; }
        public List<SelectListItem> Sizes { get; set; }
        //used for edit to store colours/sizes already assigned to product
        public List<SelectListItem> ProductColours { get; set; }
        public List<SelectListItem> ProductSizes { get; set; }
        public List<SelectListItem> ProductImages { get; set; }

        // used for preview
        public List<Product> RelatedProducts { get; set; }
        public List<ProductImage> ProductImagePreview { get; set; }

    }
}