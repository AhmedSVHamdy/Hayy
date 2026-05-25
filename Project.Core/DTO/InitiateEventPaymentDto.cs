using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Core.DTO
{
    public class InitiateEventPaymentDto
    {
        public Guid UserId { get; set; }
        public Guid BookingId { get; set; }
    }
}
