using Chronosystem.Application.Common.Interfaces.Persistence;
using FluentValidation;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Chronosystem.Application.Features.Customers.Commands.DeleteCustomer
{
    public class DeleteCustomerCommandHandler : IRequestHandler<DeleteCustomerCommand>
    {
        private readonly ICustomerRepository _repository;
        private readonly IUnitOfWork _uow;

        public DeleteCustomerCommandHandler(ICustomerRepository repository, IUnitOfWork uow)
        {
            _repository = repository;
            _uow = uow;
        }

        public async Task<Unit> Handle(DeleteCustomerCommand request, CancellationToken cancellationToken)
        {
            var entity = await _repository.GetByIdAsync(request.Id);
            if (entity == null)
                throw new ValidationException("Customer not found.");

            entity.DeletedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(entity);
            await _uow.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
