namespace SharpNest.SSE.Core.Abstractions;

/// <summary>
/// 
/// </summary>
public interface IMessage<TPayload>
{
    string Id { get; }
    TPayload Payload { get; }
    IDictionary<string, string> Metadata { get; }
}
