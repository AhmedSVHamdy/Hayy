using Project.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Core.DTO
{
    public class CreateOfferDTO
    {
        public class CreateOfferDto
        {
            public Guid PlaceId { get; set; }
            public string Title { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public string GalleryImages { get; set; } = string.Empty;
            public decimal Discount { get; set; } // نسبة الخصم
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
        }

        public class UpdateOfferDto
        {
            public Guid Id { get; set; }
            public string Title { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public string GalleryImages { get; set; } = string.Empty;
            public decimal Discount { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public OfferStatus Status { get; set; }
        }

        public class OfferResponseDto
        {
            public Guid Id { get; set; }
            public Guid PlaceId { get; set; }
            public string Title { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public string GalleryImages { get; set; } = string.Empty;
            public decimal Discount { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public string Status { get; set; } = string.Empty;
        }
    }
}
