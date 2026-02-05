using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Project.Core;
using Project.Core.Domain.RopositoryContracts;
using Project.Infrastructure;
using Project.Infrastructure.ApplicationDbContext;
using Project.Infrastructure.SignalR;
using Project.Infrastructure.Repositories;
using System.Configuration;
using WebApi.Middlewares;



var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
//السطر ده بيقول للمتصفح: "يا متصفح، لو جالك طلب من localhost:3000، عديّه متقلقش، أنا عارفه وواثق فيه".
// 1. إعدادات الـ CORS (مهمة جداً للـ SignalR)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowClient", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:4200") // حط رابط الفرونت إند بتاعك هنا
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // ⚠️ دي إلزامية مع SignalR
    });
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

builder.Services.AddHttpLogging(options =>
{
    options.LoggingFields =
    Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.RequestProperties |
    Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.ResponsePropertiesAndHeaders;
});
var app = builder.Build();


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
