using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace VBSPOSS.Data
{
    public class IntellectIDCDbContext : DbContext
    {
        public IntellectIDCDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
       

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            
        }
    }
}

