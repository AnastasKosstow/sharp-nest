namespace SharpNest.Kafka.Core.Exceptions;

public class KafkaSerializationException(string message, Exception innerException) : Exception(message, innerException)
{
}