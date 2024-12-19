using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using GestorDeProyectos.Models;
using GestorDeProyectos.Models.ViewModels;


namespace GestorDeProyectos.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Project> Projects { get; set; } = null!;
        public DbSet<ProjectUser> ProjectUsers { get; set; } = null!;
        public DbSet<ProjectLog> ProjectLogs { get; set; } = null!;
        public DbSet<StatusUpdate> StatusUpdates { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            
            builder.Entity<ProjectUser>()
                .HasKey(pu => new { pu.ProjectId, pu.UserId });

            builder.Entity<ProjectUser>()
                .HasOne(pu => pu.Project)
                .WithMany(p => p.ProjectUsers)
                .HasForeignKey(pu => pu.ProjectId);

            builder.Entity<ProjectUser>()
                .HasOne(pu => pu.User)
                .WithMany(u => u.AssignedProjects)
                .HasForeignKey(pu => pu.UserId);

            
            builder.Entity<Project>()
                .Property(p => p.TotalHours)
                .HasPrecision(10, 2); 

            
            builder.Entity<StatusUpdate>()
                .Property(s => s.HoursWorked)
                .HasPrecision(10, 2);
        }
    }
}
