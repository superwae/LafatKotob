using System.Collections.Concurrent;

namespace Lafatkotob.Services
{
    public interface IUserConnectionsService
    {
        IEnumerable<string> GetUserConnections(string id);
        bool AddUserConnection(string id, string connectionId);
        bool RemoveUserConnection(string id);
    }

    public class UserConnectionsService : IUserConnectionsService
    {
        private readonly ConcurrentDictionary<string, List<string>> _users = new ConcurrentDictionary<string, List<string>>();

        public IEnumerable<string> GetUserConnections(string id)
        {
            if (_users.TryGetValue(id, out List<string>? connections))
            {
                return connections;
            }

            return Enumerable.Empty<string>();
        }

        public bool AddUserConnection(string id, string connectionId)
        {
            if (_users.TryGetValue(id, out List<string>? connections))
            {
                connections.Add(connectionId);
            }
            else
            {
                _ = _users.TryAdd(id, new List<string>() { connectionId });
            }

            return true;
        }


        public bool RemoveUserConnection(string id)
        {
            _ = _users.TryRemove(id, out _);

            return true;
        }
    }
}
