using System.Collections.ObjectModel;
using System.Net.WebSockets;

namespace TradingApp.WebApi.Services;

public record WebSocketConnectionInfo(string ConnectionId, DateTime ConnectedAtUtc, string? RemoteEndpoint);

public interface IWebSocketConnectionManager
{
    ReadOnlyCollection<WebSocketConnectionInfo> Connections { get; }

    Task<string> RegisterAsync(WebSocket socket, string? remoteEndpoint, CancellationToken cancellationToken = default);

    Task RemoveAsync(string connectionId, WebSocketCloseStatus closeStatus = WebSocketCloseStatus.NormalClosure, string? description = null);

    Task SendAsync(string connectionId, string message, CancellationToken cancellationToken = default);

    Task BroadcastAsync(string message, CancellationToken cancellationToken = default);
}
