using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using Project.Core.Domain;
using Project.Core.Domain.Entities;
using Project.Core.Domain.RepositoryContracts;
using Project.Core.Domain.RopositoryContracts;
using Project.Core.ServiceContracts; // عشان INotifier
using Project.Core.Settings;
using Project.Infrastructure.ApplicationDbContext;
using Project.Infrastructure.Repositories;
using Project.Infrastructure.SignalR;
using System;

namespace Project.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // ====================================================
            // 1. إعدادات قواعد البيانات (SQL & Mongo)
            // ====================================================
            services.AddDbContext<HayyContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"),
                sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorNumbersToAdd: null);
                });
            });

            // MongoDb Configuration
            try { BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String)); }
            catch (BsonSerializationException) { }

            if (!BsonClassMap.IsClassMapRegistered(typeof(UserLog)))
            {
                BsonClassMap.RegisterClassMap<UserLog>(cm => { cm.AutoMap(); cm.MapIdMember(c => c.Id); });
            }

            services.AddSingleton<IMongoClient>(sp =>
            {
                var connectionString = configuration["MongoConnection"] ?? configuration.GetConnectionString("MongoConnection");
                if (string.IsNullOrEmpty(connectionString)) throw new Exception("MongoConnection string is missing.");
                return new MongoClient(connectionString);
            });

            services.AddScoped<IMongoDatabase>(sp =>
            {
                var client = sp.GetRequiredService<IMongoClient>();
                return client.GetDatabase("GraduationProjectDb");
            });

            // ====================================================
            // 2. إعدادات Identity
            // ====================================================
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

            // ====================================================
            // 3. SignalR Configuration
            // ====================================================
            services.AddSignalR();

            // ====================================================
            // 4. Repositories (مكانهم هنا)
            // ====================================================
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            

            services.AddScoped<IUserLogRepository, UserLogRepository>();
            services.AddScoped<INotificationRepository, NotificationRepository>();
            services.AddScoped<IBusinessRepository, BusinessRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IPlaceRepository, PlaceRepository>();
            services.AddScoped<IUserInterestRepository, UserInterestRepository>();
            services.AddScoped<IAdminRepository, AdminRepository>();
            services.AddScoped<IReviewRepository, ReviewRepository>();
            services.AddScoped<IBusinessPostRepository, BusinessPostRepository>();
            services.AddScoped<IPostCommentRepository, PostCommentRepository>();
            services.AddScoped<IPostLikeRepository, PostLikeRepository>();
            services.AddScoped<IReviewReplyRepository, ReviewReplyRepository>();
            services.AddScoped<IPlaceFollowRepository, PlaceFollowRepository>();


            // ====================================================
            // 5. Infrastructure Services (الخدمات المرتبطة بالبنية التحتية فقط)
            // ====================================================
            services.Configure<PaymobSettings>(configuration.GetSection("Paymob"));

            // SignalRNotifier يعتمد على HubContext الموجود هنا، لذلك يبقى هنا
            services.AddScoped<INotifier, SignalRNotifier>();

            // ❌ حذفنا باقي الـ Services (PlaceService, CategoryService, etc.)
            // لأن مكانهم الطبيعي هو ملف Core.DependencyInjection

            // ❌ حذفنا AutoMapper 
            // لأنه مسجل بالفعل في Core.DependencyInjection

            return services;
        }
    }
}