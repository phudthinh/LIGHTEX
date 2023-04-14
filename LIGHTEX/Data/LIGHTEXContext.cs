using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using LIGHTEX.Models;
using LIGHTEX.Models.Domain;

namespace LIGHTEX.Data
{
    public class LIGHTEXContext : DbContext
    {
        public LIGHTEXContext (DbContextOptions<LIGHTEXContext> options)
            : base(options)
        {
        }

        public DbSet<Account> Account { get; set; }
        public DbSet<Customer> Customer { get; set; }
        public DbSet<Category> Category { get; set; }
        public DbSet<Brand> Brand { get; set; }
        public DbSet<Product> Product { get; set; }
        public DbSet<Comment> Comment { get; set; }
        public DbSet<Cart> Cart { get; set; }
        public DbSet<Favorite> Favorite { get; set; }
        public DbSet<Bill> Bill { get; set; }
    }
}
