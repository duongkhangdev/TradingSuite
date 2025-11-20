using System;
using System.Buffers;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using TradingApp.WinUI.Contracts;

namespace TradingApp.WinUI.Services;

public sealed class RealtimeConnectionService : IRealtimeConnectionService
{
    private readonly ILogger<RealtimeConnectionService> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private HubConnection? _hubConnection;
    private ClientWebSocket? _webSocket;
    private CancellationTokenSource? _webSocketLoopCts;
    private Task? _webSocketLoopTask;

    public ConnectionStatus Status { get; private set; } = ConnectionStatus.Disconnected;
    public ConnectionTransport? Transport { get; private set; }

    public event EventHandler<ConnectionStatusChangedEventArgs>? StatusChanged;
    public event EventHandler<PriceUpdateDto>? PriceReceived;
    public event EventHandler<OrderUpdateDto>? OrderReceived;
    public event EventHandler<PositionUpdateDto>? PositionReceived;
    public event EventHandler<QuoteUpdateDto>? QuoteReceived;
    public event EventHandler<AccountUpdateDto>? AccountReceived;

    public RealtimeConnectionService(ILogger<RealtimeConnectionService> logger)
    {
        _logger = logger;
    }

    public async Task StartSignalRAsync(string hubUrl, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(hubUrl);

        await StopAsync(cancellationToken).ConfigureAwait(false);

        Transport = ConnectionTransport.SignalR;
        UpdateStatus(ConnectionStatus.Connecting, $"Đang kết nối SignalR: {hubUrl}");

        _hubConnection = new HubConnectionBuilder()
            .WithUrl(hubUrl)
            .WithAutomaticReconnect()
            .Build();

        _hubConnection.Closed += OnHubClosed;
        _hubConnection.Reconnected += OnHubReconnected;
        _hubConnection.Reconnecting += OnHubReconnecting;
        AttachSignalRHandlers(_hubConnection);

        try
        {
            await _hubConnection.StartAsync(cancellationToken).ConfigureAwait(false);
            UpdateStatus(ConnectionStatus.Connected, "Đã kết nối SignalR");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Không thể kết nối SignalR");
            UpdateStatus(ConnectionStatus.Faulted, ex.Message);
            throw;
        }
    }

