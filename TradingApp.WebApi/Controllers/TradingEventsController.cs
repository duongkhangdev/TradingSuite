using Microsoft.AspNetCore.Mvc;
using TradingApp.WebApi.Contracts;
using TradingApp.WebApi.Services;

namespace TradingApp.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class TradingEventsController : ControllerBase
{
    private readonly ITradingBroadcaster _broadcaster;
    private readonly ILogger<TradingEventsController> _logger;

    public TradingEventsController(ITradingBroadcaster broadcaster, ILogger<TradingEventsController> logger)
    {
        _broadcaster = broadcaster;
        _logger = logger;
    }

    [HttpPost("price")]
    public async Task<IActionResult> PublishPrice([FromBody] PriceUpdateDto update, CancellationToken cancellationToken)
    {
        await _broadcaster.BroadcastPriceAsync(update, cancellationToken);
        _logger.LogDebug("Broadcasted price update for {Symbol}", update.Symbol);
        return Accepted();
    }

    [HttpPost("order")]
    public async Task<IActionResult> PublishOrder([FromBody] OrderUpdateDto update, CancellationToken cancellationToken)
    {
        await _broadcaster.BroadcastOrderAsync(update, cancellationToken);
        _logger.LogDebug("Broadcasted order update {OrderId}", update.OrderId);
        return Accepted();
    }

    [HttpPost("position")]
    public async Task<IActionResult> PublishPosition([FromBody] PositionUpdateDto update, CancellationToken cancellationToken)
    {
        await _broadcaster.BroadcastPositionAsync(update, cancellationToken);
        _logger.LogDebug("Broadcasted position update for {Symbol}", update.Symbol);
        return Accepted();
    }

    [HttpPost("quote")]
    public async Task<IActionResult> PublishQuote([FromBody] QuoteUpdateDto update, CancellationToken cancellationToken)
    {
        await _broadcaster.BroadcastQuoteAsync(update, cancellationToken);
        _logger.LogDebug("Broadcasted quote update for {Symbol}", update.Symbol);
        return Accepted();
    }

    [HttpPost("account")]
    public async Task<IActionResult> PublishAccount([FromBody] AccountUpdateDto update, CancellationToken cancellationToken)
    {
        await _broadcaster.BroadcastAccountAsync(update, cancellationToken);
        _logger.LogDebug("Broadcasted account update for {AccountId}", update.AccountId);
        return Accepted();
    }
}
