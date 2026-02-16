using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Core.DTO
{
    public class PagedResult<T>
    {
        public List<T> Data { get; set; } // البيانات نفسها (الإشعارات)
        public int TotalCount { get; set; } // عدد العناصر الكلي في الداتابيز
        public int PageNumber { get; set; } // رقم الصفحة الحالية
        public int PageSize { get; set; }   // حجم الصفحة
        public int TotalPages { get; set; } // عدد الصفحات الكلي

        // خاصية مفيدة للفرونت عشان يعرف يظهر زرار "Next" ولا لأ
        public bool HasNextPage => PageNumber < TotalPages;
        public bool HasPreviousPage => PageNumber > 1;

        public PagedResult(List<T> data, int totalCount, int pageNumber, int pageSize)
        {
            Data = data;
            TotalCount = totalCount;
            PageNumber = pageNumber;
            PageSize = pageSize;
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        }
    }
}
