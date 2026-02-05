using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Project.Core.Mappers;
using Project.Core.ServiceContracts;
using Project.Core.Services;
using Project.Core.Validators;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace Project.Core
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddCoreServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IImageService, ImageService>();

            // 1. تسجيل AutoMapper
            services.AddAutoMapper(cfg => { }, typeof(NotificationProfile).Assembly);

            // 2. تسجيل Validators (عرفناهم بس)
            services.AddValidatorsFromAssemblyContaining<NotificationAddRequestValidator>();

            // 3. تسجيل Services (البيزنس لوجيك)
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IBusinessService, BusinessService>();



            return services;

        }
            
        

    }
}
