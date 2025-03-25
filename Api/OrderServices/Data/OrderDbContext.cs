using Microsoft.EntityFrameworkCore;
using OrderServices.Data.Models;
using System.Collections.Generic;

namespace OrderServices.Data
{

        public class OrderDbContext : DbContext
        {
            public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options) { }

            public DbSet<Order> Orders { get; set; }
        }
    
}
