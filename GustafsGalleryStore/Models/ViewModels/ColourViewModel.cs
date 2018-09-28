﻿using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using GustafsGalleryStore.Areas.Identity.Data;
using GustafsGalleryStore.Models.DataModels;
using GustafsGalleryStore.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
namespace GustafsGalleryStore.Models.ViewModels
{
    public class ColourViewModel
    {

        public string StatusMessage { get; set; }

        public List<Colour> Colours { get; set; }

        //new brand
        [Required]
        public string Colour { get; set; }

    }
}
