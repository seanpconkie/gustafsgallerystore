using System;
using System.ComponentModel.DataAnnotations;
namespace GustafsGalleryStore
{
    public class DeliveryCompany
    {
        public long Id { get; set; }
        [Display(Name = "Delivery Provider Name")]
        public string Company { get; set; }
    }
}
