using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SharpNest.Kafka.Core;
using SharpNest.Kafka.Core.Abstractions;
using SharpNest.Kafka.Core.Settings;
using SharpNest.Utils.Serialization;

namespace SharpNest.Kafka;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Kafka services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>A configuration object for further customization.</returns>
    public static IKafkaConfiguration AddKafka(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<KafkaSettings>(configuration.GetSection("Kafka"));

        services.AddSingleton<KafkaConnection>();
        services.AddSingleton<IPublisher, KafkaPublisher>();
        RegisterSerializer(services);

        return new KafkaConfiguration(services);
    }

    /// <summary>
    /// Adds Kafka services to the service collection with programmatic configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureSettings">Action to configure Kafka settings.</param>
    /// <returns>A configuration object for further customization.</returns>
    public static IKafkaConfiguration AddKafka(this IServiceCollection services, Action<KafkaSettings> configureSettings)
    {
        services.Configure(configureSettings);

        services.AddSingleton<KafkaConnection>();
        services.AddSingleton<KafkaConfigBuilder>();
        RegisterSerializer(services);

        return new KafkaConfiguration(services);
    }

    private static void RegisterSerializer(IServiceCollection services)
    {
        if (!services.Any(serviceDescriptor => serviceDescriptor.ServiceType == typeof(ISerializer)))
        {
            services.AddSingleton<ISerializer, SystemTextJsonSerializer>();
        }
    }
}
