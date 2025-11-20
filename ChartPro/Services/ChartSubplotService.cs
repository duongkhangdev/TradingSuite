using ChartPro;
using Cuckoo.Shared;
using ScottPlot;
using ScottPlot.Plottables;
using ScottPlot.WinForms;
using Skender.Stock.Indicators;
using System.Collections.Generic;
using System.Linq;
using TradingSuite.Charting.Extensions;
using TradingSuite.Charting.Services;

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

            // Bảo đảm trục giá bên phải cùng bề rộng với chart chính
            ScottHelper.FixRightAxisWidth(fp, ScottHelper.GetCachedRightAxisWidth());
        }

        private static void AssignRight(IPlottable p, Plot plt)
        {
            p.Axes.YAxis = plt.Axes.Right;
            p.Axes.XAxis = plt.Axes.Bottom;
        }

        public async Task LoadAndRender(FormsPlot fp, CandlestickPlot? candlePlot, List<AppQuote>? quotes, string symbol, string timeFrame, string indicatorName)
        {
            if (fp is null)
                return;

            if (string.IsNullOrWhiteSpace(indicatorName))
            {
                fp.Plot.Clear();
                fp.Refresh();
                return;
            }

            var trimmed = indicatorName.Trim();
            if (trimmed.Equals("RSI", StringComparison.OrdinalIgnoreCase))
            {
                var rsiRendered = await RenderRsiAsync(fp, candlePlot, quotes, symbol, timeFrame);
                if (!rsiRendered)
                {
                    fp.Plot.Clear();
                    fp.Refresh();
                }
                return;
            }

            if (candlePlot is null || quotes.IsNullOrEmpty() || string.IsNullOrWhiteSpace(symbol) || string.IsNullOrWhiteSpace(timeFrame))
            {
                fp.Plot.Clear();
                fp.Refresh();
                return;
            }

            var dict = await _tech.GetIndicatorsDictionary(symbol, timeFrame) ?? new Dictionary<string, object>();
            PrepareSubPlot(fp);

            var plt = fp.Plot;
            var rendered = trimmed.ToUpperInvariant() switch
            {
                "MACD" when TryGetIndicatorList(dict, "Macd", out IReadOnlyList<MacdResult> macd) => RenderMacd(plt, candlePlot, quotes, macd),
                "CCI" when TryGetIndicatorList(dict, "Cci", out IReadOnlyList<CciResult> cci) => RenderCci(plt, candlePlot, quotes, cci),
                "STOCHRSI" when TryGetIndicatorList(dict, "StochRsi", out IReadOnlyList<StochRsiResult> stochRsi) => RenderStochRsi(plt, candlePlot, quotes, stochRsi),
                _ => false
            };

            if (!rendered)
                plt.Clear();

            ScottHelper.ApplyRightAxisWidth(candlePlot, fp);
            await _chartService.AutoScaleAndRender(fp);
        }

        private static bool TryGetIndicatorList<T>(Dictionary<string, object> dict, string key, out IReadOnlyList<T> list) where T : class
        {
            list = Array.Empty<T>();
            if (!dict.TryGetValue(key, out var raw) || raw is null)
                return false;

            if (raw is IReadOnlyList<T> typedList)
            {
                list = typedList;
                return list.Count > 0;
            }

            if (raw is IEnumerable<T> typedEnum)
            {
                list = typedEnum.ToList();
                return list.Count > 0;
            }

            return false;
        }

        private static async Task<bool> RenderRsiAsync(FormsPlot fp, CandlestickPlot? candlePlot, List<AppQuote>? quotes, string symbol, string timeframe)
        {
            if (candlePlot is null || quotes.IsNullOrEmpty())
                return false;

            await Cuckoo_RsiDetector.Draw_SubChart(fp, candlePlot, quotes!, symbol, timeframe);
            return true;
        }

        private static double[] BuildXs(CandlestickPlot? candlePlot, List<AppQuote>? quotes, int length)
        {
            if (candlePlot != null && quotes != null && quotes.Count >= length)
            {
                var xs = new double[length];
                for (int i = 0; i < length; i++)
                    xs[i] = ScottHelper.GetXForIndex(candlePlot, quotes, i);
                return xs;
            }

            return Enumerable.Range(0, length).Select(i => (double)i).ToArray();
        }

        private static bool RenderMacd(Plot plt, CandlestickPlot? candlePlot, List<AppQuote>? quotes, IReadOnlyList<MacdResult> macd)
        {
            if (macd.Count == 0)
                return false;

            var xs = BuildXs(candlePlot, quotes, macd.Count);
            var macdLine = macd.Select(m => m.Macd ?? double.NaN).ToArray();
            var signalLine = macd.Select(m => m.Signal ?? double.NaN).ToArray();
            var histogram = macd.Select(m => m.Histogram ?? double.NaN).ToArray();

            var histogramLine = plt.Add.ScatterLine(xs, histogram);
            AssignRight(histogramLine, plt);
            histogramLine.MarkerSize = 0;
            histogramLine.Color = ScottPlot.Colors.SlateGray.WithAlpha(.6f);

            var macdScatter = plt.Add.ScatterLine(xs, macdLine);
            AssignRight(macdScatter, plt);
            macdScatter.MarkerSize = 0;
            macdScatter.Color = ScottPlot.Colors.DeepSkyBlue;

            var signalScatter = plt.Add.ScatterLine(xs, signalLine);
            AssignRight(signalScatter, plt);
            signalScatter.MarkerSize = 0;
            signalScatter.Color = ScottPlot.Colors.Orange;

            var zero = plt.Add.HorizontalLine(0);
            AssignRight(zero, plt);
            zero.LinePattern = LinePattern.Dashed;
            zero.Color = ScottPlot.Colors.Gray;

            return true;
        }

        private static bool RenderCci(Plot plt, CandlestickPlot? candlePlot, List<AppQuote>? quotes, IReadOnlyList<CciResult> cci)
        {
            if (cci.Count == 0)
                return false;

            var xs = BuildXs(candlePlot, quotes, cci.Count);
            var values = cci.Select(c => c.Cci ?? double.NaN).ToArray();

            var line = plt.Add.ScatterLine(xs, values);
            AssignRight(line, plt);
            line.MarkerSize = 0;
            line.Color = ScottPlot.Colors.CornflowerBlue;

            AddHorizontalGuide(plt, 100, ScottPlot.Colors.SeaGreen);
            AddHorizontalGuide(plt, 0, ScottPlot.Colors.Gray);
            AddHorizontalGuide(plt, -100, ScottPlot.Colors.OrangeRed);

            return true;
        }

        private static bool RenderStochRsi(Plot plt, CandlestickPlot? candlePlot, List<AppQuote>? quotes, IReadOnlyList<StochRsiResult> stochRsi)
        {
            if (stochRsi.Count == 0)
                return false;

            var xs = BuildXs(candlePlot, quotes, stochRsi.Count);
            var values = stochRsi.Select(s => s.StochRsi ?? double.NaN).ToArray();

            var line = plt.Add.ScatterLine(xs, values);
            AssignRight(line, plt);
            line.MarkerSize = 0;
            line.Color = ScottPlot.Colors.MediumPurple;

            AddHorizontalGuide(plt, 80, ScottPlot.Colors.SeaGreen);
            AddHorizontalGuide(plt, 20, ScottPlot.Colors.OrangeRed);
            plt.Axes.SetLimitsY(0, 100);

            return true;
        }

        private static void AddHorizontalGuide(Plot plt, double value, ScottPlot.Color color)
        {
            var line = plt.Add.HorizontalLine(value);
            AssignRight(line, plt);
            line.Color = color;
            line.LinePattern = LinePattern.Dashed;
        }
    }
}
