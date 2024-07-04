using Lafatkotob.Entities;

namespace Lafatkotob.Hubs
{
    public interface IChatHub
    {
        Task SendNotification(Notification notification, List<string> userIds);
    }
}
