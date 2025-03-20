using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using SharpNest.Kafka.Core;
using SharpNest.Kafka.Core.Abstractions;
using SharpNest.Kafka.Core.Settings;

namespace SharpNest.Kafka;

internal class KafkaConfiguration(IServiceCollection services) : IKafkaConfiguration
{
    private readonly IServiceCollection _services = services;

    /// <inheritdoc/>
    public IServiceCollection AddPublisher()
    {
        var publisherDescriptor = new ServiceDescriptor(typeof(IPublisher), typeof(KafkaPublisher), ServiceLifetime.Singleton);

        _services.Add(publisherDescriptor);
        return _services;
    }

    /// <inheritdoc/>
    public IKafkaConfiguration ConfigurePublisher(Action<KafkaPublisherSettings> configurator)
    {
        _services.Configure<KafkaSettings>(settings =>
        {
            configurator(settings.Publisher);
        });

        return this;
    }

    /// <inheritdoc/>
    public IServiceCollection AddSingletonSubscriber()
    {
        return AddKafkaSubscriber(_services, ServiceLifetime.Singleton);
    }

    /// <inheritdoc/>
    public IServiceCollection AddScopedSubscriber()
    {
        return AddKafkaSubscriber(_services, ServiceLifetime.Scoped);
    }

    /// <inheritdoc/>
    public IServiceCollection AddTransientSubscriber()
    {
        return AddKafkaSubscriber(_services, ServiceLifetime.Transient);
    }

    /// <inheritdoc/>
    public IKafkaConfiguration ConfigureSubscriber(Action<KafkaSubscriberSettings> configurator)
    {
        _services.Configure<KafkaSettings>(settings =>
        {
            configurator(settings.Subscriber);
        });

        return this;
    }

    /// <inheritdoc/>
    public IKafkaConfiguration WithAdvancedConsumerConfig(Action<ConsumerConfig> configurator)
    {
        _services.PostConfigure<ConsumerConfig>(configurator);
        return this;
    }

    /// <inheritdoc/>
    public IKafkaConfiguration WithAdvancedProducerConfig(Action<ProducerConfig> configurator)
    {
        _services.PostConfigure<ProducerConfig>(configurator);
        return this;
    }

    private static IServiceCollection AddKafkaSubscriber(IServiceCollection services, ServiceLifetime lifetime)
    {
        var subscriberDescriptor = new ServiceDescriptor(
            serviceType: typeof(ISubscriber),
            implementationType: typeof(KafkaSubscriber),
            lifetime);

        services.Add(subscriberDescriptor);
        return services;
    }
}
