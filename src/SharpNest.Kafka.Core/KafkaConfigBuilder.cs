using Confluent.Kafka;
using Microsoft.Extensions.Options;
using SharpNest.Kafka.Core.Settings;

namespace SharpNest.Kafka.Core;

public class KafkaConfigBuilder(
    IOptions<KafkaSettings> options,
    IOptionsMonitor<ConsumerConfig> consumerConfig,
    IOptionsMonitor<ProducerConfig> producerConfig)
{
    private readonly KafkaSettings _settings = options.Value;
    private readonly IOptionsMonitor<ConsumerConfig> _consumerConfig = consumerConfig;
    private readonly IOptionsMonitor<ProducerConfig> _producerConfig = producerConfig;

    public ConsumerConfig BuildConsumerConfig(string? groupId = null)
    {
        var subscriberSettings = _settings.Subscriber;
        var effectiveGroupId = groupId ?? _settings.DefaultGroup;

        var config = new ConsumerConfig
        {
            BootstrapServers = _settings.BootstrapServers,
            GroupId = effectiveGroupId,

            AutoOffsetReset = Enum.TryParse<AutoOffsetReset>(subscriberSettings.AutoOffsetReset, true, out var offsetReset)
                ? offsetReset
                : AutoOffsetReset.Earliest,
            EnableAutoCommit = subscriberSettings.EnableAutoCommit,
            AutoCommitIntervalMs = subscriberSettings.AutoCommitIntervalMs,
            SessionTimeoutMs = subscriberSettings.SessionTimeoutMs,
            MaxPollIntervalMs = subscriberSettings.MaxPollIntervalMs,
            MaxPartitionFetchBytes = subscriberSettings.MaxPartitionFetchBytes
        };

        ApplySecuritySettings(config);
        return config;
    }

    public ProducerConfig BuildProducerConfig()
    {
        var publisherSettings = _settings.Publisher;
        var config = new ProducerConfig
        {
            BootstrapServers = _settings.BootstrapServers,

            Acks = Enum.TryParse<Acks>(publisherSettings.Acks, true, out var acks)
                ? acks
                : Acks.All,
            MessageTimeoutMs = publisherSettings.MessageTimeoutMs,
            CompressionType = Enum.TryParse<CompressionType>(publisherSettings.CompressionType, true, out var compressionType)
                ? compressionType
                : CompressionType.None,
            BatchSize = publisherSettings.BatchSize,
            LingerMs = publisherSettings.LingerMs,
            MaxInFlight = publisherSettings.MaxInFlight
        };

        ApplySecuritySettings(config);
        return config;
    }

    private void ApplySecuritySettings(ClientConfig config)
    {
        if (_settings.Security != null)
        {
            config.SecurityProtocol = _settings.Security.Protocol;

            if (_settings.Security.Protocol == SecurityProtocol.SaslPlaintext ||
                _settings.Security.Protocol == SecurityProtocol.SaslSsl)
            {
                config.SaslMechanism = _settings.Security.SaslMechanism;
                config.SaslUsername = _settings.Security.Username;
                config.SaslPassword = _settings.Security.Password;
            }
        }
    }
}
