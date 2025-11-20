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
        // New high-level API similar to IChartService.LoadAndRender for subplots
        Task LoadAndRender(FormsPlot fp, CandlestickPlot? candlePlot, List<AppQuote>? quotes, string symbol, string timeFrame, string indicatorName);
    }
}
