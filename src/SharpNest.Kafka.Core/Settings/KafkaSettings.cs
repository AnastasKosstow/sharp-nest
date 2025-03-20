namespace SharpNest.Kafka.Core.Settings;

/// <summary>
/// Settings for Kafka connections.
/// </summary>
public class KafkaSettings
{
    /// <summary>
    /// Gets or sets the bootstrap servers.
    /// </summary>
    public string BootstrapServers { get; set; } = "localhost:9092";

    /// <summary>
    /// Gets or sets the default consumer group.
    /// </summary>
    public string DefaultGroup { get; set; } = "default-group";

    /// <summary>
    /// Gets or sets the number of partitions for new topics.
    /// </summary>
    public int Partitions { get; set; } = 1;

    /// <summary>
    /// Gets or sets the replication factor for new topics.
    /// </summary>
    public short ReplicationFactor { get; set; } = 1;

    /// <summary>
    /// Gets or sets the security settings.
    /// </summary>
    public KafkaSecuritySettings? Security { get; set; }

    /// <summary>
    /// Gets or sets the subscriber settings.
    /// </summary>
    public KafkaSubscriberSettings Subscriber { get; set; } = new();

    /// <summary>
    /// Gets or sets the publisher settings.
    /// </summary>
    public KafkaPublisherSettings Publisher { get; set; } = new();

}
