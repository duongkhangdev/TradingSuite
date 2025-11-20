using Microsoft.AspNetCore.SignalR;
using TradingApp.WebApi.Contracts;

namespace TradingApp.WebApi.Hubs;

public interface ITradingClient
{
    Task ReceivePrice(PriceUpdateDto priceUpdate);
    Task ReceiveOrder(OrderUpdateDto orderUpdate);
    Task ReceivePosition(PositionUpdateDto positionUpdate);
    Task ReceiveQuote(QuoteUpdateDto quoteUpdate);
    Task ReceiveAccount(AccountUpdateDto accountUpdate);
}

public sealed class TradingHub : Hub<ITradingClient>
{
    private readonly ILogger<TradingHub> _logger;

    public TradingHub(ILogger<TradingHub> logger)
    {
        _logger = logger;
    }

    public override Task OnConnectedAsync()
    {
        _logger.LogInformation("Client {ConnectionId} connected to TradingHub", Context.ConnectionId);
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        if (exception is not null)
        {
            _logger.LogWarning(exception, "Client {ConnectionId} disconnected abnormally", Context.ConnectionId);
        }
        else
        {
            _logger.LogInformation("Client {ConnectionId} disconnected", Context.ConnectionId);
        }

        return base.OnDisconnectedAsync(exception);
    }
}
