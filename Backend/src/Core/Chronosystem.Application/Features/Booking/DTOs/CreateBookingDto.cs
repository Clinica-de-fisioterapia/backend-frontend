using System;

namespace Chronosystem.Application.Features.Bookings.DTOs
{
    public class CreateBookingDto
    {
        public Guid ProfessionalId { get; set; }
        public Guid CustomerId { get; set; }
        public Guid ServiceId { get; set; }
        public Guid UnitId { get; set; }
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }
    }
}
