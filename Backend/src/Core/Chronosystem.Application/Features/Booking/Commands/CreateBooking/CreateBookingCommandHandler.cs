using Chronosystem.Application.Common.Interfaces.Persistence;
using Chronosystem.Application.Features.Bookings.Commands.CreateBooking;
using Chronosystem.Domain.Entities;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Chronosystem.Application.Features.Bookings.Commands.CreateBooking
{
    public class CreateBookingCommandHandler : IRequestHandler<CreateBookingCommand, Guid>
    {
        private readonly IBookingRepository _repo;
        private readonly IUnitOfWork _uow;

        public CreateBookingCommandHandler(IBookingRepository repo, IUnitOfWork uow)
        {
            _repo = repo;
            _uow = uow;
        }

        public async Task<Guid> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
        {
            var dto = request.Dto;

            var booking = new Booking
            {
                ProfessionalId = dto.ProfessionalId,
                CustomerId = dto.CustomerId,
                ServiceId = dto.ServiceId,
                UnitId = dto.UnitId,

                // ✔ Correção: converter DateTimeOffset → DateTime (UTC)
                StartTime = dto.StartTime.UtcDateTime,
                EndTime   = dto.EndTime.UtcDateTime,

                Status = "confirmed"
            };

            await _repo.AddAsync(booking, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            return booking.Id;
        }
    }
}
