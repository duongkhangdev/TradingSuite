using Cuckoo.Shared;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ChartPro.Services
{
    public interface IChartDataService
    {
        Task<List<AppQuote>> GetCandlesAsync(string symbol, string timeframe, int count, CancellationToken ct);
    }
}
