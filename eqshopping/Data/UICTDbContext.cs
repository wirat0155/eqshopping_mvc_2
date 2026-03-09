using eqshopping.Models.DbView;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eqshopping.Data
{
    public class UICTDbContext : DbContext
    {
        public UICTDbContext(DbContextOptions<UICTDbContext> options) : base(options)
        {
        }


        public DbSet<vw_emp_general> vw_emp_general { get; set; }
    }
}
