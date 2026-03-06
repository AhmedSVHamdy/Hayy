using Google;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Project.Core.Enums;
using Project.Core.ServiceContracts;
using Project.Infrastructure.ApplicationDbContext;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Core.Services
{
    public class BookingExpirationWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<BookingExpirationWorker> _logger;

        public BookingExpirationWorker(IServiceScopeFactory scopeFactory, ILogger<BookingExpirationWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // بنفتح Scope عشان نقدر نكلم الداتابيز
                    using var scope = _scopeFactory.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<HayyContext>();
                    var notifier = scope.ServiceProvider.GetRequiredService<INotifier>(); // SignalR

                    // 1. نجيب كل الحجوزات اللي معلقه وعدى وقتها 
                    var expiredBookings = await dbContext.EventBookings
                        .Where(b => b.Status == BookingStatus.Pending && b.PaymentDeadline < DateTime.UtcNow)
                        .ToListAsync(stoppingToken);

                    foreach (var expiredBooking in expiredBookings)
                    {
                        // 2. نلغي الحجز اللي مدفعش
                        expiredBooking.Status = BookingStatus.Cancelled;
                        expiredBooking.PaymentDeadline = null;

                        _logger.LogInformation($"تم إلغاء الحجز {expiredBooking.Id} لعدم الدفع.");

                        // 3. نجيب أول واحد في قائمة الانتظار لنفس الإيفنت ده
                        var nextInWaitlist = await dbContext.EventBookings
                            .Where(b => b.EventId == expiredBooking.EventId && b.Status == BookingStatus.Waitlisted)
                            .OrderBy(b => b.WaitlistPosition)
                            .FirstOrDefaultAsync(stoppingToken);

                        if (nextInWaitlist != null)
                        {
                            // 4. مبروك! دورك جه.. هنديله 15 دقيقة يدفع
                            nextInWaitlist.Status = BookingStatus.Pending;
                            nextInWaitlist.PaymentDeadline = DateTime.UtcNow.AddMinutes(15);
                            nextInWaitlist.WaitlistPosition = null; // نطلعه من الطابور

                            // 5. نبعتله إشعار على الموبايل بـ SignalR (اليوزر نفسه)
                            await notifier.SendNotificationToUserWaitlist(
                                nextInWaitlist.UserId.ToString(),
                                "مبروك! تذكرتك من قائمة الانتظار أصبحت متاحة. أمامك 15 دقيقة لإتمام الدفع."
                            );
                        }
                    }

                    if (expiredBookings.Any())
                    {
                        await dbContext.SaveChangesAsync(stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "حدث خطأ أثناء فحص الحجوزات المنتهية.");
                }

                // الكود هينام دقيقة ويرجع يشتغل تاني
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}
