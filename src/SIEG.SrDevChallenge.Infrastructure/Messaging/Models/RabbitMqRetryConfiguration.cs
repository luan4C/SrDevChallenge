namespace SIEG.SrDevChallenge.Infrastructure.Messaging.Models;

public class RabbitMqRetryConfiguration
{
    public int MaxRetryAttempts { get; set; } = 3;
    public TimeSpan InitialDelay { get; set; } = TimeSpan.FromSeconds(2);
    public TimeSpan MaxDelay { get; set; } = TimeSpan.FromMinutes(5);
    public double BackoffMultiplier { get; set; } = 2.0;
    public bool EnableDeadLetter { get; set; } = true;
    public string DeadLetterSuffix { get; set; } = ".dlx";
    public bool EnableCircuitBreaker { get; set; } = true;
    public int CircuitBreakerThreshold { get; set; } = 5;
    public TimeSpan CircuitBreakerDuration { get; set; } = TimeSpan.FromMinutes(1);
}
