namespace Chronosystem.Application.Features.Services.DTOs;

public record ServiceDto(Guid Id, string Name, int DurationMinutes, decimal Price);

public record UpdateServiceBodyDto(string Name, int DurationMinutes, decimal Price);