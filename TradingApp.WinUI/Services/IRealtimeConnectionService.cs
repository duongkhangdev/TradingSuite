using System;
using System.Threading;
using System.Threading.Tasks;
using TradingApp.WinUI.Contracts;

namespace TradingApp.WinUI.Services;

public enum ConnectionTransport
{
    SignalR,
    WebSocket
}

public enum ConnectionStatus
{
    Disconnected,
    Connecting,
    Connected,
    Faulted
}

public sealed record ConnectionStatusChangedEventArgs(
    ConnectionStatus Status,
    string Message,
    ConnectionTransport? Transport);

public interface IRealtimeConnectionService : IAsyncDisposable
{
    ConnectionStatus Status { get; }
    ConnectionTransport? Transport { get; }

    event EventHandler<ConnectionStatusChangedEventArgs>? StatusChanged;
    event EventHandler<PriceUpdateDto>? PriceReceived;
    event EventHandler<OrderUpdateDto>? OrderReceived;
    event EventHandler<PositionUpdateDto>? PositionReceived;
    event EventHandler<QuoteUpdateDto>? QuoteReceived;
    event EventHandler<AccountUpdateDto>? AccountReceived;

    Task StartSignalRAsync(string hubUrl, CancellationToken cancellationToken = default);

    Task StartWebSocketAsync(string socketUrl, CancellationToken cancellationToken = default);

    Task StopAsync(CancellationToken cancellationToken = default);
}
