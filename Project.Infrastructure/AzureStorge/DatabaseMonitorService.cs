using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Project.Core.ServiceContracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Core.Services
{
    public class DatabaseMonitorService : IDatabaseMonitorService
    {
        private readonly IConfiguration _configuration;
        // لو عندك خدمة جاهزة بتبعت إيميلات في المشروع اعملها Inject هنا
         private readonly IEmailService _emailService; 

        public DatabaseMonitorService(IConfiguration configuration , IEmailService emailService )
        {
            _configuration = configuration;
            // _emailService = emailService;
        }

        public async Task CheckDatabaseSizeAsync()
        {
            // 1. هنجيب الـ Connection String بتاعك
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            double usedSpaceMB = 0;

            // 2. هنشغل الـ Query اللي بيحسب المساحة
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                string query = "SELECT SUM(reserved_page_count) * 8.0 / 1024 FROM sys.dm_db_partition_stats";

                using (var command = new SqlCommand(query, connection))
                {
                    var result = await command.ExecuteScalarAsync();
                    if (result != DBNull.Value)
                    {
                        usedSpaceMB = Convert.ToDouble(result);
                    }
                }
            }

            // 3. نتحقق من الشرط (80% من الـ 2 جيجا = 1638.4 ميجا)
            double thresholdMB = 1638.4;

            if (usedSpaceMB > thresholdMB)
            {
                // 4. نبعت الإيميل!
                string message = $"تحذير هام: الداتابيز بتاعتك استهلكت {usedSpaceMB:F2} ميجابايت وتخطت حاجز الـ 80%!";

                // استخدم الخدمة بتاعتك لبعت الإيميل
                 await _emailService.SendEmailAsync("ahmedmohamedtaha200444@gmail.com", "تحذير مساحة الداتابيز", message);

                Console.WriteLine(message); // للـ Testing في الـ Console
            }
        }
    }
}
