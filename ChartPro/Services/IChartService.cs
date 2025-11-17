using ScottPlot;
using ScottPlot.Plottables;

namespace ChartPro.Services
{
    public interface IChartService
    {
        // Layout m?c ??nh cho chart giá
        void ApplyDefaultLayout(Plot plt);

        // Gán tr?c ph?i cho plottable
        void AssignPriceAxisRight(IPlottable plottable, Plot plt);

        // Padding m?c ??nh dùng chung ?? các subplot c?n ??u v?i chart giá
        PixelPadding GetDefaultPadding();
    }
}
