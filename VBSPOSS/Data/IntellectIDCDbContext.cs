//using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VBSPOSS.Data.IntellectIDC.Models;
using VBSPOSS.Models.IDC;

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

        public virtual DbSet<ChangePosDataChecking> ChangePosDataCheckings { get; set; }

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

            modelBuilder.Entity<ChangePosDataChecking>(
                entity =>
                {
                    entity.HasNoKey();
                    entity.ToTable(
                        "CHANGE_POS_DATA_CHECKING",
                        "IDL_LMS");
                });


        }
    }
}

