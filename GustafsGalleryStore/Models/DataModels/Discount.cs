using System;
namespace GustafsGalleryStore.Models.DataModels
{
    public class Discount
    {
        public bool IsLive { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? StartDate { get; set; }
        public decimal Percentage { get; set; }
        public decimal Value { get; set; }
        public long Id { get; set; }
        public string Code { get; set; }
    }
}
