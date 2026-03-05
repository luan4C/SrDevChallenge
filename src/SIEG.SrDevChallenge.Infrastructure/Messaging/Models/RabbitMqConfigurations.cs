using System;

namespace SIEG.SrDevChallenge.Infrastructure.Messaging.Models;

public class RabbitMqConfigurations
{
    public string Host { get; set; } = default!;
    public string User { get; set; } = default!;
    public string Password { get; set; } = default!;
    public int Port { get; set; }
}
