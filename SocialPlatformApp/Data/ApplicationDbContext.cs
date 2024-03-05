using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SocialPlatformApp.Models;

namespace SocialPlatformApp.Data
{   //PASUL 3 - USERI SI ROLURI
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Comment> Comments { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Profile> Profiles { get; set; }
        public DbSet<Request> Requests { get; set; }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }

        public DbSet<UserGroup> UserGroups { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // definirea relatiei many-to-many 

            base.OnModelCreating(modelBuilder);

            // definire primary key compus
            modelBuilder.Entity<UserGroup>()
                .HasKey(ab => new { ab.Id, ab.UserId, ab.GroupId });


            // definire relatii cu modelele Group si User (FK)

            modelBuilder.Entity<UserGroup>()
                .HasOne(ab => ab.User)
                .WithMany(ab => ab.UserGroups)
                .HasForeignKey(ab => ab.UserId);

            modelBuilder.Entity<UserGroup>()
                .HasOne(ab => ab.Group)
                .WithMany(ab => ab.UserGroups)
                .HasForeignKey(ab => ab.GroupId);

            // 1 to 1 user -> profile

            modelBuilder.Entity<ApplicationUser>()
           .HasOne(a => a.Profile)
           .WithOne(p => p.User)
           .HasForeignKey<Profile>(p => p.UserId);
        }
    }

}