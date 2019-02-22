using System;
using System.ComponentModel.DataAnnotations;

namespace GustafsGalleryStore.Models.DataModels
{
    public class Discount
    {
        [Display(Name = "Discount Code Live?")]
        public bool IsLive { get; set; }
        [Display(Name = "End Date")]
        public DateTime? EndDate { get; set; }
        [Display(Name = "Start Date")]
        public DateTime? StartDate { get; set; }
        public decimal Percentage { get; set; }
        public decimal Value { get; set; }
        public long Id { get; set; }
        public string Code { get; set; }
    }
}
