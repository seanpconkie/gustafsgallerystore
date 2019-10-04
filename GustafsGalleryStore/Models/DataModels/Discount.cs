using System;
using System.ComponentModel.DataAnnotations;

namespace GustafsGalleryStore.Models.DataModels
{
    public class Discount
    {
        [Display(Name = "Is the discount code live?")]
        public bool IsLive { get; set; }
        [Display(Name = "Does the discount code have a maximum number of uses?")]
        public bool HasMaxUse { get; set; }
        [Display(Name = "Does the discount code have a minimum spend?")]
        public bool HasMinValue { get; set; }


        [Display(Name = "End Date")]
        public DateTime? EndDate { get; set; }

        [Display(Name = "Start Date")]
        public DateTime? StartDate { get; set; }

        public decimal Percentage { get; set; }
        public decimal Value { get; set; }
        public decimal MinSpend { get; set; }

        public int MaxUsage { get; set; }
        public int UsageCount { get; set; }

        public long Id { get; set; }

        public string Code { get; set; }
    }
}
