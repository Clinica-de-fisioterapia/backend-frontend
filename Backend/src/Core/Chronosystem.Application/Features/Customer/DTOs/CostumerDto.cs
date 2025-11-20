using System;
using Chronosystem.Application.Features.People.DTOs;

namespace Chronosystem.Application.Features.Customers.DTOs
{
    public class CustomerDto
    {
        public Guid Id { get; set; }
        public Guid PersonId { get; set; }
        public PersonDto? Person { get; set; }
    }
}
