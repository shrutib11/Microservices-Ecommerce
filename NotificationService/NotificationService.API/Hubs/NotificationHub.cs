using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace NotificationService.API.Hubs;

public class NotificationHub : Hub
{
    private static readonly Dictionary<string, List<string>> _userConnections = new();

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var role = Context.User?.FindFirst(ClaimTypes.Role)?.Value;

        if (!string.IsNullOrEmpty(userId))
        {
            lock (_userConnections)
            {
                if (!_userConnections.ContainsKey(userId))
                    _userConnections[userId] = new List<string>();

                _userConnections[userId].Add(Context.ConnectionId);
            }
        }

        if (role == "Admin")
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "Admins");
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
            lock (_userConnections)
            {
                if (_userConnections.ContainsKey(userId))
                {
                    _userConnections[userId].Remove(Context.ConnectionId);

                    if (_userConnections[userId].Count == 0)
                        _userConnections.Remove(userId);
                }
            }
        }

        await base.OnDisconnectedAsync(exception);
    }

    public static IReadOnlyDictionary<string, HashSet<string>> GetConnections()
    {
        return _userConnections.ToDictionary(
            pair => pair.Key,
            pair => pair.Value.ToHashSet()
        );
    }
}
