using System;

namespace Chronosystem.Application.Features.Units.DTOs;

public class UpdateUnitDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
