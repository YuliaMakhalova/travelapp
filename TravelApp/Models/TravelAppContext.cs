using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TravelApp.Models
{
    public class TravelAppContext : DbContext
    {
        public TravelAppContext(DbContextOptions<TravelAppContext> options)
            :base(options)
        {}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasAlternateKey(u => u.Email)
                .HasName("AlternateKey_Email");

            modelBuilder.Entity<User>()
                .Property(u => u.AvatarUrl)
                .HasColumnType("varchar(200)");

            modelBuilder.Entity<User>()
                .Property(u => u.AvatarUrl)
                .HasDefaultValue("default.jpg");
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Trip> Trips { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Photo> Photos { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Star> Stars { get; set; }
    }
}
