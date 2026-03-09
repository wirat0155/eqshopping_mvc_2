using eqshopping.Models.DbModel;
using eqshopping.Models.DbView;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eqshopping.Data
{
    public class UICT2EQSDbContext : DbContext
    {
        public UICT2EQSDbContext(DbContextOptions<UICT2EQSDbContext> options) : base(options)
        {
            
        }

        public DbSet<pd_plant> pd_plant { get; set; }
        public DbSet<vw_uict_vw_emp> vw_uict_vw_emp { get; set; }
        public DbSet<eqs_usermanagement> eqs_usermanagement { get; set; }
        public DbSet<pd_cell> pd_cell { get; set; }
        public DbSet<eqs_celllock> eqs_celllock { get; set; }
        public DbSet<eqs_shoppingtran> eqs_shoppingtran { get; set; }
        public DbSet<eqs_shoppingtransub> eqs_shoppingtransub { get; set; }
        public DbSet<pd_cellproduct> pd_cellproduct { get; set; }
        public DbSet<eqs_productequipment> eqs_productequipment { get; set; }
        public DbSet<eqs_equipment> eqs_equipment { get; set; }
        public DbSet<sys_log> sys_log { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<vw_uict_vw_emp>(entity =>
            {
                entity.HasNoKey();
            });
        }
    }
}
