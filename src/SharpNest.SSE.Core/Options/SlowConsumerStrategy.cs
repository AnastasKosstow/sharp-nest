namespace SharpNest.SSE.Core.Options;

public enum SlowConsumerStrategy
{
    /// <summary>
    /// Drop messages for slow subscribers
    /// </summary>
    DropMessages,

    /// <summary>
    /// Disconnect slow subscribers
    /// </summary>
    DisconnectSubscriber,

    /// <summary>
    /// Wait for slow subscribers (may cause backpressure)
    /// </summary>
    Wait
}
