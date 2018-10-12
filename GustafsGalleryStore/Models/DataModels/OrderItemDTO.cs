using System;
namespace GustafsGalleryStore.Models.DataModels
{
    public class OrderItemDTO
    {
        public long OrderId { get; set; }
        public long ProductId { get; set; }
        public string Size { get; set; }
        public string Colour { get; set; }
    }
}
