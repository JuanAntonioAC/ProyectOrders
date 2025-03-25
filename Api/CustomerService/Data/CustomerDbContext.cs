using CustomerService.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace CustomerService.Data
{
    public class CustomerDbContext :DbContext
    {

        /// <summary>
        /// Constructor DataBase
        /// </summary>
        /// <param name="options"></param>
        public CustomerDbContext(DbContextOptions<CustomerDbContext> options) : base(options) { }

        public DbSet<Customer> Customers { get; set; }
    }
}
