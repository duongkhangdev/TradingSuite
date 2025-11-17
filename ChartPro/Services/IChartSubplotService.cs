using Cuckoo.Shared;
using ScottPlot;
using ScottPlot.Plottables;
using ScottPlot.WinForms;
using System.Threading.Tasks;

namespace ChartPro.Services
{
    public interface IChartSubplotService
    {
        void PrepareSubPlot(FormsPlot fp);
        Task PlotRsi(FormsPlot fp, CandlestickPlot candlePlot, List<AppQuote> quotes, string symbol, string timeframe);
        Task PlotMacd(FormsPlot fp, CandlestickPlot candlePlot, List<AppQuote> quotes, string symbol, string timeframe);
        Task PlotCci(FormsPlot fp, CandlestickPlot candlePlot, List<AppQuote> quotes, string symbol, string timeframe);
        Task PlotStochRsi(FormsPlot fp, CandlestickPlot candlePlot, List<AppQuote> quotes, string symbol, string timeframe);

        // New high-level API similar to IChartService.LoadAndRender for subplots
        Task LoadAndRender(FormsPlot fp, string symbol, string timeFrame, string indicatorName);
    }
}
