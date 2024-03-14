using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WorkplaceCollaboration.Models;

namespace WorkplaceCollaboration.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Channel> Channels { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Invite> Invites { get; set; }
        public DbSet<ChannelUser> ChannelUsers { get; set; }
        public DbSet<ChannelMod> ChannelMods { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // definire primary key compus
            modelBuilder.Entity<Invite>()
            .HasKey(ab => new {
                ab.Id,
                ab.UserId,
                ab.ChannelId
            });
            // definire relatii cu modelele User si Channel (FK)
            modelBuilder.Entity<Invite>()
            .HasOne(ab => ab.User)
            .WithMany(ab => ab.Invites)
            .HasForeignKey(ab => ab.UserId);
            modelBuilder.Entity<Invite>()
            .HasOne(ab => ab.Channel)
            .WithMany(ab => ab.Invites)
            .HasForeignKey(ab => ab.ChannelId);

            // definire primary key compus
            modelBuilder.Entity<ChannelMod>()
            .HasKey(ab => new {
                ab.Id,
                ab.UserId,
                ab.ChannelId
            });
            // definire relatii cu modelele Mod si Channel (FK)
            modelBuilder.Entity<ChannelMod>()
            .HasOne(ab => ab.User)
            .WithMany(ab => ab.ChannelMods)
            .HasForeignKey(ab => ab.UserId);
            modelBuilder.Entity<ChannelMod>()
            .HasOne(ab => ab.Channel)
            .WithMany(ab => ab.ChannelMods)
            .HasForeignKey(ab => ab.ChannelId);

            modelBuilder.Entity<ChannelUser>()
            .HasKey(ab => new {
                ab.Id,
                ab.UserId,
                ab.ChannelId
            });

            modelBuilder.Entity<ChannelUser>()
            .HasOne(ab => ab.User)
            .WithMany(ab => ab.ChannelUsers)
            .HasForeignKey(ab => ab.UserId);
            modelBuilder.Entity<ChannelUser>()
            .HasOne(ab => ab.Channel)
            .WithMany(ab => ab.ChannelUsers)
            .HasForeignKey(ab => ab.ChannelId);

            modelBuilder.Entity<Channel>()
            .HasOne<User>(a => a.Creator)
            .WithMany(c => c.ChannelsCreated)
            .HasForeignKey(a => a.CreatorId);
        }
    }
}