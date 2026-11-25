using Microsoft.Extensions.Logging;
using PetroVision.BLL.Interfaces;

namespace PetroVision.BLL.Services;

internal static class EventNotifierExtensions
{
    // Real-time events are best effort: building the payload or sending it must never
    // fail the HTTP request or the detection that already saved its changes.
    public static async Task TryNotifyAsync(this IEventNotifier notifier, ILogger logger, string eventName,
        Func<IEventNotifier, Task> send)
    {
        try
        {
            await send(notifier);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Real-time event {Event} could not be sent", eventName);
        }
    }
}
