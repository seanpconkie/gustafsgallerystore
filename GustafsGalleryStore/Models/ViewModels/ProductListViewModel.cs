using System;
using System.Collections.Generic;
using GustafsGalleryStore.Models.DataModels;
namespace GustafsGalleryStore.Models.ViewModels
{
    public class ProductListViewModel
    {

        public List<Product> Products { get; set; }
        public string StatusMessage { get; set; }
        public string FailureMessage { get; set; }
        public string SuccessMessage { get; set; }
        public List<ProductBrand> Brands { get; set; }
        public List<Department> Departments { get; set; }
        public List<string> FilteredBrands { get; set; }
        public List<string> FilteredDepartments { get; set; }

    }
}
