using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Threading.Tasks;
#pragma warning disable 1591
namespace WebApi.Middlewares
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(ILogger<ExceptionHandlingMiddleware> logger, RequestDelegate next)
        {
            _logger = logger;
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                // ✅ الصح: مرر الـ ex كامل عشان الـ Logs في Azure تلقط الـ Stack Trace بالملي
                _logger.LogError(ex, "حدث خطأ غير متوقع أثناء معالجة الطلب: {Message}", ex.Message);

                var statusCode = ex switch
                {
                    KeyNotFoundException => (int)HttpStatusCode.NotFound,
                    UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
                    ArgumentException => (int)HttpStatusCode.BadRequest,
                    _ => (int)HttpStatusCode.InternalServerError
                };

                httpContext.Response.StatusCode = statusCode;
                httpContext.Response.ContentType = "application/json";

                // ✅ الحل الذكي: لو الخطأ 500 اخفي التفاصيل واظهر رسالة عامة، لو خطأ متوقع اظهر الرسالة الحقيقية
                string clientMessage = statusCode == (int)HttpStatusCode.InternalServerError
                    ? "حدث خطأ داخلي في السيرفر، يرجى المحاولة لاحقاً."
                    : ex.Message;

                await httpContext.Response.WriteAsJsonAsync(new
                {
                    StatusCode = statusCode,
                    Message = clientMessage,
                    Type = statusCode == (int)HttpStatusCode.InternalServerError ? "ServerError" : ex.GetType().Name
                });
            }
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class ExceptionHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseExceptionHandlingMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ExceptionHandlingMiddleware>();
        }
    }
}
