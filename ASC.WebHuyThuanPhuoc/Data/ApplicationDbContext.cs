using ASC.Model.Models;
using System.Reflection.Emit;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ASC.WebHuyThuanPhuoc.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public virtual DbSet<MasterDataKey> MasterDataKeys { get; set; }
        public virtual DbSet<MasterDataValue> MasterDataValues { get; set; }
        public virtual DbSet<ServiceRequest> ServiceRequests { get; set; }
        public virtual DbSet<Product> Products { get; set; }
        public DbSet<ServiceRequestMessage> ServiceRequestMessages { get; set; }
        public DbSet<OnlineUser> OnlineUsers { get; set; }
        public DbSet<Promotion> Promotions { get; set; }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            Database.Migrate();
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<MasterDataKey>()
                .HasKey(c => new { c.PartitionKey, c.RowKey });
            builder.Entity<MasterDataValue>()
                .HasKey(c => new { c.PartitionKey, c.RowKey });
            builder.Entity<ServiceRequest>()
                .HasKey(c => new { c.PartitionKey, c.RowKey });
            base.OnModelCreating(builder);
            builder.Entity<ServiceRequestMessage>()
                .HasKey(x => new { x.PartitionKey, x.RowKey });

            builder.Entity<OnlineUser>()
                .HasKey(x => new { x.PartitionKey, x.RowKey });

            builder.Entity<Promotion>()
                .HasKey(x => new { x.PartitionKey, x.RowKey });
        }
    }
}
