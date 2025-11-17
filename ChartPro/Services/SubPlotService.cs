using ScottPlot;
using ScottPlot.Plottables;

namespace ChartPro.Services
{
    public sealed class SubPlotService : ISubPlotService
    {
        private readonly IChartService _chartService;

        public SubPlotService(IChartService chartService)
        {
            _chartService = chartService;
        }

        private void PrepareSubPlot(Plot plt)
        {
            // Clear before applying layout for a consistent baseline
            plt.Clear();

            // Use DateTime X but hide bottom axis (no time labels for subplots)
            plt.Axes.DateTimeTicksBottom();
            plt.Axes.Bottom.IsVisible = false;

            // Match price chart layout: value axis on the right, hide left axis
            plt.Axes.Right.IsVisible = true;
            plt.Axes.Left.IsVisible = false;

            // Use shared padding from ChartService
            plt.Layout.Fixed(_chartService.GetDefaultPadding());
        }

        private static void AssignRight(IPlottable p, Plot plt)
        {
            p.Axes.YAxis = plt.Axes.Right;
            p.Axes.XAxis = plt.Axes.Bottom;
        }

        public void PlotRsi(Plot plt, double[] times, double[] rsi)
        {
            PrepareSubPlot(plt);
            var line = plt.Add.Scatter(times, rsi);
            AssignRight(line, plt);
            plt.Axes.SetLimitsY(0, 100);
            var h70 = plt.Add.HorizontalLine(70); h70.Color = new ScottPlot.Color(0, 160, 0); AssignRight(h70, plt);
            var h30 = plt.Add.HorizontalLine(30); h30.Color = new ScottPlot.Color(200, 0, 0); AssignRight(h30, plt);
            plt.Axes.AutoScale();
        }

        public void PlotMacd(Plot plt, double[] times, double[] macd)
        {
            PrepareSubPlot(plt);
            var line = plt.Add.Scatter(times, macd);
            AssignRight(line, plt);
            var h0 = plt.Add.HorizontalLine(0); h0.Color = new ScottPlot.Color(128, 128, 128); AssignRight(h0, plt);
            plt.Axes.AutoScale();
        }

        public void PlotCci(Plot plt, double[] times, double[] cci)
        {
            PrepareSubPlot(plt);
            var line = plt.Add.Scatter(times, cci);
            AssignRight(line, plt);
            var h100 = plt.Add.HorizontalLine(100); h100.Color = new ScottPlot.Color(0, 160, 0); AssignRight(h100, plt);
            var h0 = plt.Add.HorizontalLine(0); h0.Color = new ScottPlot.Color(128, 128, 128); AssignRight(h0, plt);
            var hm100 = plt.Add.HorizontalLine(-100); hm100.Color = new ScottPlot.Color(200, 0, 0); AssignRight(hm100, plt);
            plt.Axes.AutoScale();
        }

        public void PlotStochRsi(Plot plt, double[] times, double[] stochRsi)
        {
            PrepareSubPlot(plt);
            var line = plt.Add.Scatter(times, stochRsi);
            AssignRight(line, plt);
            plt.Axes.SetLimitsY(0, 100);
            var h80 = plt.Add.HorizontalLine(80); h80.Color = new ScottPlot.Color(0, 160, 0); AssignRight(h80, plt);
            var h20 = plt.Add.HorizontalLine(20); h20.Color = new ScottPlot.Color(200, 0, 0); AssignRight(h20, plt);
            plt.Axes.AutoScale();
        }
    }
}
