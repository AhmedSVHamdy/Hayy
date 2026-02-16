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
using System.Reflection;

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
                options.CreateMap<NotificationAddRequest, Notification>();

            });
            services.AddValidatorsFromAssemblyContaining<ChangePasswordValidator>();

            // 2. إعدادات الإيميل
            services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
            services.AddTransient<IEmailService, EmailService>();

            // 3. تسجيل خدمات الـ Authentication
            services.AddTransient<IJwtService, JwtService>();
            services.AddScoped<IAuthWeb, AuthWeb>();
            services.AddScoped<IAuthUsers, AuthApp>();

            // 4. تسجيل باقي خدمات البيزنس
            services.AddScoped<IImageService, ImageService>();
            services.AddScoped<IInterestService, InterestService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IPlaceService, PlaceService>();
            services.AddScoped<IReviewService, ReviewService>();
            services.AddScoped<IBusinessPostService, BusinessPostService>();
            services.AddScoped<IPostCommentService, PostCommentService>();
            services.AddScoped<IPostLikeService, PostLikeService>();
            services.AddScoped<IUserLogService, UserLogService>();

            services.AddScoped<IBusinessService, BusinessService>();

            services.AddScoped<IAdminService, AdminService>();


            // 2. تفعيل FluentValidation
            services.AddValidatorsFromAssemblyContaining<CreateUserLogValidator>();
            // AutoMapper Profiles

            services.AddAutoMapper(cfg => cfg.AddProfile<ReviewProfile>());
            services.AddAutoMapper(cfg => cfg.AddProfile<BusinessPostProfile>());
            services.AddAutoMapper(cfg => cfg.AddProfile<CommentProfile>());
            services.AddAutoMapper(cfg => cfg.AddProfile<LikeProfile>());
            services.AddAutoMapper(cfg => cfg.AddProfile<NotificationProfile>());
            services.AddAutoMapper(cfg => cfg.AddProfile<UserLogProfile>());
            services.AddAutoMapper(cfg => cfg.AddProfile<CategoryProfile>());
            services.AddAutoMapper(cfg => cfg.AddProfile<PlaceProfile>());


            return services;
        }
    }
}