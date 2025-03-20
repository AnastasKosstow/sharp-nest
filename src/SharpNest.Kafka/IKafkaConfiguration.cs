using Microsoft.Extensions.DependencyInjection;
using SharpNest.Kafka.Core.Settings;

namespace SharpNest.Kafka;

public interface IKafkaConfiguration
{
    /// <summary>
    /// Adds a Kafka producer to the service collection.
    /// </summary>
    IServiceCollection AddPublisher();

    /// <summary>
    /// Configures the Kafka publisher settings programmatically.
    /// </summary>
    /// <param name="configurator">Action to configure publisher settings.</param>
    IKafkaConfiguration ConfigurePublisher(Action<KafkaPublisherSettings> configurator);

    /// <summary>
    /// Adds a singleton Kafka consumer to the service collection.
    /// </summary>
    IServiceCollection AddSingletonSubscriber();

    /// <summary>
    /// Adds a scoped Kafka consumer to the service collection.
    /// </summary>
    IServiceCollection AddScopedSubscriber();

    /// <summary>
    /// Adds a transient Kafka consumer to the service collection.
    /// </summary>
    IServiceCollection AddTransientSubscriber();

    /// <summary>
    /// Configures the Kafka subscriber settings programmatically.
    /// </summary>
    /// <param name="configurator">Action to configure subscriber settings.</param>
    IKafkaConfiguration ConfigureSubscriber(Action<KafkaSubscriberSettings> configurator);
}
