using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cuckoo.Shared;

namespace ChartPro.Services
{
    /// <summary>
    /// IChartDataService implementation that retrieves candles from shared QuoteService storage.
    /// </summary>
    public sealed class QuoteChartDataService : IChartDataService
    {
        private readonly IQuoteService _quoteService;

        public QuoteChartDataService(IQuoteService quoteService)
        {
            _quoteService = quoteService ?? throw new ArgumentNullException(nameof(quoteService));
        }

        public async Task<List<AppQuote>> GetCandlesAsync(string symbol, string timeframe, int count, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var data = await _quoteService.GetAsync(symbol, timeframe) ?? new List<AppQuote>();
            if (count > 0 && data.Count > count)
                return data.Skip(Math.Max(0, data.Count - count)).ToList();
            return data;
        }
    }
}
