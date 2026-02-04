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
                _logger.LogError($"{ex.GetType().ToString()}: {ex.Message}");
                if (ex.InnerException is not null)
                {
                    _logger.LogError($"{ex.InnerException.GetType().ToString()}: {ex.InnerException.Message}");
                }

                // 👇 التعديل هنا: تحديد الكود حسب نوع الخطأ
                var statusCode = ex switch
                {
                    KeyNotFoundException => (int)HttpStatusCode.NotFound,       // 404 (مش موجود)
                    UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized, // 401 (غير مسموح)
                    ArgumentException => (int)HttpStatusCode.BadRequest,        // 400 (بيانات غلط)
                    _ => (int)HttpStatusCode.InternalServerError                // 500 (أي حاجة تانية)
                };

                httpContext.Response.StatusCode = statusCode;
                httpContext.Response.ContentType = "application/json"; // تأكيد نوع المحتوى

                // بنرجع رسالة الخطأ
                await httpContext.Response.WriteAsJsonAsync(new
                {
                    StatusCode = statusCode,
                    Message = ex.Message,
                    Type = ex.GetType().Name
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
