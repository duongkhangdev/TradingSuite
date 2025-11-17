using Microsoft.Extensions.Logging;
using ChartPro.Services;
using TradingApp.WinUI.Docking;
using ChartPro;

namespace TradingApp.WinUI.Factories
{
    public sealed class ChartDocumentFactory : IChartDocumentFactory
    {
        private readonly IChartDataService _dataService;
        private readonly ILogger<ChartDocument> _logger;
        private readonly IChartService _chartService;

        public ChartDocumentFactory(IChartDataService dataService, ILogger<ChartDocument> logger, IChartService chartService)
        {
            _dataService = dataService;
            _logger = logger;
            _chartService = chartService;
        }

        public ChartDocument Create(string symbol, string timeframe)
            => new ChartDocument(symbol, timeframe, _dataService, _logger, _chartService);

        // helper to create a factory wired to the QuoteService-based data service
        public static IChartDocumentFactory WithQuoteService(IQuoteService quoteService, ILogger<ChartDocument> logger, IChartService chartService)
        {
            var dataSvc = new QuoteChartDataService(quoteService);
            return new ChartDocumentFactory(dataSvc, logger, chartService);
        }
    }
}
