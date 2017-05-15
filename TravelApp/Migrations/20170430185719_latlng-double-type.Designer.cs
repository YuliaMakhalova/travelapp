using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using TravelApp.Models;

namespace TravelApp.Migrations
{
    [DbContext(typeof(TravelAppContext))]
    [Migration("20170430185719_latlng-double-type")]
    partial class latlngdoubletype
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.1");

            modelBuilder.Entity("TravelApp.Models.Comment", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Content");

                    b.Property<DateTime>("DateAdd");

                    b.Property<int?>("EventId");

                    b.Property<int?>("LocationId");

                    b.Property<int?>("UserId");

                    b.HasKey("Id");

                    b.HasIndex("EventId");

                    b.HasIndex("LocationId");

                    b.HasIndex("UserId");

                    b.ToTable("Comments");
                });

            modelBuilder.Entity("TravelApp.Models.Event", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Address");

                    b.Property<DateTime>("DateAdd");

                    b.Property<DateTime>("DateUpdate");

                    b.Property<string>("Description");

                    b.Property<DateTime>("EventDate");

                    b.Property<string>("EventType");

                    b.Property<bool>("IsPublic");

                    b.Property<double>("Latitude");

                    b.Property<int>("LocationId");

                    b.Property<double>("Longitude");

                    b.Property<int>("MapZoom");

                    b.Property<int>("Position");

                    b.Property<string>("Title");

                    b.HasKey("Id");

                    b.HasIndex("LocationId");

                    b.ToTable("Events");
                });

            modelBuilder.Entity("TravelApp.Models.Location", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Address");

                    b.Property<DateTime>("ArrivalDate");

                    b.Property<string>("City");

                    b.Property<string>("Country");

                    b.Property<DateTime>("DateAdd");

                    b.Property<DateTime>("DateUpdate");

                    b.Property<string>("Description");

                    b.Property<bool>("IsPublic");

                    b.Property<double>("Latitude");

                    b.Property<double>("Longitude");

                    b.Property<int>("MapZoom");

                    b.Property<string>("PlaceTitle");

                    b.Property<int>("Position");

                    b.Property<string>("State");

                    b.Property<int>("TripId");

                    b.HasKey("Id");

                    b.HasIndex("TripId");

                    b.ToTable("Locations");
                });

            modelBuilder.Entity("TravelApp.Models.Photo", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int?>("EventId");

                    b.Property<bool>("IsBasic");

                    b.Property<int?>("LocationId");

                    b.Property<int>("TripId");

                    b.Property<string>("Url");

                    b.HasKey("Id");

                    b.HasIndex("EventId");

                    b.HasIndex("LocationId");

                    b.HasIndex("TripId");

                    b.ToTable("Photos");
                });

            modelBuilder.Entity("TravelApp.Models.Star", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("DateAdd");

                    b.Property<int?>("EventId");

                    b.Property<int?>("LocationId");

                    b.Property<int?>("TripId");

                    b.Property<int?>("UserId");

                    b.Property<int>("Value");

                    b.HasKey("Id");

                    b.HasIndex("EventId");

                    b.HasIndex("LocationId");

                    b.HasIndex("TripId");

                    b.HasIndex("UserId");

                    b.ToTable("Stars");
                });

            modelBuilder.Entity("TravelApp.Models.Trip", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("DateAdd");

                    b.Property<string>("Description");

                    b.Property<bool>("IsPublic");

                    b.Property<string>("Title");

                    b.Property<int>("UserId");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("Trips");
                });

            modelBuilder.Entity("TravelApp.Models.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("AvatarUrl")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("varchar(200)")
                        .HasDefaultValue("default.jpg");

                    b.Property<string>("Email")
                        .IsRequired();

                    b.Property<string>("Name")
                        .HasMaxLength(126);

                    b.Property<string>("Password");

                    b.Property<string>("Surname")
                        .HasMaxLength(126);

                    b.HasKey("Id");

                    b.HasAlternateKey("Email")
                        .HasName("AlternateKey_Email");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("TravelApp.Models.Comment", b =>
                {
                    b.HasOne("TravelApp.Models.Event", "Event")
                        .WithMany("Comments")
                        .HasForeignKey("EventId");

                    b.HasOne("TravelApp.Models.Location", "Location")
                        .WithMany("Comments")
                        .HasForeignKey("LocationId");

                    b.HasOne("TravelApp.Models.User", "User")
                        .WithMany("Comments")
                        .HasForeignKey("UserId");
                });

            modelBuilder.Entity("TravelApp.Models.Event", b =>
                {
                    b.HasOne("TravelApp.Models.Location", "Location")
                        .WithMany("Events")
                        .HasForeignKey("LocationId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("TravelApp.Models.Location", b =>
                {
                    b.HasOne("TravelApp.Models.Trip", "Trip")
                        .WithMany("Locations")
                        .HasForeignKey("TripId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("TravelApp.Models.Photo", b =>
                {
                    b.HasOne("TravelApp.Models.Event", "Event")
                        .WithMany("Photos")
                        .HasForeignKey("EventId");

                    b.HasOne("TravelApp.Models.Location", "Location")
                        .WithMany("Photos")
                        .HasForeignKey("LocationId");

                    b.HasOne("TravelApp.Models.Trip", "Trip")
                        .WithMany("Photos")
                        .HasForeignKey("TripId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("TravelApp.Models.Star", b =>
                {
                    b.HasOne("TravelApp.Models.Event", "Event")
                        .WithMany("Stars")
                        .HasForeignKey("EventId");

                    b.HasOne("TravelApp.Models.Location", "Location")
                        .WithMany("Stars")
                        .HasForeignKey("LocationId");

                    b.HasOne("TravelApp.Models.Trip", "Trip")
                        .WithMany("Stars")
                        .HasForeignKey("TripId");

                    b.HasOne("TravelApp.Models.User", "User")
                        .WithMany("Stars")
                        .HasForeignKey("UserId");
                });

            modelBuilder.Entity("TravelApp.Models.Trip", b =>
                {
                    b.HasOne("TravelApp.Models.User", "User")
                        .WithMany("Trips")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
        }
    }
}
