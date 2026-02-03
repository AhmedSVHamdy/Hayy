using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Project.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Project.Infrastructure.ApplicationDbContext;

public partial class HayyContext : DbContext
{
    public HayyContext()
    {
    }

    public HayyContext(DbContextOptions<HayyContext> options): base(options)
    {

    }
protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        OnModelCreatingPartial(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
   

    // Users & Settings
    public DbSet<User> Users { get; set; }
    public DbSet<UserSettings> UserSettings { get; set; }
    public DbSet<Admin> Admins { get; set; }
    public DbSet<AdminAction> AdminActions { get; set; }

    // Business & Plans
    public DbSet<Business> Businesses { get; set; }
    public DbSet<BusinessPlan> BusinessPlans { get; set; }
    public DbSet<BusinessAnalytic> BusinessAnalytics { get; set; }
    public DbSet<BusinessVerification> BusinessVerifications { get; set; }
    public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }
    public DbSet<Payment> Payments { get; set; }

    // Places & Content
    public DbSet<Place> Places { get; set; }
    public DbSet<OpeningHour> OpeningHours { get; set; }
    public DbSet<BusinessPost> BusinessPosts { get; set; }
    public DbSet<Event> Events { get; set; }
    public DbSet<Offer> Offers { get; set; }

    // Reviews & Interactions
    public DbSet<Review> Reviews { get; set; }
    public DbSet<PostComment> PostComments { get; set; }
    public DbSet<PostLike> PostLikes { get; set; }
    public DbSet<EventBooking> EventBookings { get; set; }
    public DbSet<PlaceFollow> PlaceFollows { get; set; }

    // Categories & Tags
    public DbSet<Category> Categories { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<CategoryTag> CategoryTags { get; set; }
    public DbSet<PlaceTag> PlaceTags { get; set; }

    // AI & Recommendations
    public DbSet<UserLog> UserLogs { get; set; }
    public DbSet<UserInterestProfile> UserInterestProfiles { get; set; }
    public DbSet<RecommendedItem> RecommendedItems { get; set; }
    public DbSet<Notification> Notifications { get; set; }

    

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
