using SharpNest.Samples.SSE.Configuration;
using SharpNest.SSE;
using SharpNest.SSE.Core.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddServerSentEvent()
    .Configure(options =>
    {
        options.ChannelCapacity = 500;
        options.WriteTimeout = TimeSpan.FromSeconds(2);
        options.SlowConsumerStrategy = SlowConsumerStrategy.DropMessages;
    })
    .WithSource<NotificationSource>();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(o =>
    {
        o.SwaggerEndpoint("/openapi/v1.json", "v1");
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
