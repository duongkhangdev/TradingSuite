using ScottPlot;
using ScottPlot.Plottables;

namespace ChartPro.Services
{
    public sealed class ChartService : IChartService
    {
        private static readonly PixelPadding DefaultPadding = new PixelPadding(60, 10, 10, 30);

        public void ApplyDefaultLayout(Plot plt)
        {
            // X d??i d?ng th?i gian
            plt.Axes.DateTimeTicksBottom();

            // Tr?c giá bên ph?i
            var right = plt.Axes.Right;
            right.IsVisible = true;

            // ?n tr?c trái ?? tránh trùng l?p
            plt.Axes.Left.IsVisible = false;

            // L??i nh?
            plt.Grid.MajorLineColor = new ScottPlot.Color(40, 40, 40);
            plt.Grid.MinorLineColor = new ScottPlot.Color(25, 25, 25);

            // Padding: Left, Right, Top, Bottom
            plt.Layout.Fixed(DefaultPadding);
        }

        public void AssignPriceAxisRight(IPlottable plottable, Plot plt)
        {
            // Gán tr?c Y c?a plottable sang Right Axis
            plottable.Axes.YAxis = plt.Axes.Right;
            plottable.Axes.XAxis = plt.Axes.Bottom;
        }

        public PixelPadding GetDefaultPadding() => DefaultPadding;
    }
}
