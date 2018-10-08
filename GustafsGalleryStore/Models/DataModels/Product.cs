using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
namespace GustafsGalleryStore.Models.DataModels
{
    public class Product
    {
        //Product details
        public long Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public long Stock { get; set; }
        [DisplayFormat(DataFormatString = "{0:F2}")]
        public decimal Price { get; set; }
        [DisplayFormat(DataFormatString = "{0:F2}")]
        public decimal PostagePrice { get; set; }
        public string ProductCode { get; set; }
        public DateTime? CreateDate { get; set; }

        //brand
        public ProductBrand ProductBrand { get; set; }
        public long ProductBrandId { get; set; }
        //Size
        public List<ProductSize> ProductSizes { get; set; }
        //Colour
        public List<ProductColour> ProductColours { get; set; }
        //Image
        public List<ProductImage> ProductImages { get; set; }
        //Department
        public long DepartmentId { get; set; }
        public Department Department { get; set; }
    }
}
