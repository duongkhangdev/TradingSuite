using ScottPlot;
using ScottPlot.Plottables;

namespace ChartPro.Services
{
    public interface IChartService
    {
        void ApplyDefaultLayout(Plot plt);
        void AssignPriceAxisRight(IPlottable plottable, Plot plt);
    }
}
