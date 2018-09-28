using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using GustafsGalleryStore.Areas.Identity.Data;
using GustafsGalleryStore.Models.DataModels;
using GustafsGalleryStore.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
namespace GustafsGalleryStore.Models.DataModels
{
    public class ProductImage
    {

        public long Id { get; set; }
        public long ProductId { get; set; }
        [Required]
        public string Uri { get; set; }


        #region Public Methods
        public static List<SelectListItem> GetList(List<ProductImage> resultList)
        {

            List<SelectListItem> images = new List<SelectListItem>();

            foreach (var item in resultList)
            {
                images.Add(new SelectListItem { Value = item.Id.ToString(), Text = item.Uri.Replace("https://s3.amazonaws.com/sbt-solutions.imagestore/", "").Replace("?Authorization", "")});
            }

            return images;
        }
        #endregion
    }
}
