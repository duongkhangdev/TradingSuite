using Cuckoo.Shared;
using ScottPlot;
using ScottPlot.Plottables;
using ScottPlot.WinForms;
using System.Threading.Tasks;

namespace ChartPro.Services
{
    public class ChartService : IChartService
    {
        private static readonly PixelPadding DefaultPadding = new PixelPadding(60, 10, 10, 30);
        private readonly IQuoteService _quoteService;

        public ChartService(IQuoteService quoteService)
        {
            _quoteService = quoteService;
        }

        public void ApplyDefaultLayout(Plot plt)
        {
            // X dưới dạng thời gian
            plt.Axes.DateTimeTicksBottom();

            // Trục giá bên phải
            var right = plt.Axes.Right;
            right.IsVisible = true;

            // Ẩn trục trái để tránh trùng lặp
            plt.Axes.Left.IsVisible = false;

            // Lưới nhẹ
            plt.Grid.MajorLineColor = new ScottPlot.Color(40, 40, 40);
            plt.Grid.MinorLineColor = new ScottPlot.Color(25, 25, 25);

            // Padding: Left, Right, Top, Bottom
            plt.Layout.Fixed(DefaultPadding);
        }

        public void AssignPriceAxisRight(IPlottable plottable, Plot plt)
        {
            // Gán trục Y của plottable sang Right Axis
            plottable.Axes.YAxis = plt.Axes.Right;
            plottable.Axes.XAxis = plt.Axes.Bottom;
        }

        public PixelPadding GetDefaultPadding() => DefaultPadding;

        public async Task ApplyBackgroundText(FormsPlot fp, string symbol, string timeFrame)
        {
            var plot = fp.Plot;

            (var line1, var line2) = plot.Add.BackgroundText($"{symbol}, {timeFrame}", "Highest Recommendation by Cuckoo");

            line1.LabelFontColor = Colors.Gray.WithAlpha(.4);
            line1.LabelFontSize = 64;
            line1.LabelBold = false;

            line2.LabelFontColor = Colors.Gray.WithAlpha(.4);
            line2.LabelFontSize = 18;
            line2.LabelBold = false;

            await Task.CompletedTask;
        }

        public async Task<CandlestickPlot?> LoadAndRender(FormsPlot fp, string symbol, string timeFrame, bool hasGap, List<AppQuote>? quotes)
        {
            if (string.IsNullOrEmpty(symbol) || string.IsNullOrEmpty(timeFrame))
                return null;
            if (quotes == null || !quotes.Any())
                return null;

            var plot = fp.Plot;

            var interval = BrokerHelper.GetInterval(timeFrame);
            List<OHLC> OHLCs = quotes.ToOHLCs(interval);

            // 1. đặt lại text nền
            await ApplyBackgroundText(fp, symbol, timeFrame);

            // 2. vẽ nến
            var candlestickPlot = plot.Add.Candlestick(OHLCs);
            //candlestickPlot.SymbolWidth = 1.0;

            // 3) Đặt lại trục Y bên phải            
            candlestickPlot.Axes.YAxis = plot.Axes.Right;
            plot.Grid.YAxis = plot.Axes.Right;
            plot.Axes.Left.IsVisible = false;
            plot.Axes.Right.IsVisible = true;
            plot.Axes.Right.TickGenerator = new ScottPlot.TickGenerators.NumericAutomatic()
            {
                //LabelFormatter = (double value) => value.ToString("C")
            };

            DateTime[] tickDates = quotes!
                        .Select(x => x.Date)
                        .ToArray();

            if (hasGap == true)
            {
                var financeAxis = new FinancialTimeAxis(tickDates);
                var dtTimes = financeAxis.DateTimes;
                plot.Add.Plottable(financeAxis);
                //formsPlot1.Plot.Axes.Bottom.TickGenerator = new ScottPlot.TickGenerators.NumericAutomatic();
                plot.Axes.Bottom.TickLabelStyle.IsVisible = false;
                plot.Grid.XAxisStyle.IsVisible = true;

                candlestickPlot.Sequential = true; // enable sequential mode to place candles at X = 0, 1, 2, ...
            }
            else
            {
                var dtAx = plot.Axes.DateTimeTicksBottom(); // setup DateTime ticks on the bottom

            }

            // 
            plot.Axes.Bottom.MinimumSize = 60;

            // force a redraw
            await AutoScaleAndRender(fp);

            await Task.CompletedTask;
            return candlestickPlot;
        }

        public async Task AutoScaleAndRender(FormsPlot fp)
        {
            var plot = fp.Plot;

            plot.Axes.AutoScale();
            fp.Refresh();

            await Task.CompletedTask;
        }
    }
}
