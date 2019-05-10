using System;
using System.Collections.Generic;
using GustafsGalleryStore.Models.DataModels;
namespace GustafsGalleryStore.Models.ViewModels
{
    public class ProductListViewModel
    {

        public List<Product> Products { get; set; }
        public string ReturnUrl { get; set; }
        public string StatusMessage { get; set; }
        public string FailureMessage { get; set; }
        public string SuccessMessage { get; set; }
        public List<ProductBrand> Brands { get; set; }
        public List<Department> Departments { get; set; }
        public List<string> FilteredBrands { get; set; }
        public List<string> FilteredDepartments { get; set; }

        public int PageItems { get; set; }
        public int PageNumber { get; set; }
        public int MaxPages { get; set; }
        public string OrderBy { get; set; }
        public string OrderByModifier { get; set; }
        public string PreviousUrl { get; set; }
        public string NextUrl { get; set; }

        public string ItemCountUrl1 { get; set; }
        public string ItemCountUrl2 { get; set; }
        public string ItemCountUrl3 { get; set; }

        public string CreateDateUrl { get; set; }
        public string BrandUrl { get; set; }
        public string DepartmentUrl { get; set; }
        public string PriceUrl { get; set; }
        public string TitleUrl { get; set; }

        public List<string> PageURLs { get; set; }

    }
}
