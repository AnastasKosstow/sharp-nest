namespace SharpNest.Kafka.Core.Settings;

/// <summary>
/// Settings for Kafka consumers.
/// </summary>
public class KafkaSubscriberSettings
{
    /// <summary>
    /// Gets or sets the auto offset reset behavior (earliest, latest, error).
    /// </summary>
    public string AutoOffsetReset { get; set; } = "earliest";

    /// <summary>
    /// Gets or sets whether to enable auto-commit of offsets.
    /// </summary>
    public bool EnableAutoCommit { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to enable commit empty messages.
    /// </summary>
    public bool CommitEmptyMessages { get; set; } = false;

    /// <summary>
    /// Gets or sets the auto-commit interval in milliseconds.
    /// </summary>
    public int AutoCommitIntervalMs { get; set; } = 5000;

    /// <summary>
    /// Gets or sets the session timeout in milliseconds.
    /// </summary>
    public int SessionTimeoutMs { get; set; } = 30000;

    /// <summary>
    /// Gets or sets the maximum poll interval in milliseconds.
    /// </summary>
    public int MaxPollIntervalMs { get; set; } = 300000;

    /// <summary>
    /// Gets or sets the number of messages to request in each fetch.
    /// </summary>
    public int MaxPartitionFetchBytes { get; set; } = 1048576;

    /// <summary>
    /// Gets or sets the strategy for handling errors that occur during message processing.
    /// Commit (commit and continue), 
    /// Retry (don't commit), 
    /// DeadLetter (send to dead letter topic),
    /// Throw (propagate exception).
    /// </summary>
    public ErrorHandlingStrategy ErrorHandlingStrategy { get; set; } = ErrorHandlingStrategy.Commit;

    /// <summary>
    /// Gets or sets the topic name for storing messages that failed processing.
    /// </summary>
    public string DeadLetterTopic { get; set; } = "dead-letter-queue";
}
