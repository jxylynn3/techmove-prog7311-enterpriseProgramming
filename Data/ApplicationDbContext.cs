using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ST10448420_TechMove_GLMS.Models;

namespace ST10448420_TechMove_GLMS.Data
{
    public class ApplicationDbContext: IdentityDbContext<ApplicationUser>// Inherit from IdentityDbContext to include ASP.NET Core Identity tables
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Client> Clients { get; set; }
        public DbSet<Contract> Contracts { get; set; }
        public DbSet<ServiceRequest> ServiceRequests { get; set; }

        //adding the database sets for the state pattern later...its later now
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<Contract>().HasOne(c => c.Client)
                .WithMany(cl => cl.Contracts).HasForeignKey(c => c.ClientID)
                .OnDelete(DeleteBehavior.Restrict); // Prevent deleting clients with active contracts (the state pattern)

            builder.Entity<ServiceRequest>()
                .HasOne(s => s.Contract)
                .WithMany(c => c.ServiceRequests)
                .HasForeignKey(s => s.ContractID);

            builder.Entity<ServiceRequest>()
                .Property(s => s.CostUSD)
                .HasPrecision(18, 2);

            builder.Entity<ServiceRequest>()
                .Property(s => s.CostZAR)
                .HasPrecision(18, 2);

            builder.Entity<ApplicationUser>().HasOne(u => u.Client)
            .WithMany().HasForeignKey(u => u.ClientID)
            .OnDelete(DeleteBehavior.Restrict);// Prevent deleting clients with links to other users
        }
    }
}