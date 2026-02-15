using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;    
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Project.Core;
using Project.Core.Domain;
using Project.Core.Domain.Entities;
using Project.Core.Domain.RepositoryContracts;
using Project.Core.Domain.RopositoryContracts;
using Project.Core.ServiceContracts;
using Project.Core.Services;
using Project.Infrastructure;
using Project.Infrastructure.ApplicationDbContext;
using Project.Infrastructure.Configurations;
using Project.Infrastructure.Repositories;
using Project.Infrastructure.SignalR;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using WebApi.Middlewares;

IdentityModelEventSource.ShowPII = true;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers(options =>
{
    //Authorization policy
    //var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
    //options.Filters.Add(new AuthorizeFilter(policy));

});

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "api.xml"));
});

// Infrastructure and Core 
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddCoreServices(builder.Configuration);
builder.Services.AddSignalR();



// CORS
var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>();

// 2. إعداد خدمة الـ CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowClient", policyBuilder =>
    {
        policyBuilder
            .WithOrigins(allowedOrigins!) // السماح للروابط دي بس
            .AllowAnyHeader()             // السماح بأي Header
            .AllowAnyMethod()             // السماح بـ GET, POST, PUT, DELETE
            .AllowCredentials();          // السماح بالكوكيز والتوكن (مهم جداً للـ SignalR)
    });
});



var configIssuer = builder.Configuration["Jwt:Issuer"];
var configKey = builder.Configuration["Jwt:Key"];

Console.WriteLine("------------------------------------------------");
Console.WriteLine($"🧐 SERVER EXPECTS Issuer: '{configIssuer}'");
Console.WriteLine($"🧐 TOKEN HAS Issuer:      'http://localhost:5058'"); // ده اللي أنا طلعته من التوكن بتاعك
Console.WriteLine($"🔑 Key Length Loaded:     {configKey?.Length ?? 0}");
Console.WriteLine("------------------------------------------------");

var secretKey = builder.Configuration["Jwt:Key"];

// 🛑 اختبار سريع: طباعة أول 5 حروف من المفتاح عشان تتأكد إنه قرأ المفتاح الصح
// المفروض يطبع: Key Loaded: HayyI...
if (!string.IsNullOrEmpty(secretKey))
{
    Console.WriteLine($"✅ Key Loaded successfully from Config/Secrets! Starts with: {secretKey.Substring(0, 5)}...");
}
else
{
    Console.WriteLine("❌❌❌ ERROR: Key is NULL or EMPTY! Check User Secrets ID.");
}
builder.Services.AddAuthentication(options => {
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;


})
 .AddJwtBearer(options => {
     
     options.TokenValidationParameters = new TokenValidationParameters()
     {
         ValidateIssuer = true,
         ValidIssuer = builder.Configuration["Jwt:Issuer"],

         ValidateAudience = true,
         ValidAudiences = new[]
         {
             builder.Configuration["Jwt:AudienceWeb"],   // اقبل الويب
             builder.Configuration["Jwt:AudienceMobile"] // واقبل الموبايل
         },

         ValidateLifetime = true,
         ValidateIssuerSigningKey = true,
         IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!)),
     };
     options.Events = new JwtBearerEvents
     {
         OnAuthenticationFailed = context =>
         {
             Console.WriteLine($"🔥🔥🔥 Exception: {context.Exception.Message}");
             return Task.CompletedTask;
         },
         OnTokenValidated = context =>
         {
             Console.WriteLine("🟢🟢🟢 ALHAMDULLILAH! Token Worked! 🟢🟢🟢");
             return Task.CompletedTask;
         },
         OnChallenge = context =>
         {
             Console.WriteLine("🟠 OnChallenge Error: " + context.Error + " - " + context.ErrorDescription);
             return Task.CompletedTask;
         }
     };
 });




builder.Services.AddAuthorization(options => {

});


builder.Services.AddHttpLogging(options =>
{
    options.LoggingFields =
    Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.RequestProperties |
    Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.ResponsePropertiesAndHeaders;
});



builder.Services.AddScoped<IUserLogService, UserLogService>();


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    try
    {
        var userManager = services.GetRequiredService<UserManager<User>>();
        var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();
        var config = services.GetRequiredService<IConfiguration>();

        // 👇 شيلنا الـ Comment من هنا
        await DbInitializer.SeedAdminUser(userManager, roleManager, config);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}
app.UseExceptionHandlingMiddleware();
app.UseHttpLogging();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHsts();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseCors("AllowClient");// 3. تفعيل الـ CORS



app.UseAuthentication();
app.UseAuthorization();
// 👇 السطر ده هو اللي بيفتح قناة الاتصال للفرونت إند
app.MapHub<NotificationHub>("/notificationHub");

app.MapControllers();

app.Run();
