using System;
namespace GustafsGalleryStore.Models.DataModels
{
    public class OrderItem
    {
        public long Id { get; set; }
        public long OrderId { get; set; }
        public long Quantity { get; set; }

        public long ProductId { get; set; }
        public Product Product { get; set; }

        public long SizeId { get; set; }

        public long ColourId { get; set; }

    }
}
