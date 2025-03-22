namespace SharpNest.Kafka.Core.Settings;

/// <summary>
/// Settings for Kafka producers.
/// </summary>
public class KafkaPublisherSettings
{
    /// <summary>
    /// Gets or sets the required acknowledgments (0, 1, all).
    /// </summary>
    public string Acks { get; set; } = "all";

    /// <summary>
    /// Gets or sets the message timeout in milliseconds.
    /// </summary>
    public int MessageTimeoutMs { get; set; } = 30000;

    /// <summary>
    /// Gets or sets the compression type (none, gzip, snappy, lz4, zstd).
    /// </summary>
    public string CompressionType { get; set; } = "none";

    /// <summary>
    /// Gets or sets the maximum size of a batch in bytes.
    /// </summary>
    public int BatchSize { get; set; } = 16384;

    /// <summary>
    /// Gets or sets the linger time in milliseconds.
    /// </summary>
    public int LingerMs { get; set; } = 5;

    /// <summary>
    /// Gets or sets the maximum number of in-flight requests.
    /// </summary>
    public int MaxInFlight { get; set; } = 5;
}
