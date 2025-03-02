using System.ComponentModel.DataAnnotations;
using ADAM.Domain.Models;

namespace ADAM.Application.Objects;

public class CreateUserSubscriptionDto
{
    public Guid UserGuid { get; set; }

    public SubscriptionType Type { get; set; }

    [MaxLength(255)]
    public string Value { get; set; }
}