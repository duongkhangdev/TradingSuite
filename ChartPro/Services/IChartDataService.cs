using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ChartPro.Services
{
    public sealed class Candle
    {
        public DateTime Time { get; init; }
        public double Open { get; init; }
        public double High { get; init; }
        public double Low { get; init; }
        public double Close { get; init; }
        public double Volume { get; init; }
    }

    public interface IChartDataService
    {
        Task<IReadOnlyList<Candle>> GetCandlesAsync(string symbol, string timeframe, int count, CancellationToken ct);
    }
}
