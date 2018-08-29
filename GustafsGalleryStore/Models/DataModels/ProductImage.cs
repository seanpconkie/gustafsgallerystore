using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using GustafsGalleryStore.Areas.Identity.Data;
using GustafsGalleryStore.Models.DataModels;
using GustafsGalleryStore.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
namespace GustafsGalleryStore.Models.DataModels
{
    public class ProductImage
    {

        public long Id { get; set; }
        public long ProductId { get; set; }
        public string Uri { get; set; }

    }
}
