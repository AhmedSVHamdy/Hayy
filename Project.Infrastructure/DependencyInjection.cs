using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Project.Core.Domain;
using Project.Core.Domain.Entities;
using Project.Core.Domain.RopositoryContracts;
using Project.Core.Mappers;
using Project.Core.ServiceContracts;
using Project.Core.Services;
using Project.Core.Validators;
using Project.Infrastructure.ApplicationDbContext;
using Project.Infrastructure.Repositories;
using Project.Infrastructure.SignalR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<HayyContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"),
                sqlOptions => // 👈 ضيف السطر ده والي تحته
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,       // يحاول 5 مرات قبل ما ييأس
                        maxRetryDelay: TimeSpan.FromSeconds(10), // يستنى 10 ثواني بين كل محاولة
                        errorNumbersToAdd: null); // أرقام أخطاء SQL إضافية (اختياري)
                });
            });


            services.AddIdentity<User, ApplicationRole>(options =>
            {
                options.Password.RequiredUniqueChars = 3;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 8;

            })
            .AddEntityFrameworkStores<HayyContext>()
            .AddDefaultTokenProviders()
            .AddUserStore<UserStore<User, ApplicationRole, HayyContext, Guid>>()
            .AddRoleStore<RoleStore<ApplicationRole, HayyContext, Guid>>();

            // 1. تعريف الـ Repositories
            services.AddScoped<INotificationRepository, NotificationRepository>();
            services.AddScoped<IBusinessRepository, BusinessRepository>();
            // 3. تعريف الـ SignalR والـ Notifier
            services.AddScoped<INotifier, SignalRNotifier>(); // ربط الانترفيس بالتنفيذ

            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IUserInterestRepository, UserInterestRepository>();



            return services;
        }
    }
}
