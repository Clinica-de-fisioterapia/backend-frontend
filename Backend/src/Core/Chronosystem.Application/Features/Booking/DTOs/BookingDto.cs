using System;

namespace Chronosystem.Application.Features.Bookings.DTOs
{
    public class BookingDto
    {
        public Guid Id { get; set; }
        public Guid ProfessionalId { get; set; }
        public Guid CustomerId { get; set; }
        public Guid ServiceId { get; set; }
        public Guid UnitId { get; set; }
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }
        public string Status { get; set; } = default!;
    }
}
