using Cuckoo.Shared;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace ChartPro
{
    public interface IQuoteService
    {
        // Events for consumers to react to updates
        event Action<string, string, List<AppQuote>>? QuotesAddedOrUpdated; // symbol, timeframe, quotes
        event Action<string>? SymbolRemoved; // symbol removed entirely

        void AddOrUpdate(string symbol, string time_frame, List<AppQuote> model);
        bool Remove(string symbol);
        Task<List<AppQuote>?> GetAllAsync();
        Task<List<AppQuote>?> GetAsync(string symbol, string time_frame);
        Task<AppQuote?> GetLastAsync(string symbol, string time_frame);
        bool ContainsKey(string symbol);
        bool ContainsKey(string symbol, string time_frame);
        int Count();

        // DI-friendly helpers
        IEnumerable<string> GetSymbols();
        IEnumerable<string> GetTimeFrames(string symbol);
        IReadOnlyDictionary<string, IReadOnlyDictionary<string, List<AppQuote>>> Snapshot();
    }

    /// <summary>
    /// Thread-safe in-memory quote storage service.
    /// Register as a singleton in DI container for shared state:
    /// services.AddSingleton<IQuoteService, QuoteService>();
    /// </summary>
    public class QuoteService : IQuoteService
    {
        // symbol -> timeframe -> quotes
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, List<AppQuote>>> _quotes = new();
        private readonly ILogger<QuoteService> _logger;

        public event Action<string, string, List<AppQuote>>? QuotesAddedOrUpdated;
        public event Action<string>? SymbolRemoved;

        public QuoteService(ILogger<QuoteService> logger)
        {
            _logger = logger;
        }

        public void AddOrUpdate(string symbol, string time_frame, List<AppQuote> model)
        {
            if (string.IsNullOrWhiteSpace(symbol) || string.IsNullOrWhiteSpace(time_frame) || model is null)
                return;

            var tfDict = _quotes.GetOrAdd(symbol, _ =>
            {
                _logger.LogInformation("QuoteService: Added new symbol {Symbol}.", symbol);
                return new ConcurrentDictionary<string, List<AppQuote>>();
            });

            // Add new timeframe or update existing
            tfDict.AddOrUpdate(time_frame,
                _ =>
                {
                    _logger.LogInformation("QuoteService: Added timeframe {TF} for symbol {Symbol} with {Count} records.", time_frame, symbol, model.Count);
                    QuotesAddedOrUpdated?.Invoke(symbol, time_frame, model);
                    return model;
                },
                (_, existing) =>
                {
                    _logger.LogInformation("QuoteService: Updated timeframe {TF} for symbol {Symbol} from {OldCount} to {NewCount} records.", time_frame, symbol, existing.Count, model.Count);
                    QuotesAddedOrUpdated?.Invoke(symbol, time_frame, model);
                    return model;
                });
        }

        public bool Remove(string symbol)
        {
            if (_quotes.TryRemove(symbol, out _))
            {
                _logger.LogInformation("QuoteService: Removed symbol {Symbol}.", symbol);
                SymbolRemoved?.Invoke(symbol);
                return true;
            }
            return false;
        }

        public async Task<List<AppQuote>?> GetAllAsync()
        {
            // Flatten all quotes (could be large; caller should be aware)
            var all = _quotes.Values
                .SelectMany(tf => tf.Values)
                .SelectMany(list => list)
                .ToList();
            return await Task.FromResult(all);
        }

        public async Task<List<AppQuote>?> GetAsync(string symbol, string time_frame)
        {
            if (!_quotes.TryGetValue(symbol, out var tfDict))
                return null;
            if (!tfDict.TryGetValue(time_frame, out var list))
                return null;
            return await Task.FromResult(list);
        }

        public async Task<AppQuote?> GetLastAsync(string symbol, string time_frame)
        {
            var list = await GetAsync(symbol, time_frame);
            return list?.LastOrDefault();
        }

        public bool ContainsKey(string symbol) => _quotes.ContainsKey(symbol);

        public bool ContainsKey(string symbol, string time_frame)
            => _quotes.TryGetValue(symbol, out var tfDict) && tfDict.ContainsKey(time_frame);

        public int Count() => _quotes.Count;

        public IEnumerable<string> GetSymbols() => _quotes.Keys;

        public IEnumerable<string> GetTimeFrames(string symbol)
            => _quotes.TryGetValue(symbol, out var tfDict) ? tfDict.Keys : Enumerable.Empty<string>();

        public IReadOnlyDictionary<string, IReadOnlyDictionary<string, List<AppQuote>>> Snapshot()
            => _quotes.ToDictionary(k => k.Key, v => (IReadOnlyDictionary<string, List<AppQuote>>)v.Value.ToDictionary(t => t.Key, t => t.Value));
    }
}
