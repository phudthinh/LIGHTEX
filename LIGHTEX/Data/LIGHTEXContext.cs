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
    }
}
