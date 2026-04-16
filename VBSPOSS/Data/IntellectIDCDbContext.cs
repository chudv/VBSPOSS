//using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VBSPOSS.Data.IntellectIDC.Models;

namespace VBSPOSS.Data
{
    public class IntellectIDCDbContext : DbContext
    {
        public virtual DbSet<CellValue> CellValues { get; set; }
        public virtual DbSet<QueryResult> QueryResults { get; set; }
        
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
            });
            modelBuilder.Entity<QueryResult>(eb =>
            {
                eb.HasNoKey();
                eb.ToView("QueryResults");
            });

        }
    }
}

