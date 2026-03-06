using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Core.DTO
{
    public class VerifyTicketResultDto
    {
        public bool IsValid { get; set; }
        public string Message { get; set; } = string.Empty;
        public string EventTitle { get; set; } = string.Empty;
        public string AttendeeName { get; set; } = string.Empty; // لو مسجل اسم اليوزر
        public int TicketQuantity { get; set; }
    }
}
