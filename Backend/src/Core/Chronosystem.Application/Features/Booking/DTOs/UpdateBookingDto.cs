namespace Chronosystem.Application.Dtos.Booking;
public class UpdateBookingDto
{
    public Guid ServiceId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}
