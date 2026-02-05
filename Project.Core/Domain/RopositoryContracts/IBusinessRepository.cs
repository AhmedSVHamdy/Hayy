using Project.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Core.Domain.RopositoryContracts
{
    public interface IBusinessRepository
    {
        // بنجيب البيزنس بالـ ID عشان نعدل حالته
        Task<Business?> GetByIdAsync(Guid id);

        // دالة الحفظ (Update)
        Task UpdateAsync(Business business);
    }
}
