using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Project.Core.Domain;
using Project.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Project.Infrastructure.ApplicationDbContext;

public partial class HayyContext : IdentityDbContext<User, ApplicationRole, Guid>
{
    public HayyContext()
    {
    }

    public HayyContext(DbContextOptions<HayyContext> options) : base(options)
    {

    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        OnModelCreatingPartial(modelBuilder);
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }


    // Users & Settings
    public new DbSet<User> Users { get; set; } = null!;
    public DbSet<UserSettings> UserSettings { get; set; } = null!;
    public DbSet<AdminAction> AdminActions { get; set; } = null!;
    // Business & Plans
    public DbSet<Business> Businesses { get; set; } = null!;
    public DbSet<BusinessPlan> BusinessPlans { get; set; } = null!;
    public DbSet<BusinessAnalytic> BusinessAnalytics { get; set; } = null!;
    public DbSet<BusinessVerification> BusinessVerifications { get; set; } = null!;
    public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; } = null!;
    public DbSet<Payment> Payments { get; set; } = null!;

    // Places & Content
    public DbSet<Place> Places { get; set; } = null!;
    public DbSet<OpeningHour> OpeningHours { get; set; } = null!;
    public DbSet<BusinessPost> BusinessPosts { get; set; } = null!;
    public DbSet<Event> Events { get; set; } = null!;
    public DbSet<Offer> Offers { get; set; } = null!;

    // Reviews & Interactions
    public DbSet<Review> Reviews { get; set; } = null!;
    public DbSet<PostComment> PostComments { get; set; } = null!;
    public DbSet<PostLike> PostLikes { get; set; } = null!;
    public DbSet<EventBooking> EventBookings { get; set; } = null!;
    public DbSet<PlaceFollow> PlaceFollows { get; set; } = null!;
    public DbSet<ReviewReply> ReviewReplies { get; set; } = null!;

    // Categories & Tags
    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<Tag> Tags { get; set; } = null!;
    public DbSet<CategoryTag> CategoryTags { get; set; } = null!;
    public DbSet<PlaceTag> PlaceTags { get; set; } = null!;

    // AI & Recommendations
    public DbSet<UserInterestProfile> UserInterestProfiles { get; set; } = null!;
    public DbSet<RecommendedItem> RecommendedItems { get; set; } = null!;
    public DbSet<Notification> Notifications { get; set; } = null!;



    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
