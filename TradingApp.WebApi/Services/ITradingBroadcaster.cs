using TradingApp.WebApi.Contracts;

namespace TradingApp.WebApi.Services;

public interface ITradingBroadcaster
{
    Task BroadcastPriceAsync(PriceUpdateDto update, CancellationToken cancellationToken = default);
    Task BroadcastOrderAsync(OrderUpdateDto update, CancellationToken cancellationToken = default);
    Task BroadcastPositionAsync(PositionUpdateDto update, CancellationToken cancellationToken = default);
    Task BroadcastQuoteAsync(QuoteUpdateDto update, CancellationToken cancellationToken = default);
    Task BroadcastAccountAsync(AccountUpdateDto update, CancellationToken cancellationToken = default);
}
