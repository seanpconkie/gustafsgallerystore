using System;
namespace GustafsGalleryStore.Models.DataModels
{
    public class OrderItemDTO
    {
        public long Id { get; set; }
        public long OrderId { get; set; }
        public long ProductId { get; set; }
        public string Size { get; set; }
        public string Colour { get; set; }
        public int Quantity { get; set; }
    }
}
