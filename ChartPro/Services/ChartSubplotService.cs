using Cuckoo.Shared;
using ScottPlot;
using ScottPlot.Plottables;
using ScottPlot.WinForms;
using System;
using System.Threading.Tasks;

namespace ChartPro.Services
{
    public sealed class ChartSubplotService : IChartSubplotService
    {
        private readonly IChartService _chartService;
        private readonly IChartTechnicalService _tech;

        public ChartSubplotService(IChartService chartService, IChartTechnicalService tech)
        {
            _chartService = chartService;
            _tech = tech;
        }

        public void PrepareSubPlot(FormsPlot fp)
        {
            var plt = fp.Plot;

            // Clear before applying layout for a consistent baseline
            plt.Clear();

            // Use DateTime X but hide bottom axis (no time labels for subplots)
            plt.Axes.DateTimeTicksBottom();
            plt.Axes.Bottom.IsVisible = false;

            // Match price chart layout: value axis on the right, hide left axis
            plt.Axes.Right.IsVisible = true;
            plt.Axes.Left.IsVisible = false;

            // Hide grid for subplots
            plt.Grid.IsVisible = false;
            // Also ensure axis-aligned grid styles are hidden (ScottPlot v5)
            plt.Grid.XAxisStyle.IsVisible = false;
            plt.Grid.YAxisStyle.IsVisible = false;

            // Use shared padding from ChartService
            plt.Layout.Fixed(_chartService.GetDefaultPadding());
        }

        private static void AssignRight(IPlottable p, Plot plt)
        {
            p.Axes.YAxis = plt.Axes.Right;
            p.Axes.XAxis = plt.Axes.Bottom;
        }

        public async Task PlotRsi(FormsPlot fp, CandlestickPlot candlePlot, List<AppQuote> quotes, string symbol, string timeframe)
        {
            PrepareSubPlot(fp);
            await Cuckoo_RsiDetector.Draw_SubChart(fp, candlePlot, quotes, symbol, timeframe);
            fp.Plot.Axes.AutoScale();
        }

        public async Task PlotMacd(FormsPlot fp, CandlestickPlot candlePlot, List<AppQuote> quotes, string symbol, string timeframe)
        {
            //PrepareSubPlot(plt);
            //var line = plt.Add.Scatter(times, macd);
            //AssignRight(line, plt);
            //var h0 = plt.Add.HorizontalLine(0); h0.Color = new ScottPlot.Color(128, 128, 128); AssignRight(h0, plt);
            //plt.Axes.AutoScale();
        }

        public async Task PlotCci(FormsPlot fp, CandlestickPlot candlePlot, List<AppQuote> quotes, string symbol, string timeframe)
        {
            //PrepareSubPlot(plt);
            //var line = plt.Add.Scatter(times, cci);
            //AssignRight(line, plt);
            //var h100 = plt.Add.HorizontalLine(100); h100.Color = new ScottPlot.Color(0, 160, 0); AssignRight(h100, plt);
            //var h0 = plt.Add.HorizontalLine(0); h0.Color = new ScottPlot.Color(128, 128, 128); AssignRight(h0, plt);
            //var hm100 = plt.Add.HorizontalLine(-100); hm100.Color = new ScottPlot.Color(200, 0, 0); AssignRight(hm100, plt);
            //plt.Axes.AutoScale();
        }

        public async Task PlotStochRsi(FormsPlot fp, CandlestickPlot candlePlot, List<AppQuote> quotes, string symbol, string timeframe)
        {
            //PrepareSubPlot(plt);
            //var line = plt.Add.Scatter(times, stochRsi);
            //AssignRight(line, plt);
            //plt.Axes.SetLimitsY(0, 100);
            //var h80 = plt.Add.HorizontalLine(80); h80.Color = new ScottPlot.Color(0, 160, 0); AssignRight(h80, plt);
            //var h20 = plt.Add.HorizontalLine(20); h20.Color = new ScottPlot.Color(200, 0, 0); AssignRight(h20, plt);
            //plt.Axes.AutoScale();
        }

        public async Task LoadAndRender(FormsPlot fp, string symbol, string timeFrame, string indicatorName)
        {
            if (fp is null) return;
            if (string.IsNullOrWhiteSpace(symbol) || string.IsNullOrWhiteSpace(timeFrame)) return;

            // Fetch lightweight arrays precomputed by technical service
            var dict = await _tech.GetIndicatorsDictionary(symbol, timeFrame) ?? new System.Collections.Generic.Dictionary<string, object>();
            var timesObj = await _tech.GetAsync(symbol, timeFrame, "Times");
            var times = timesObj as double[] ?? Array.Empty<double>();

            var plt = fp.Plot;

            switch (indicatorName.ToUpperInvariant())
            {
                case "RSI":
                    var rsi = dict.TryGetValue("RsiArr", out var rsiObj) && rsiObj is double[] r ? r : Array.Empty<double>();
                    //PlotRsi(plt, times, rsi);
                    break;
                case "MACD":
                    var macd = dict.TryGetValue("MacdArr", out var macdObj) && macdObj is double[] m ? m : Array.Empty<double>();
                    //PlotMacd(plt, times, macd);
                    break;
                case "CCI":
                    var cci = dict.TryGetValue("CciArr", out var cciObj) && cciObj is double[] c ? c : Array.Empty<double>();
                    //PlotCci(plt, times, cci);
                    break;
                case "STOCHRSI":
                    var stoch = dict.TryGetValue("StochRsiArr", out var stObj) && stObj is double[] s ? s : Array.Empty<double>();
                    //PlotStochRsi(plt, times, stoch);
                    break;
                default:
                    // Unknown indicator, just clear
                    PrepareSubPlot(fp);
                    break;
            }

            // refresh and return
            await _chartService.AutoScaleAndRender(fp);
        }
    }
}
