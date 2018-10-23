using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GustafsGalleryStore.Models.DataModels
{
    public class Return
    {

        public long Id { get; set; }
        public long OrderId { get; set; }

        public DateTime? ReturnOpenedDate { get; set; }
        [Display(Name = "Return Complete Date")]
        public DateTime? ReturnCompleteDate { get; set; }
        [Display(Name = "Refund Issued Date")]
        public DateTime? RefundCreatedDate { get; set; }

        public string RefundId { get; set; }
        [Display(Name = "Refund Message")]
        public string RefundMessage { get; set; }
        [Display(Name = "Refund Status")]
        public string RefundStatus { get; set; }

        public string Reason { get; set; }

        public List<ReturnItem> ReturnItems { get; set; }
        [NotMapped]
        public List<Product> Products { get; set; }
    }
}
