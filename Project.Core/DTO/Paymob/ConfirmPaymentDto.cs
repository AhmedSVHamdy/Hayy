using Project.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Core.DTO.Paymob
{
    public class ConfirmPaymentDto
    {
        public Guid BookingId { get; set; }
        public string TransactionId { get; set; } = string.Empty;
        public PaymentMethod PaymentMethod { get; set; }
    }
}
