using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Core.DTO
{
    public class RegisterDto
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        // أي بيانات تانية عاوز تاخدها من اليوزر غير الصورة
    }
}
