using Cuckoo.Shared;
using ScottPlot;
using ScottPlot.Plottables;
using ScottPlot.WinForms;

namespace ChartPro.Services
{
    public interface IChartService
    {
        // Layout mặc định cho chart giá
        void ApplyDefaultLayout(Plot plt);

        // Gán trục phải cho plottable
        void AssignPriceAxisRight(IPlottable plottable, Plot plt);

        // Padding mặc định dùng chung để các subplot căn đều với chart giá
        PixelPadding GetDefaultPadding();

        Task ApplyBackgroundText(FormsPlot fp, string symbol, string timeFrame);      
        Task<CandlestickPlot?> LoadAndRender(FormsPlot fp, string symbol, string timeFrame, bool hasGap, List<AppQuote>? quotes);
        Task AutoScaleAndRender(FormsPlot fp);
    }
}
