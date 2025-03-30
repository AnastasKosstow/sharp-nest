namespace SharpNest.Kafka.Core.Settings;

public enum ErrorHandlingStrategy
{
    /// <summary>
    /// Commit the message offset and continue
    /// </summary>
    Commit,

    /// <summary>
    /// Don't commit, let the message be reprocessed
    /// </summary>
    Retry,

    /// <summary>
    /// Send to dead letter queue and commit
    /// </summary>
    DeadLetter,

    /// <summary>
    /// Throw the exception up the stack
    /// </summary>
    Throw
}