    public async Task StartWebSocketAsync(string socketUrl, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(socketUrl);

        await StopAsync(cancellationToken).ConfigureAwait(false);

        Transport = ConnectionTransport.WebSocket;
        UpdateStatus(ConnectionStatus.Connecting, $"Đang kết nối WebSocket: {socketUrl}");

        _webSocket = new ClientWebSocket();

        try
        {
            await _webSocket.ConnectAsync(new Uri(socketUrl), cancellationToken).ConfigureAwait(false);
            UpdateStatus(ConnectionStatus.Connected, "Đã kết nối WebSocket");

            _webSocketLoopCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _webSocketLoopTask = Task.Run(() => ListenWebSocketAsync(_webSocketLoopCts.Token));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Không thể kết nối WebSocket");
            UpdateStatus(ConnectionStatus.Faulted, ex.Message);
            throw;
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        var tasks = new List<Task>();

        if (_hubConnection != null)
        {
            tasks.Add(_hubConnection.StopAsync(cancellationToken));
            tasks.Add(_hubConnection.DisposeAsync().AsTask());
            _hubConnection = null;
        }

        if (_webSocket != null)
        {
            try
            {
                if (_webSocket.State == WebSocketState.Open)
                {
                    tasks.Add(_webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Stop", cancellationToken));
                }
            }
            catch (WebSocketException ex)
            {
                _logger.LogWarning(ex, "Lỗi khi đóng WebSocket");
            }
            finally
            {
                _webSocket.Dispose();
                _webSocket = null;
            }
        }

        if (_webSocketLoopCts != null)
        {
            _webSocketLoopCts.Cancel();
            _webSocketLoopCts.Dispose();
            _webSocketLoopCts = null;
        }

        if (_webSocketLoopTask != null)
        {
            tasks.Add(_webSocketLoopTask);
            _webSocketLoopTask = null;
        }

        if (tasks.Count > 0)
        {
            try
            {
                await Task.WhenAll(tasks).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // ignored
            }
        }

        Transport = null;
        UpdateStatus(ConnectionStatus.Disconnected, "Đã ngắt kết nối");
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync();
    }

    private Task OnHubClosed(Exception? exception)
    {
        if (exception != null)
        {
            _logger.LogWarning(exception, "SignalR hub closed do lỗi");
            UpdateStatus(ConnectionStatus.Faulted, exception.Message);
        }
        else
        {
            UpdateStatus(ConnectionStatus.Disconnected, "SignalR đóng kết nối");
        }

        return Task.CompletedTask;
    }

    private Task OnHubReconnecting(Exception? exception)
    {
        if (exception != null)
        {
            _logger.LogWarning(exception, "SignalR reconnecting do lỗi");
        }

        UpdateStatus(ConnectionStatus.Connecting, "SignalR đang khôi phục kết nối");
        return Task.CompletedTask;
    }

    private Task OnHubReconnected(string? connectionId)
    {
        UpdateStatus(ConnectionStatus.Connected, "SignalR đã khôi phục kết nối");
        return Task.CompletedTask;
    }

    private void AttachSignalRHandlers(HubConnection hubConnection)
    {
        hubConnection.On<PriceUpdateDto>("ReceivePrice", update => RaisePrice(update));
        hubConnection.On<OrderUpdateDto>("ReceiveOrder", update => RaiseOrder(update));
        hubConnection.On<PositionUpdateDto>("ReceivePosition", update => RaisePosition(update));
        hubConnection.On<QuoteUpdateDto>("ReceiveQuote", update => RaiseQuote(update));
        hubConnection.On<AccountUpdateDto>("ReceiveAccount", update => RaiseAccount(update));
    }

    private async Task ListenWebSocketAsync(CancellationToken cancellationToken)
    {
        var socket = _webSocket;
        if (socket == null)
            return;

        var buffer = ArrayPool<byte>.Shared.Rent(8 * 1024);

        try
        {
            while (!cancellationToken.IsCancellationRequested && socket.State == WebSocketState.Open)
            {
                var result = await socket.ReceiveAsync(buffer.AsMemory(), cancellationToken).ConfigureAwait(false);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    UpdateStatus(ConnectionStatus.Disconnected, "WebSocket đóng kết nối");
                    break;
                }

                if (result.Count > 0 && result.EndOfMessage)
                {
                    var text = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    _logger.LogDebug("WebSocket message: {Message}", text);
                    DispatchWebSocketMessage(text);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // loop cancelled
        }
        catch (WebSocketException ex)
        {
            _logger.LogError(ex, "WebSocket receive loop fault");
            UpdateStatus(ConnectionStatus.Faulted, ex.Message);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    private void DispatchWebSocketMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message) || message[0] != '{')
        {
            return;
        }

        try
        {
            var envelope = JsonSerializer.Deserialize<WebSocketEnvelope>(message, JsonOptions);
            if (envelope?.Type is null)
                return;

            switch (envelope.Type.ToLowerInvariant())
            {
                case "price":
                    if (TryDeserialize(envelope.Payload, out PriceUpdateDto price))
                        RaisePrice(price);
                    break;
                case "order":
                    if (TryDeserialize(envelope.Payload, out OrderUpdateDto order))
                        RaiseOrder(order);
                    break;
                case "position":
                    if (TryDeserialize(envelope.Payload, out PositionUpdateDto position))
                        RaisePosition(position);
                    break;
                case "quote":
                    if (TryDeserialize(envelope.Payload, out QuoteUpdateDto quote))
                        RaiseQuote(quote);
                    break;
                case "account":
                    if (TryDeserialize(envelope.Payload, out AccountUpdateDto account))
                        RaiseAccount(account);
                    break;
            }
        }
        catch (JsonException ex)
        {
            _logger.LogDebug(ex, "Không parse được message realtime");
        }
    }

    private sealed record WebSocketEnvelope(string? Type, JsonElement Payload);

    private static bool TryDeserialize<T>(JsonElement element, out T value) where T : class
    {
        try
        {
            value = element.Deserialize<T>(JsonOptions)!;
            return value is not null;
        }
        catch
        {
            value = default!;
            return false;
        }
    }

    private void UpdateStatus(ConnectionStatus status, string message)
    {
        Status = status;
        StatusChanged?.Invoke(this, new ConnectionStatusChangedEventArgs(status, message, Transport));
    }

    private void RaisePrice(PriceUpdateDto update) => PriceReceived?.Invoke(this, update);
    private void RaiseOrder(OrderUpdateDto update) => OrderReceived?.Invoke(this, update);
    private void RaisePosition(PositionUpdateDto update) => PositionReceived?.Invoke(this, update);
    private void RaiseQuote(QuoteUpdateDto update) => QuoteReceived?.Invoke(this, update);
    private void RaiseAccount(AccountUpdateDto update) => AccountReceived?.Invoke(this, update);
}
