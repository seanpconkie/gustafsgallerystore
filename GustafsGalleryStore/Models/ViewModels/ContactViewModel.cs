using System;
using System.ComponentModel.DataAnnotations;

namespace GustafsGalleryStore.Models.ViewModels
{
    public class ContactViewModel
    {
        public string Email { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public string Name { get; set; }

        public string StatusMessage { get; set; }
        public string FailureMessage { get; set; }
        public string SuccessMessage { get; set; }
    }
}
