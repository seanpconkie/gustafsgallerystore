using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using GustafsGalleryStore.Models.DataModels;

namespace GustafsGalleryStore.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public DbSet<CustomerTitle> Titles { get; set; }
        public DbSet<CustomerContact> CustomerContacts { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<OrderHistory> OrderHistories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductBrand> ProductBrands { get; set; }
        public DbSet<ProductColour> ProductColours { get; set; }
        public DbSet<ProductSize> ProductSizes { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
    }
}
