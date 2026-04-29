using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Project.Core.Domain.Entities;

namespace Project.Infrastructure.Configuration
{
    internal class BusinessSubscriptionConfiguration : IEntityTypeConfiguration<BusinessSubscription>
    {
        public void Configure(EntityTypeBuilder<BusinessSubscription> builder)
        {
            builder.ToTable("BusinessSubscriptions");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.StartDate)
                   .IsRequired();

            builder.Property(x => x.EndDate)
                   .IsRequired();

            builder.Property(x => x.IsActive)
                   .HasDefaultValue(true);

            builder.Property(x => x.AutoRenew)
                   .HasDefaultValue(false);

            // ✅ التعديل: ربطنا WithMany بالـ navigation property عشان EF ميعملش BusinessId1
            builder.HasOne(x => x.Business)
                   .WithMany(b => b.Subscriptions)
                   .HasForeignKey(x => x.BusinessId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Plan)
                   .WithMany()
                   .HasForeignKey(x => x.PlanId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}