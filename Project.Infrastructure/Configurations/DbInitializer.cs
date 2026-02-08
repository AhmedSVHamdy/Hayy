using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Project.Core.Domain;
using Project.Core.Domain.Entities; // تأكد من المسار
using Project.Core.Enums;
using System;
using System.Threading.Tasks;

namespace Project.Infrastructure.Configurations
{
    public static class DbInitializer
    {
        public static async Task SeedAdminUser(
            UserManager<User> userManager,
            RoleManager<ApplicationRole> roleManager,
            IConfiguration configuration)
        {
            // 1. قراءة البيانات
            string adminEmail = configuration["SuperAdmin:Email"] ?? throw new Exception("Admin Email not configured");
            string adminPassword = configuration["SuperAdmin:Password"] ?? throw new Exception("Admin Password not configured");

            // 2. 🔥 إنشاء الرولز لو مش موجودة (خطوة ضرورية جداً)
            string[] roles = { "Admin", "Business", "User" };

            foreach (var roleName in roles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    // لو بتستخدم Guid للـ Id
                    await roleManager.CreateAsync(new ApplicationRole { Name = roleName, NormalizedName = roleName.ToUpper() });
                    // لو بتستخدم IdentityRole العادية:
                    // await roleManager.CreateAsync(new ApplicationRole(roleName));
                }
            }

            // 3. إنشاء الأدمن
            var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
            if (existingAdmin == null)
            {
                var user = new User
                {
                    FullName = "Super Admin",
                    UserName = adminEmail,
                    Email = adminEmail,
                    UserType = UserType.Admin.ToString(),
                    IsVerified = true,
                    EmailConfirmed = true, // مهم عشان يقدر يعمل Login
                    CreatedAt = DateTime.UtcNow,
                    SecurityStamp = Guid.NewGuid().ToString(),
                    City = "Mansoura"
                };

                // إنشاء اليوزر
                var result = await userManager.CreateAsync(user, adminPassword);

                if (result.Succeeded)
                {
                    // إعطاء صلاحية الأدمن
                    await userManager.AddToRoleAsync(user, "Admin");
                }
                else
                {
                    // لو حصل خطأ (مثلاً الباسورد ضعيف)
                    throw new Exception($"Failed to create admin: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
        }
    }
}