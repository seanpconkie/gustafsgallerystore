using System.Collections.Generic;
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
using System;

namespace GustafsGalleryStore.Models.ViewModels
{
    public class DiscountViewModel
    {

        public string StatusMessage { get; set; }
        public string FailureMessage { get; set; }
        public string SuccessMessage { get; set; }

        public List<Discount> Discounts { get; set; }

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
        [Display(Name = "Percentage of order total (excludes delivery)")]
        public decimal Percentage { get; set; }
        [Display(Name = "Value of discount code (£)")]
        public decimal Value { get; set; }
        [Display(Name = "Minimum Spend (£)")]
        public decimal MinSpend { get; set; }

        [Display(Name = "Maxmimum number of uses")]
        public int MaxUsage { get; set; }
        [Display(Name = "Current number of uses")]
        public int UsageCount { get; set; }
        public long Id { get; set; }
        public string Code { get; set; }
    }
}
