using System;
using System.Collections.Generic;
using GustafsGalleryStore.Areas.Identity.Data;
namespace GustafsGalleryStore.Models.ViewModels
{
    public class StaffListViewModel
    {
        public IList<ApplicationUser> Staff { get; set; }
        public string StatusMessage { get; set; }
        public string FailureMessage { get; set; }
        public string SuccessMessage { get; set; }
    }
}
