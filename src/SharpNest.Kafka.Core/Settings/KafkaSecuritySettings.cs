using Confluent.Kafka;

namespace SharpNest.Kafka.Core.Settings;

/// <summary>
/// Security settings for Kafka connections.
/// </summary>
public class KafkaSecuritySettings
{
    /// <summary>
    /// Gets or sets the security protocol.
    /// </summary>
    public SecurityProtocol Protocol { get; set; } = SecurityProtocol.Plaintext;

    /// <summary>
    /// Gets or sets the SASL mechanism.
    /// </summary>
    public SaslMechanism SaslMechanism { get; set; } = SaslMechanism.Plain;

    /// <summary>
    /// Gets or sets the SASL username.
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Gets or sets the SASL password.
    /// </summary>
    public string? Password { get; set; }
}
