namespace TradingApp.WebApi.Contracts;

public sealed record PriceUpdateDto(
    string Symbol,
    decimal Bid,
    decimal Ask,
    decimal Last,
    decimal Volume,
    DateTime TimestampUtc);

public sealed record OrderUpdateDto(
    Guid OrderId,
    string Symbol,
    string Side,
    decimal Quantity,
    decimal Price,
    string Status,
    DateTime TimestampUtc);

public sealed record PositionUpdateDto(
    string Symbol,
    decimal Quantity,
    decimal AveragePrice,
    decimal UnrealizedPnL,
    decimal RealizedPnL,
    DateTime TimestampUtc);

public sealed record QuoteUpdateDto(
    string Symbol,
    decimal Bid,
    decimal BidSize,
    decimal Ask,
    decimal AskSize,
    DateTime TimestampUtc);

public sealed record AccountUpdateDto(
    string AccountId,
    decimal Balance,
    decimal Equity,
    decimal MarginUsed,
    decimal MarginAvailable,
    DateTime TimestampUtc);
