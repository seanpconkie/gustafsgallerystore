using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
namespace GustafsGalleryStore.Models.DataModels
{
    public class ProductSize
    {
        public long Id { get; set; }
        public string Size { get; set; }


        #region Public Methods
        public static List<SelectListItem> GetSizes(List<ProductSize> resultList)
        {

            List<SelectListItem> sizes = new List<SelectListItem>();

            foreach (var item in resultList)
            {
                sizes.Add(new SelectListItem { Value = item.Id.ToString(), Text = item.Size });
            }

            return sizes;
        }
        #endregion
    }
}
