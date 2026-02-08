using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Project.Core.Domain.Entities;
using Project.Core.Domain.RepositoryContracts; // تأكد من الاسم الصحيح
using Project.Core.DTO;
using Project.Core.Mappers;
using Project.Core.ServiceContracts;
using Project.Core.Services;
using Project.Core.Validators;

namespace Project.Core
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddCoreServices(this IServiceCollection services, IConfiguration configuration)
        {
            // 1. تسجيل خدمات الـ System/Framework
            services.AddHttpContextAccessor(); // 👈 نضعها في البداية للأمان
            services.AddAutoMapper(options =>
            {
                options.CreateMap<Business, BusinessResponse>();
                options.CreateMap<NotificationAddRequest, Notification>();

            });
            services.AddValidatorsFromAssemblyContaining<ChangePasswordValidator>();

            // 2. إعدادات الإيميل
            services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
            services.AddTransient<IEmailService, EmailService>();

            // 3. تسجيل خدمات الـ Authentication
            services.AddTransient<IJwtService, JwtService>();
            services.AddScoped<IAuthWeb, AuthWeb>();
            services.AddScoped<IAuthUsers, AuthUsers>();

            // 4. تسجيل باقي خدمات البيزنس
            services.AddScoped<IImageService, ImageService>();
            services.AddScoped<IInterestService, InterestService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IBusinessService, BusinessService>();


            services.AddScoped<IAdminService, AdminService>();

            return services;
        }
    }
}