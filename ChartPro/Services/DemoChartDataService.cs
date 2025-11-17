using Cuckoo.Shared;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ChartPro.Services
{
    public sealed class DemoChartDataService : IChartDataService
    {
        public Task<List<AppQuote>> GetCandlesAsync(string symbol, string timeframe, int count, CancellationToken ct)
        {
            var rnd = new Random(symbol.GetHashCode() ^ timeframe.GetHashCode());
            double price = 100;
            var list = new List<AppQuote>(count);
            var start = DateTime.UtcNow.AddMinutes(-count);

            for (int i = 0; i < count; i++)
            {
                ct.ThrowIfCancellationRequested();

                var t = start.AddMinutes(i);
                var delta = (rnd.NextDouble() - 0.5) * 0.8;
                var open = price;
                price = Math.Max(1, price + delta);
                var close = price;
                var high = Math.Max(open, close) + rnd.NextDouble() * 0.4;
                var low = Math.Min(open, close) - rnd.NextDouble() * 0.4;

                list.Add(new AppQuote
                {
                    Date = t,
                    Open = (decimal)open,
                    High = (decimal)high,
                    Low = (decimal)low,
                    Close = (decimal)close,
                    Volume = rnd.Next(100, 5000)
                });
            }

            return Task.FromResult<List<AppQuote>>(list);
        }
    }
}
