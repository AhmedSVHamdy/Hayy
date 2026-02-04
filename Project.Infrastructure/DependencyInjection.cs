using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Project.Core.Domain;
using Project.Core.Domain.Entities;
using Project.Core.Domain.RopositoryContracts;
using Project.Infrastructure.ApplicationDbContext;
using Project.Infrastructure.Repositories;
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
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
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


            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IUserInterestRepository, UserInterestRepository>();



            return services;
        }
    }
}
