using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
//using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;
using Project.Core; // 👈 1. ضيفنا دي عشان يشوف AddCoreServices
using Project.Core.ServiceContracts;
using Project.Infrastructure; // ضروري عشان يشوف دالة AddInfrastructureServices
using Project.Infrastructure.ApplicationDbContext; // عشان الـ Seeder
using Project.Infrastructure.SignalR; // عشان NotificationHub
using System.Reflection;
using System.Text;
using System.Text.Json;
using WebApi.Middlewares; // لو عندك Middleware

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. استدعاء طبقات المشروع (Infrastructure & Core)
// ==========================================

// أ) البنية التحتية (Database, Identity, Repositories, SignalR)
builder.Services.AddInfrastructureServices(builder.Configuration);

// ب) قلب المشروع (Services, AutoMapper, Validators) 👈 2. ضيفنا السطر المهم ده
builder.Services.AddCoreServices(builder.Configuration);


// ==========================================
// 2. إعدادات الـ Web API (Controllers, Swagger, CORS)
// ==========================================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// إعدادات Swagger
builder.Services.AddSwaggerGen(options =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token in the text box below.\r\n\r\nExample: '12345abcdef'"
    });

    // 2. تفعيل القفل على الـ Endpoints
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});


 


// إعدادات CORS
var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowClient", policyBuilder =>
    {
        policyBuilder
            .WithOrigins(allowedOrigins ?? new[] { "http://localhost:8080" })
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});
// ==========================================
// 3. إعدادات التوثيق (JWT Authentication)
// ==========================================
var secretKey = builder.Configuration["Jwt:Key"];
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidateAudience = true,
        ValidAudiences = new[]
        {
            builder.Configuration["Jwt:AudienceWeb"],
            builder.Configuration["Jwt:AudienceMobile"]
        },
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!)),
    };

    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine($"Authentication Failed: {context.Exception.Message}");
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            return Task.CompletedTask;
        }
    };
});

// ==========================================
// 4. بناء التطبيق والـ Middleware
// ==========================================
var app = builder.Build();
app.UseExceptionHandlingMiddleware();

using (var scope = app.Services.CreateScope())
{
    var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();

    recurringJobManager.AddOrUpdate<IOfferService>(
        "expire-finished-offers-job",
        service => service.ExpireFinishedOffersAsync(),
        Cron.Daily(),
        new RecurringJobOptions
        {
            TimeZone = TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time")
        }
    );
}


if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger(); // دي كفاية مرة واحدة هنا
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "HAYY API V1");
        options.EnableDeepLinking();
        options.DisplayRequestDuration();
        options.EnableFilter();
    });
}

// تهيئة البيانات (Seeding) - فعلناها عشان الاختبار
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // لو عايز تشغل الـ Seeder بتاع التصنيفات، شيل الـ Comment ده 👇
        // var context = services.GetRequiredService<HayyContext>();
        // await DataSeeder.SeedAsync(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error during database seeding.");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();   
}
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// الترتيب مهم جداً
app.UseCors("AllowClient");
app.UseAuthentication();
app.UseAuthorization();

// نقاط النهاية (Endpoints)
app.MapHub<NotificationHub>("/notificationHub");
// 👇 السطر ده بس عشان يشغل لك لوحة التحكم على المتصفح
app.UseHangfireDashboard("/hangfire");
app.MapControllers();

app.Run();