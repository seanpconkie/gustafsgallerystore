﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
namespace GustafsGalleryStore.Models.DataModels
{
    public class ProductColour
    {
        public long Id { get; set; }
        public string Colour { get; set; }
        public long ProductId { get; set; }


        #region Public Methods
        public static List<SelectListItem> GetList(List<ProductColour> resultList)
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
