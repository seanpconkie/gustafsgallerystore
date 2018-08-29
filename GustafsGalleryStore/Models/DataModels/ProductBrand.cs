using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
namespace GustafsGalleryStore.Models.DataModels
{
    public class ProductBrand
    {
        public long Id { get; set; }
        public string Brand { get; set; }
        public string BrandCode { get; set; }

        #region Public Methods
        public static List<SelectListItem> GetBrands(List<ProductBrand> resultList)
        {

            List<SelectListItem> brands = new List<SelectListItem>();

            foreach (var item in resultList)
            {
                brands.Add(new SelectListItem { Value = item.Id.ToString(), Text = item.Brand });
            }

            return brands;
        }
        #endregion
    }
}
