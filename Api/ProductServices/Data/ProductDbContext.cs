using Microsoft.EntityFrameworkCore;
using ProductServices.Data.Models;
using System.Collections.Generic;

namespace ProductServices.Data
{
    public class ProductDbContext : DbContext
    {

        /// <summary>
        /// Constructor DataBase context
        /// </summary>
        /// <param name="options"></param>
        public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options) { }

        public DbSet<Product> Products { get; set; }
    }
}
