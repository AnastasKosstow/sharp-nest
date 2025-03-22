using SharpNest.Kafka;
using SharpNest.Kafka.Core.Abstractions;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddKafka(builder.Configuration)
    .AddPublisher()
    .AddSingletonSubscriber();

builder.Services.AddHostedService<KafkaConsumerService>();

var app = builder.Build();

app.UseHttpsRedirection();

app.MapPost("/publish", async (IPublisher publisher) =>
{
    var result = await publisher.PublishAsync(
        "messages-topic",
        request.Key,
        request.Content,
        CancellationToken.None
    );

    return Results.Ok(
        new
        {
            Topic = result.Topic,
            Partition = result.Partition,
            Timestamp = result.Timestamp,
            Success = result.Success
        });
});

app.Run();
