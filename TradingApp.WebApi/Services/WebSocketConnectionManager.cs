using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Net.WebSockets;
using System.Text;
using System.Linq;

namespace TradingApp.WebApi.Services;

public sealed class WebSocketConnectionManager : IWebSocketConnectionManager
{
    private readonly ConcurrentDictionary<string, ManagedWebSocket> _connections = new();

    public ReadOnlyCollection<WebSocketConnectionInfo> Connections => _connections.Values
        .Select(connection => connection.Info)
        .OrderBy(info => info.ConnectedAtUtc)
        .ToList()
        .AsReadOnly();

    public Task<string> RegisterAsync(WebSocket socket, string? remoteEndpoint, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var connectionId = Guid.NewGuid().ToString("N");
        var info = new WebSocketConnectionInfo(connectionId, DateTime.UtcNow, remoteEndpoint);
        var managed = new ManagedWebSocket(socket, info);

        if (!_connections.TryAdd(connectionId, managed))
        {
            throw new InvalidOperationException("Unable to register websocket connection.");
        }

        return Task.FromResult(connectionId);
    }

    public async Task RemoveAsync(string connectionId, WebSocketCloseStatus closeStatus = WebSocketCloseStatus.NormalClosure, string? description = null)
    {
        if (_connections.TryRemove(connectionId, out var managed))
        {
            try
            {
                if (managed.Socket.State is WebSocketState.Open or WebSocketState.CloseReceived)
                {
                    var closeMessage = description ?? "Closed by server";
                    await managed.Socket.CloseAsync(closeStatus, closeMessage, CancellationToken.None);
                }
            }
            catch (WebSocketException)
            {
                // Ignored because connection is being terminated anyway.
            }
            finally
            {
                managed.Socket.Dispose();
            }
        }
    }

    public Task SendAsync(string connectionId, string message, CancellationToken cancellationToken = default)
    {
        if (!_connections.TryGetValue(connectionId, out var managed))
        {
            throw new KeyNotFoundException($"Connection '{connectionId}' was not found.");
        }

        return SendInternalAsync(managed.Socket, message, cancellationToken);
    }

    public async Task BroadcastAsync(string message, CancellationToken cancellationToken = default)
    {
        foreach (var managed in _connections.Values)
        {
            await SendInternalAsync(managed.Socket, message, cancellationToken);
        }
    }

    private static Task SendInternalAsync(WebSocket socket, string message, CancellationToken cancellationToken)
    {
        if (socket.State != WebSocketState.Open)
        {
            return Task.CompletedTask;
        }

        var payload = Encoding.UTF8.GetBytes(message);
        return socket.SendAsync(payload, WebSocketMessageType.Text, true, cancellationToken);
    }

    private sealed record ManagedWebSocket(WebSocket Socket, WebSocketConnectionInfo Info);
}
