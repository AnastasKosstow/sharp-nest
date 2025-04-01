using SharpNest.SSE.Core.Abstractions;

namespace SharpNest.Samples.SSE.Configuration;

public class Notification : IMessage
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public object Payload { get; set; }
    public IDictionary<string, string> Metadata { get; set; }
}
