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

            // التواريخ (أساس السيستم)
            builder.Property(x => x.StartDate)
                   .IsRequired();

            builder.Property(x => x.EndDate)
                   .IsRequired();

            // هل الاشتراك مفعل؟
            builder.Property(x => x.IsActive)
                   .HasDefaultValue(true);

            builder.Property(x => x.AutoRenew)
                   .HasDefaultValue(false);

            // العلاقات
            // 1. الاشتراك يخص بيزنس واحد
            builder.HasOne(x => x.Business)
                   .WithMany() // أو WithMany(b => b.Subscriptions) لو ضفتها هناك
                   .HasForeignKey(x => x.BusinessId)
                   .OnDelete(DeleteBehavior.Cascade);

            // 2. الاشتراك يتبع خطة سعرية واحدة
            builder.HasOne(x => x.Plan)
                   .WithMany()
                   .HasForeignKey(x => x.PlanId)
                   .OnDelete(DeleteBehavior.Restrict); // ممنوع تمسح باقة وفي ناس مشتركين فيها
        }
    }
}