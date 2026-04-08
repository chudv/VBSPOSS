using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VBSPOSS.Data.IntellectIDC.Models;

namespace VBSPOSS.Data
{
    public class IntellectIDCDbContext : DbContext
    {
        public virtual DbSet<CellValue> CellValues { get; set; }

        public IntellectIDCDbContext(DbContextOptions<IntellectIDCDbContext> options)
            : base(options)
        {
        }
       

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<CellValue>(eb =>
            {
                eb.HasNoKey();
                eb.ToView("CellValues");
                eb.Property(v => v.Code).HasColumnName("Code");
            });
        }
    }
}

