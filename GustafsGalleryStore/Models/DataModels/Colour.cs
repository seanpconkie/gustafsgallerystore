using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
namespace GustafsGalleryStore.Models.DataModels
{
    public class Colour
    {

        public long Id { get; set; }

        [Required]
        [StringLength(250)]
        public string Value { get; set; }

        #region Public Methods
        public static List<SelectListItem> GetList(List<Colour> resultList)
        {

            List<SelectListItem> items = new List<SelectListItem>();

            foreach (var item in resultList)
            {
                items.Add(new SelectListItem { Value = item.Id.ToString(), Text = item.Value });
            }

            return items;
        }
        #endregion
    }
}
