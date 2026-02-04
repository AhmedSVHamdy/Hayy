using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Project.Core.Domain.RopositoryContracts;
using Project.Core.DTO;
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
            services.AddScoped<IInterestService, InterestService>();

            services.AddTransient<IJwtService, JwtService>();

            services.AddScoped<IAuthWeb, AuthWeb>();
            services.AddScoped<IAuthUsers, AuthUsers>();

            services.AddValidatorsFromAssemblyContaining<ChangePasswordValidator>();

            services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));

            // 2. تسجيل الخدمة
            services.AddTransient<IEmailService, EmailService>();




            return services;

        }
            
        

    }
}
