using System;
namespace GustafsGalleryStore
{
    public class DeliveryType
    {
        public long Id { get; set; }
        public string Type { get; set; }
        public decimal Price { get; set; }
        public string Time { get; set; }
        //Provider
        public long DeliveryCompanyId { get; set; }
        public DeliveryCompany DeliveryCompany { get; set; }
    }
}
