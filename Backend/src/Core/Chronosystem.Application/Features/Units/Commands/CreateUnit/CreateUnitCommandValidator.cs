using System.Threading;
using Chronosystem.Application.Common.Interfaces.Persistence;
using FluentValidation;

namespace Chronosystem.Application.Features.Units.Commands.CreateUnit;

public sealed class CreateUnitCommandValidator : AbstractValidator<CreateUnitCommand>
{
    private readonly IUnitRepository _unitRepository;

    public CreateUnitCommandValidator(IUnitRepository unitRepository)
    {
        _unitRepository = unitRepository;

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("O nome da unidade é obrigatório.")
            .MaximumLength(255).WithMessage("O nome da unidade não pode exceder 255 caracteres.")
            .MustAsync((name, cancellation) => BeUniqueName(name.Trim(), cancellation))
            .WithMessage("Já existe uma unidade com este nome.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("O ID do usuário é obrigatório.");
    }

    private async Task<bool> BeUniqueName(string name, CancellationToken cancellationToken)
    {
        return !await _unitRepository.UnitNameExistsAsync(name, cancellationToken);
    }
}
