using Microsoft.AspNetCore.Mvc;
using SharpNest.SSE.Core.Abstractions;
using System.Text.Json;
using System.Text;

namespace SharpNest.Samples.SSE.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class NotificationsController(ISSEMessageHubService hub) : ControllerBase
    {
        private readonly ISSEMessageHubService _hub = hub;

        [HttpGet("stream")]
        public async Task Stream(CancellationToken cancellationToken)
        {
            Response.Headers.ContentType = "text/event-stream";
            Response.Headers.CacheControl = "no-cache";
            Response.Headers.Connection = "keep-alive";

            var clientClosedToken = HttpContext.RequestAborted;
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(clientClosedToken, cancellationToken);

            try
            {
                await foreach (var message in _hub.SubscribeAsync(linkedCts.Token))
                {
                    if (linkedCts.Token.IsCancellationRequested)
                    {
                        break;
                    }

                    await WriteMessageAsync(HttpContext, message, linkedCts.Token);

                    await Response.Body.FlushAsync(linkedCts.Token);
                }
            }
            finally
            {
                linkedCts.Dispose();
            }
        }

        private async Task WriteMessageAsync(HttpContext context, IMessage message, CancellationToken cancellationToken)
        {
            var writer = context.Response.Body;
            if (!string.IsNullOrEmpty(message.Id))
            {
                await writer.WriteAsync(Encoding.UTF8.GetBytes($"id: {message.Id}\n"), cancellationToken);
            }
            if (message.Metadata != null && message.Metadata.TryGetValue("event", out string eventType))
            {
                await writer.WriteAsync(Encoding.UTF8.GetBytes($"event: {eventType}\n"), cancellationToken);
            }

            string data = JsonSerializer.Serialize(message.Payload);
            await writer.WriteAsync(Encoding.UTF8.GetBytes($"data: {data}\n\n"), cancellationToken);
        }
    }
}
