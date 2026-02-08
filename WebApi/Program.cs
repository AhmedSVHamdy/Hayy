using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;    
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Project.Core;
using Project.Core.Domain;
using Project.Core.Domain.Entities;
using Project.Core.Domain.RepositoryContracts;
using Project.Infrastructure;
using Project.Infrastructure.ApplicationDbContext;
using Project.Infrastructure.Configurations;
using Project.Infrastructure.Repositories;
using Project.Infrastructure.SignalR;
using System.Configuration;
using WebApi.Middlewares;



var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers(options =>
{
    //Authorization policy
    var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
    options.Filters.Add(new AuthorizeFilter(policy));

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
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(app =>
    {
        app.WithOrigins(builder.Configuration.GetSection("Origins").Get<string[]>()!);
    });
});

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
         IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)) // علامة ! عشان لو راجع null
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
app.UseCors("AllowClient");// 3. تفعيل الـ CORS
app.UseRouting();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
// 👇 السطر ده هو اللي بيفتح قناة الاتصال للفرونت إند
app.MapHub<NotificationHub>("/notificationHub");

app.MapControllers();

app.Run();
