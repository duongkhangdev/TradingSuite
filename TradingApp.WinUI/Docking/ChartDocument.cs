using System;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using ScottPlot;
using ScottPlot.WinForms;
using ChartPro.Services;
using WeifenLuo.WinFormsUI.Docking;

namespace TradingApp.WinUI.Docking
{
    public class ChartDocument : DockContent
    {
        private readonly FormsPlot _formsPlot;
        private readonly ToolStrip _toolStrip;
        private readonly IChartDataService _dataService;
        private readonly ILogger<ChartDocument> _logger;
        private readonly IChartService _chartService;
        private CancellationTokenSource? _cts;

        public string Symbol { get; }
        public string Timeframe { get; }

        private sealed class NoopLogger<T> : ILogger<T>
        {
            IDisposable ILogger.BeginScope<TState>(TState state) => Dummy.Instance;
            bool ILogger.IsEnabled(LogLevel logLevel) => false;
            void ILogger.Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) { }
            private sealed class Dummy : IDisposable { public static readonly Dummy Instance = new(); public void Dispose() { } }
        }

        public ChartDocument(string symbol, string timeframe)
            : this(symbol, timeframe, new ChartPro.Services.DemoChartDataService(), new NoopLogger<ChartDocument>(), new ChartPro.Services.ChartService())
        { }

        public ChartDocument(string symbol, string timeframe, IChartDataService dataService, ILogger<ChartDocument>? logger = null, IChartService? chartService = null)
        {
            Symbol = symbol;
            Timeframe = timeframe;
            _dataService = dataService;
            _logger = logger ?? new NoopLogger<ChartDocument>();
            _chartService = chartService ?? new ChartPro.Services.ChartService();

            Text = $"{symbol} [{timeframe}]";
            TabText = Text;

            _toolStrip = new ToolStrip
            {
                GripStyle = ToolStripGripStyle.Hidden,
                Dock = DockStyle.Top
            };

            var lblSymbol = new ToolStripLabel(symbol)
            {
                Font = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold)
            };

            var lblTf = new ToolStripLabel(timeframe);

            var btnRefresh = new ToolStripButton("Refresh");
            btnRefresh.Click += async (s, e) => await RefreshChartAsync();

            _toolStrip.Items.Add(lblSymbol);
            _toolStrip.Items.Add(new ToolStripSeparator());
            _toolStrip.Items.Add(lblTf);
            _toolStrip.Items.Add(new ToolStripSeparator());
            _toolStrip.Items.Add(btnRefresh);

            _formsPlot = new FormsPlot
            {
                Dock = DockStyle.Fill
            };

            Controls.Add(_formsPlot);
            Controls.Add(_toolStrip);

            Load += ChartDocument_Load;
            Disposed += (s, e) => _cts?.Cancel();
        }

        private async void ChartDocument_Load(object? sender, EventArgs e)
        {
            await RefreshChartAsync();
        }

        private static TimeSpan ParseTimeframe(string tf)
        {
            if (string.IsNullOrWhiteSpace(tf)) return TimeSpan.FromMinutes(1);
            tf = tf.Trim().ToUpperInvariant();
            try
            {
                if (tf.StartsWith("M") && int.TryParse(tf[1..], out int m))
                    return TimeSpan.FromMinutes(m);
                if (tf.StartsWith("H") && int.TryParse(tf[1..], out int h))
                    return TimeSpan.FromHours(h);
                if (tf.StartsWith("D") && int.TryParse(tf[1..], out int d))
                    return TimeSpan.FromDays(d);
                if (tf == "W")
                    return TimeSpan.FromDays(7);
                if (tf.StartsWith("W") && int.TryParse(tf[1..], out int wv))
                    return TimeSpan.FromDays(7 * wv);
            }
            catch { }
            return TimeSpan.FromMinutes(1);
        }

        public async Task RefreshChartAsync()
        {
            try
            {
                _cts?.Cancel();
                _cts = new CancellationTokenSource();
                var ct = _cts.Token;

                var candles = await _dataService.GetCandlesAsync(Symbol, Timeframe, 300, ct);
                var span = ParseTimeframe(Timeframe);
                var ohlcs = candles.Select(c => new OHLC(c.Open, c.High, c.Low, c.Close, c.Time, span)).ToArray();

                var plt = _formsPlot.Plot;
                plt.Clear();
                _chartService.ApplyDefaultLayout(plt);

                var cs = plt.Add.Candlestick(ohlcs);
                _chartService.AssignPriceAxisRight(cs, plt);

                plt.Axes.AutoScale();

                _formsPlot.Refresh();
            }
            catch (OperationCanceledException)
            {
                // ignore
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to refresh chart for {Symbol} {Timeframe}", Symbol, Timeframe);
                MessageBox.Show(this, ex.Message, "Chart Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected override string GetPersistString()
        {
            return $"{nameof(ChartDocument)};{Symbol};{Timeframe}";
        }
    }
}
