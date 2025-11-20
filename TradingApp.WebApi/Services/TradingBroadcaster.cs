using Microsoft.AspNetCore.SignalR;
using TradingApp.WebApi.Contracts;
using TradingApp.WebApi.Hubs;

namespace TradingApp.WebApi.Services;

public sealed class TradingBroadcaster : ITradingBroadcaster
{
    private readonly IHubContext<TradingHub, ITradingClient> _hubContext;

    public TradingBroadcaster(IHubContext<TradingHub, ITradingClient> hubContext)
    {
        _hubContext = hubContext;
    }

    public Task BroadcastPriceAsync(PriceUpdateDto update, CancellationToken cancellationToken = default)
        => _hubContext.Clients.All.ReceivePrice(update);

    public Task BroadcastOrderAsync(OrderUpdateDto update, CancellationToken cancellationToken = default)
        => _hubContext.Clients.All.ReceiveOrder(update);

    public Task BroadcastPositionAsync(PositionUpdateDto update, CancellationToken cancellationToken = default)
        => _hubContext.Clients.All.ReceivePosition(update);

    public Task BroadcastQuoteAsync(QuoteUpdateDto update, CancellationToken cancellationToken = default)
        => _hubContext.Clients.All.ReceiveQuote(update);

    public Task BroadcastAccountAsync(AccountUpdateDto update, CancellationToken cancellationToken = default)
        => _hubContext.Clients.All.ReceiveAccount(update);
}
