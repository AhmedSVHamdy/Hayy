using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Project.Core.Domain;
using Project.Core.Domain.Entities;
using Project.Core.Enums;

namespace Project.Infrastructure.Configurations
{
    public static class DbInitializer
    {
        // ضفنا IConfiguration هنا 👇
        public static async Task SeedAdminUser(UserManager<User> userManager, RoleManager<ApplicationRole> roleManager, IConfiguration configuration)
        {
            // 1. قراءة البيانات من الإعدادات المخفية
            string adminEmail = configuration["SuperAdmin:Email"] ?? throw new Exception("Admin Email not found in config");
            string adminPassword = configuration["SuperAdmin:Password"] ?? throw new Exception("Admin Password not found in config");

            // ... كود إنشاء الرولز كما هو ...

            // 2. استخدام المتغيرات
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                var user = new User
                {
                    FullName = "Abdelrahman",
                    UserName = adminEmail, // استخدام المتغير
                    Email = adminEmail,    // استخدام المتغير
                    UserType = UserType.Admin.ToString(),
                    IsVerified = true,
                    CreatedAt = DateTime.UtcNow,
                    SecurityStamp = Guid.NewGuid().ToString(),
                    City = "Mansoura"
                };

                var result = await userManager.CreateAsync(user, adminPassword); // استخدام المتغير

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Admin");
                }
            }
        }
    }
}
