using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Firma.Data.Data
{
    public class FirmaContext : DbContext
    {
        public FirmaContext(DbContextOptions<FirmaContext> options) 
            : base(options)
        {
        }

        public DbSet<CMS.Topicality> Topicality { get; set; } = default!;
        public DbSet<Shop.Cart> Cart { get; set; } = default!;
        public DbSet<Customers.Order> Order { get; set; } = default!;
        public DbSet<Customers.OrderItem> OrderItem { get; set; } = default!;
        public DbSet<CMS.Page> Page { get; set; } = default!;
        public DbSet<Shop.Product> Product { get; set; } = default!;
        public DbSet<Shop.ProductType> ProductType { get; set; } = default!;
        public DbSet<Customers.User> User { get; set; } = default!;
    }
}
