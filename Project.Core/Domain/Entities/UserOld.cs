using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Project.Core.Domain.Entities
{
    public class UserOld
    {
        [Key]
        public int Id { get; set; } // ده المفتاح الأساسي
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; } // (مؤقتاً هنحفظه نص عادي)

        // 👇 ده أهم سطر: هنا هنخزن اللينك اللي هيرجع من Azure
        public string? ProfilePictureUrl { get; set; }
    }


}
