using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VBSPOSS.Data.Models;
using VBSPOSS.Models;

namespace VBSPOSS.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public virtual DbSet<ListOfValue> ListOfValues { get; set; }
        public virtual DbSet<ListOfPos> ListOfPoss { get; set; }
        public virtual DbSet<CellValue> CellValues { get; set; }
        public virtual DbSet<PosRepresentative> PosRepresentatives { get; set; }
        public virtual DbSet<Menu> Menus { get; set; }//
        public virtual DbSet<User> Users { get; set; }

        public virtual DbSet<UserView> UserViews { get; set; }
        public virtual DbSet<Role> Roles { get; set; }
        public virtual DbSet<MenuRole> MenuRoles { get; set; }
        public virtual DbSet<MenuRoleView> MenuRoleViews { get; set; }
        public virtual DbSet<Permission> Permissions { get; set; }
        public virtual DbSet<RolePermission> RolePermissions { get; set; }
        public virtual DbSet<ListOfCommune> ListOfCommunes { get; set; }

        public DbSet<TideTermWorking> TideTermWorkings { get; set; }

        public DbSet<InterestRatePosApply> InterestRatePosApplys { get; set; }

        public virtual DbSet<StaffView> StaffViews { get; set; }
        public virtual DbSet<NotiTemp> NotiTemps { get; set; }//

        public virtual DbSet<AttachedFileInfo> AttachedFileInfos { get; set; }
        public virtual DbSet<InterestRateConfigMaster> InterestRateConfigMasters { get; set; }
        public virtual DbSet<InterestRateTermDetail> InterestRateTermDetails { get; set; }

        public virtual DbSet<InterestRateConfigMasterView> InterestRateConfigMasterViews { get; set; }

        // Thêm DbSet cho ListOfProducts
        public DbSet<ListOfProducts> ListOfProducts { get; set; }

        // Cấu hình OnModelCreating nếu cần
        //add
        public virtual DbSet<ProductParameter> ProductParameters { get; set; }

        public DbSet<ProductParametersView> ProductParametersViews { get; set; }

        public virtual DbSet<UserManagementIDC> UserManagementIDCs { get; set; }

        public virtual DbSet<UserIDCMaster> UserIDCMasters { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserManagementIDC>().ToTable("UserManagementIDC");
            modelBuilder.Entity<UserManagementIDC>().HasKey(x => new { x.Id });

            modelBuilder.Entity<UserIDCMaster>().ToTable("UserIDCMaster");
            modelBuilder.Entity<UserIDCMaster>().HasKey(x => new { x.Id, x.UserId, x.NickName });

            modelBuilder.Entity<ListOfValue>().ToTable("ListOfValue");
            modelBuilder.Entity<ListOfValue>().HasKey(x => new { x.Id });

            modelBuilder.Entity<ListOfPos>().ToTable("ListOfPos");
            modelBuilder.Entity<ListOfPos>().HasKey(x => new { x.Code });

            modelBuilder.Entity<Menu>().ToTable("Menu");
            modelBuilder.Entity<Menu>().HasKey(x => new { x.Id });

            modelBuilder.Entity<NotiTemp>().ToTable("NotiTemp");
            modelBuilder.Entity<NotiTemp>().HasKey(x => new { x.Id });

            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<User>().HasKey(x => new { x.Id });

            modelBuilder.Entity<MenuRole>().ToTable("MenuRole");
            modelBuilder.Entity<MenuRole>().HasKey(x => new { x.Id });

            modelBuilder.Entity<Role>().ToTable("Role");
            modelBuilder.Entity<Role>().HasKey(x => new { x.Id });
            
            modelBuilder.Entity<ListOfCommune>().ToTable("ListOfCommune");
            modelBuilder.Entity<ListOfCommune>().HasKey(x => new { x.ProvinceCode, x.DistrictCode, x.CommuneCode, x.SubCommuneCode });

            modelBuilder.Entity<StaffView>().ToTable("vStaff");
            modelBuilder.Entity<StaffView>().HasKey(x => new { x.Id });

            modelBuilder.Entity<StaffView>().ToTable("vStaff");
            modelBuilder.Entity<StaffView>().HasKey(x => new { x.Id });

            modelBuilder.Entity<AttachedFileInfo>().ToTable("AttachedFileInfo");
            modelBuilder.Entity<AttachedFileInfo>().HasKey(x => new { x.FileId });

            modelBuilder.Entity<InterestRateConfigMaster>().ToTable("InterestRateConfigMaster");
            modelBuilder.Entity<InterestRateConfigMaster>().HasKey(x => new { x.Id });
            //add Index
            modelBuilder.Entity<InterestRateConfigMasterView>(eb =>
            {
                eb.HasNoKey();
                eb.ToView("InterestRateConfigMasterView");
            });

            //add Index ChLS
            modelBuilder.Entity<ProductParametersView>(entity =>
            {
                entity.HasNoKey();  
                entity.ToView("ProductParametersView");  
                                                         
                                                         
            });

            modelBuilder.Entity<InterestRateTermDetail>().ToTable("InterestRateTermDetail");
            modelBuilder.Entity<InterestRateTermDetail>().HasKey(x => new { x.Id });

            modelBuilder.Entity<MenuRoleView>(eb =>
            {
                eb.HasNoKey();
                eb.ToView("vMenuRole");
            });

            modelBuilder.Entity<UserView>(eb =>
            {
                eb.HasNoKey();
                eb.ToView("vUsers");
            });
            modelBuilder.Entity<InterestRateConfigMasterView>(eb =>
            {
                eb.HasNoKey(); 
                eb.ToView("InterestRateConfigMasterView");
            });

            modelBuilder.Entity<Permission>().ToTable("Permission");
            modelBuilder.Entity<Permission>().HasKey(x => new { x.Id });

            modelBuilder.Entity<RolePermission>().ToTable("RolePermission");
            modelBuilder.Entity<RolePermission>().HasKey(x => new { x.Id });

            modelBuilder.Entity<CellValue>(eb =>
            {
                eb.HasNoKey();
                eb.ToView("CellValues");
                eb.Property(v => v.Code).HasColumnName("Code");
            });

            // Cấu hình thêm cho ListOfProducts nếu cần (ví dụ: khóa chính, mối quan hệ, v.v.)
            modelBuilder.Entity<ListOfProducts>().HasKey(p => p.Id);
            modelBuilder.Entity<ListOfProducts>().Property(p => p.Status).HasDefaultValue(1);

            modelBuilder.Entity<TideTermWorking>().ToTable("TideTermWorking");


            modelBuilder.Entity<ProductParameter>(entity =>
            {
                entity.ToTable("ProductParameters", "dbo");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("Id").ValueGeneratedOnAdd();
                entity.Property(e => e.ProductGroupCode).HasMaxLength(32).IsRequired().HasColumnType("varchar(32)");
                entity.Property(e => e.ProductCode).HasMaxLength(8).IsRequired().HasColumnType("varchar(8)");
                entity.Property(e => e.ProductName).HasMaxLength(256).HasColumnType("nvarchar(256)");
                entity.Property(e => e.ApplyPosFlag).IsRequired();
                entity.Property(e => e.MinInterestRateSpread).HasPrecision(18, 8).IsRequired();
                entity.Property(e => e.MaxInterestRateSpread).HasPrecision(18, 8).IsRequired();
                entity.Property(e => e.EffectedDate).HasColumnType("date").IsRequired();
                entity.Property(e => e.Status).IsRequired();
                entity.Property(e => e.Remark).HasMaxLength(1024).HasColumnType("nvarchar(1024)");
                entity.Property(e => e.CreatedBy).HasMaxLength(50).HasColumnType("nvarchar(50)");
                entity.Property(e => e.ModifiedBy).HasMaxLength(50).HasColumnType("nvarchar(50)");
                entity.Property(e => e.ApproverBy).HasMaxLength(50).HasColumnType("nvarchar(50)");

                
                entity.HasIndex(e => new { e.ProductGroupCode, e.ProductCode, e.EffectedDate });
            });


            modelBuilder.Entity<InterestRatePosApply>(entity =>
            {
                entity.ToTable("InterestRatePosApply");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                      .ValueGeneratedOnAdd();   // ⬅ auto IDENTITY

                entity.Property(e => e.IntRateConfigId).IsRequired();

                entity.Property(e => e.PosCode)
                      .HasMaxLength(6)
                      .IsRequired();

                entity.Property(e => e.CreatedBy)
                      .HasMaxLength(50);

                entity.Property(e => e.CreatedDate)
                      .HasColumnType("date");
            });

            modelBuilder.Entity<PosRepresentative>().ToTable("PosRepresentative");
            modelBuilder.Entity<PosRepresentative>().HasKey(x => new { x.Id });
        }
    }
}

