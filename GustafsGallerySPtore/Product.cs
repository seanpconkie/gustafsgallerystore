using System;
using System.Collections.Generic;
namespace GustafsGalleryStore
{
    public class Product
    {
        //Product details
        public long Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public long Stock { get; set; }
        public float Price { get; set; }
        public float PostagePrice { get; set; }

        //brand
        public ProductBrand ProductBrand { get; set; }
        public long BrandId { get; set; }
        //Size
        public List<ProductSize> ProductSizes { get; set; }
        //Colour
        public List<ProductColour> ProductColours { get; set; }

    }
}
