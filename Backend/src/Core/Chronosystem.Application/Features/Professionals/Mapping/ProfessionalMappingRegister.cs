using System.Runtime.CompilerServices;
using Chronosystem.Application.Features.Professionals.DTOs;
using Chronosystem.Domain.Entities;
using Mapster;

namespace Chronosystem.Application.Features.Professionals.Mapping;

internal static class ProfessionalMappingRegister
{
    [ModuleInitializer]
    internal static void Register()
    {
        TypeAdapterConfig<CreateProfessionalDto, Professional>
            .NewConfig()
            .MapWith(dto => Professional.Create(dto.UserId, dto.RegistryCode, dto.Specialty));

        TypeAdapterConfig<Professional, ProfessionalResponseDto>
            .NewConfig();

        TypeAdapterConfig<UpdateProfessionalDto, Professional>
            .NewConfig()
            .IgnoreNonMapped(true)
            .AfterMapping((dto, entity) =>
            {
                entity.UpdateRegistryCode(dto.RegistryCode);
                entity.UpdateSpecialty(dto.Specialty);
            });
    }
}
