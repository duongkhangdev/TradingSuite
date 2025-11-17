using ScottPlot;
using ScottPlot.Plottables;

namespace ChartPro.Services
{
    public sealed class ChartService : IChartService
    {
        public void ApplyDefaultLayout(Plot plt)
        {
            plt.Axes.DateTimeTicksBottom();

            var right = plt.Axes.Right;
            right.IsVisible = true;
            plt.Axes.Left.IsVisible = false;

            plt.Grid.MajorLineColor = new ScottPlot.Color(40, 40, 40);
            plt.Grid.MinorLineColor = new ScottPlot.Color(25, 25, 25);

            plt.Layout.Fixed(new PixelPadding(60, 10, 10, 30));
        }

        public void AssignPriceAxisRight(IPlottable plottable, Plot plt)
        {
            plottable.Axes.YAxis = plt.Axes.Right;
            plottable.Axes.XAxis = plt.Axes.Bottom;
        }
    }
}
