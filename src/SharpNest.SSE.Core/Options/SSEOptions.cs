namespace SharpNest.SSE.Core.Options;

public class SSEOptions
{
    /// <summary>
    /// Maximum capacity of the message channel for each subscriber
    /// </summary>
    public int ChannelCapacity { get; set; } = 200;

    /// <summary>
    /// Timeout for writing messages to subscribers
    /// </summary>
    public TimeSpan WriteTimeout { get; set; } = TimeSpan.FromSeconds(1);
}
