using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Core.DTO
{
    public class VerifyOtpDto
    {
        public string Email { get; set; } = string.Empty;
        public string Otp { get; set; } = string.Empty;
    }
}
