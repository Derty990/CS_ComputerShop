using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Firma.Intranet.Models.CMS;
using Firma.Intranet.Models.Shop;
using Firma.Intranet.Models.Customers;

namespace Firma.Intranet.Data
{
    public class FirmaIntranetContext : DbContext
    {
        public FirmaIntranetContext (DbContextOptions<FirmaIntranetContext> options)
            : base(options)
        {
        }

        public DbSet<Firma.Intranet.Models.CMS.Topicality> Topicality { get; set; } = default!;
        public DbSet<Firma.Intranet.Models.Shop.Cart> Cart { get; set; } = default!;
        public DbSet<Firma.Intranet.Models.Customers.Order> Order { get; set; } = default!;
        public DbSet<Firma.Intranet.Models.Customers.OrderItem> OrderItem { get; set; } = default!;
        public DbSet<Firma.Intranet.Models.CMS.Page> Page { get; set; } = default!;
        public DbSet<Firma.Intranet.Models.Shop.Product> Product { get; set; } = default!;
        public DbSet<Firma.Intranet.Models.Shop.ProductType> ProductType { get; set; } = default!;
        public DbSet<Firma.Intranet.Models.Customers.User> User { get; set; } = default!;
    }
}
