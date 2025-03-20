namespace SharpNest.Kafka.Core.Exceptions;

public class KafkaPublisherException(string message, Exception innerException) : Exception(message, innerException)
{
}
