using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
namespace GustafsGalleryStore.Models.DataModels
{
    public class ProductColour
    {

        public long Id { get; set; }
        public string Colour { get; set; }


        #region Public Methods
        public static List<SelectListItem> GetColours(List<ProductColour> resultList)
        {

            List<SelectListItem> colours = new List<SelectListItem>();

            foreach (var item in resultList)
            {
                colours.Add(new SelectListItem { Value = item.Id.ToString(), Text = item.Colour });
            }

            return colours;
        }
        #endregion
    }
}
