using Chronosystem.Application.Common.Interfaces.Persistence;
using Chronosystem.Application.Features.Bookings.Commands.CreateBooking;
using FluentValidation;
using System.Threading;
using System.Threading.Tasks;

namespace Chronosystem.Application.Features.Bookings.Commands.CreateBooking
{
    public class CreateBookingCommandValidator : AbstractValidator<CreateBookingCommand>
    {
        public CreateBookingCommandValidator(
            IProfessionalRepository professionalRepository,
            ICustomerRepository customerRepository,
            IServiceRepository serviceRepository,
            IUnitRepository unitRepository,
            IBookingRepository bookingRepository)
        {
            RuleFor(x => x.Dto.ProfessionalId)
                .NotEmpty().WithMessage("ProfessionalId é obrigatório.");

            RuleFor(x => x.Dto.CustomerId)
                .NotEmpty().WithMessage("CustomerId é obrigatório.");

            RuleFor(x => x.Dto.ServiceId)
                .NotEmpty().WithMessage("ServiceId é obrigatório.");

            RuleFor(x => x.Dto.UnitId)
                .NotEmpty().WithMessage("UnitId é obrigatório.");

            RuleFor(x => x.Dto)
                .MustAsync(async (dto, ct) =>
                {
                    // start < end
                    if (dto.EndTime <= dto.StartTime) return false;

                    // not in the past
                    if (dto.StartTime < DateTimeOffset.UtcNow) return false;

                    // check service exists and get duration
                    var service = await serviceRepository.GetByIdAsync(dto.ServiceId, ct);
                    if (service == null) return false;

                    // ensure end time respects service duration
                    var expectedEnd = dto.StartTime.AddMinutes(service.DurationMinutes);
                    if (dto.EndTime > expectedEnd.AddMinutes(1)) // allow tiny slack (1 min)
                        return false;

                    // check overlapping bookings for professional
                    var overlaps = await bookingRepository.GetOverlappingAsync(dto.ProfessionalId, dto.StartTime, dto.EndTime, ct);
                    return overlaps == null || overlaps.Count == 0;
                })
                .WithMessage("Invalid time: overlapping booking, duration mismatch or past time.");
        }
    }
}
