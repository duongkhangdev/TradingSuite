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
using ChartPro.Toolbars;
using WeifenLuo.WinFormsUI.Docking;
using System.Collections.Generic;

namespace TradingApp.WinUI.Docking
{
    public class ChartDocument : DockContent
    {
        private readonly ToolStripContainer _container;
        private readonly ChartTopToolbar _topToolbar;
        private readonly ChartLeftToolbar _leftToolbar;

        private readonly TableLayoutPanel _plotsHost;
        private readonly FormsPlot _pricePlot;

        private sealed class Subplot
        {
            public string Key { get; init; } = string.Empty;
            public FormsPlot Plot { get; init; } = new FormsPlot();
            public Func<double[], double[]> Compute { get; init; } = _ => Array.Empty<double>();
            public Action<Plot, double[], double[]> Render { get; init; } = (_, __, ___) => { };
        }

        private readonly List<Subplot> _subplots = new();

        private readonly IChartDataService _dataService;
        private readonly ILogger<ChartDocument> _logger;
        private readonly IChartService _chartService;
        private readonly ISubPlotService _subPlotService;
        private CancellationTokenSource? _cts;

        public string Symbol { get; }
        public string Timeframe { get; private set; }

        private sealed class NoopLogger<T> : ILogger<T>
        {
            IDisposable ILogger.BeginScope<TState>(TState state) => Dummy.Instance;
            bool ILogger.IsEnabled(LogLevel logLevel) => false;
            void ILogger.Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) { }
            private sealed class Dummy : IDisposable { public static readonly Dummy Instance = new(); public void Dispose() { } }
        }

        public ChartDocument(string symbol, string timeframe)
            : this(symbol, timeframe, new ChartPro.Services.DemoChartDataService(), new NoopLogger<ChartDocument>(), new ChartPro.Services.ChartService(), new ChartPro.Services.SubPlotService())
        { }

        public ChartDocument(string symbol, string timeframe, IChartDataService dataService, ILogger<ChartDocument>? logger = null, IChartService? chartService = null, ISubPlotService? subPlotService = null)
        {
            Symbol = symbol;
            Timeframe = timeframe;
            _dataService = dataService;
            _logger = logger ?? new NoopLogger<ChartDocument>();
            _chartService = chartService ?? new ChartPro.Services.ChartService();
            _subPlotService = subPlotService ?? new ChartPro.Services.SubPlotService();

            Text = $"{symbol},{timeframe}";
            TabText = Text;

            _container = new ToolStripContainer { Dock = DockStyle.Fill };

            _topToolbar = new ChartTopToolbar();
            _topToolbar.TimeframeSelected += tf => { Timeframe = tf; _ = RefreshChartAsync(); UpdateTitle(); };
            _topToolbar.SymbolChanged += sym => { /* future: load different symbol */ UpdateTitle(); };
            _topToolbar.RefreshRequested += async () => await RefreshChartAsync();
            _topToolbar.IndicatorToggled += OnIndicatorToggled;
            _container.TopToolStripPanel.Controls.Add(_topToolbar);

            _leftToolbar = new ChartLeftToolbar();
            _leftToolbar.DrawToolSelected += tool => HandleDrawTool(tool);
            _leftToolbar.ClearRequested += () => ClearDrawings();
            _container.LeftToolStripPanel.Controls.Add(_leftToolbar);

            _plotsHost = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1 };

            _pricePlot = new FormsPlot { Dock = DockStyle.Fill };
            _plotsHost.Controls.Add(_pricePlot, 0, 0);

            // Register subplots dynamically (order defines row order)
            AddSubplot("RSI", initiallyVisible: true,
                compute: closes => ComputeRsi(closes, 14),
                render: (plt, times, vals) => _subPlotService.PlotRsi(plt, times, vals));

            AddSubplot("MACD", initiallyVisible: false,
                compute: closes => ComputeMacd(closes),
                render: (plt, times, vals) => _subPlotService.PlotMacd(plt, times, vals));

            AddSubplot("CCI", initiallyVisible: false,
                compute: closes => ComputeCci(closes, 20),
                render: (plt, times, vals) => _subPlotService.PlotCci(plt, times, vals));

            AddSubplot("StochRSI", initiallyVisible: false,
                compute: closes => ComputeStochRsi(closes, 14),
                render: (plt, times, vals) => _subPlotService.PlotStochRsi(plt, times, vals));

            _container.ContentPanel.Controls.Add(_plotsHost);
            Controls.Add(_container);

            // One-way sync: when price plot zoom/pan -> sync indicator X ranges
            _pricePlot.MouseWheel += (s, e) => SyncIndicatorsXFromPrice();
            _pricePlot.MouseUp += (s, e) => SyncIndicatorsXFromPrice();

            Load += ChartDocument_Load;
            Disposed += (s, e) => _cts?.Cancel();
        }

        private void AddSubplot(string key, bool initiallyVisible, Func<double[], double[]> compute, Action<Plot, double[], double[]> render)
        {
            var plot = new FormsPlot { Dock = DockStyle.Fill, Visible = initiallyVisible };
            var sp = new Subplot
            {
                Key = key,
                Plot = plot,
                Compute = compute,
                Render = render
            };
            _subplots.Add(sp);
            // Ensure correct row for this subplot (row index is subplots index + 1)
            int row = _subplots.Count; // price at 0
            if (_plotsHost.RowCount < row + 1)
                _plotsHost.RowCount = row + 1;
            _plotsHost.Controls.Add(plot, 0, row);
        }

        private void SyncIndicatorsXFromPrice()
        {
            try
            {
                var lim = _pricePlot.Plot.Axes.GetLimits();
                double left = lim.Left;
                double right = lim.Right;
                foreach (var sp in _subplots)
                {
                    if (sp.Plot.Visible)
                    {
                        sp.Plot.Plot.Axes.SetLimitsX(left, right);
                        sp.Plot.Refresh();
                    }
                }
            }
            catch { }
        }

        private void OnIndicatorToggled(string name, bool visible)
        {
            var sp = _subplots.FirstOrDefault(s => string.Equals(s.Key, name, StringComparison.OrdinalIgnoreCase));
            if (sp != null)
            {
                sp.Plot.Visible = visible;
                RecalculateRowHeights();
                _ = RefreshChartAsync();
            }
        }

        private void RecalculateRowHeights()
        {
            _plotsHost.SuspendLayout();
            _plotsHost.RowStyles.Clear();

            int visibleIndicatorCount = _subplots.Count(p => p.Plot.Visible);
            if (visibleIndicatorCount == 0)
            {
                _plotsHost.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
                for (int i = 0; i < _subplots.Count; i++)
                    _plotsHost.RowStyles.Add(new RowStyle(SizeType.Absolute, 0f));
            }
            else
            {
                _plotsHost.RowStyles.Add(new RowStyle(SizeType.Percent, 60f));
                float each = 40f / visibleIndicatorCount;
                foreach (var sp in _subplots)
                {
                    _plotsHost.RowStyles.Add(sp.Plot.Visible
                        ? new RowStyle(SizeType.Percent, each)
                        : new RowStyle(SizeType.Absolute, 0f));
                }
            }
            _plotsHost.ResumeLayout(true);
        }

        private void UpdateTitle()
        {
            Text = $"{Symbol},{Timeframe}";
            TabText = Text;
        }

        private void HandleDrawTool(string tool)
        {
            _logger.LogInformation("Draw tool selected: {Tool} on {Symbol} {Timeframe}", tool, Symbol, Timeframe);
        }

        private void ClearDrawings()
        {
            _logger.LogInformation("Clear drawings: {Symbol} {Timeframe}", Symbol, Timeframe);
        }

        private async void ChartDocument_Load(object? sender, EventArgs e)
        {
            _topToolbar.Initialize(Symbol, Timeframe, new[] { Symbol, "EURUSD", "US30" });
            RecalculateRowHeights();
            await RefreshChartAsync();
            SyncIndicatorsXFromPrice();
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
                var closes = candles.Select(c => c.Close).ToArray();
                var times = candles.Select(c => c.Time.ToOADate()).ToArray();

                var pplt = _pricePlot.Plot;
                pplt.Clear();
                _chartService.ApplyDefaultLayout(pplt);
                var cs = pplt.Add.Candlestick(ohlcs);
                _chartService.AssignPriceAxisRight(cs, pplt);
                pplt.Axes.AutoScale();

                foreach (var sp in _subplots)
                {
                    if (!sp.Plot.Visible) continue;
                    var values = sp.Compute(closes);
                    sp.Render(sp.Plot.Plot, times, values);
                    sp.Plot.Refresh();
                }

                _pricePlot.Refresh();
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

        private static double[] ComputeRsi(double[] closes, int period)
        {
            if (closes.Length == 0 || period <= 0) return Array.Empty<double>();
            double[] rsi = new double[closes.Length];
            double avgGain = 0, avgLoss = 0;
            for (int i = 1; i < closes.Length; i++)
            {
                double change = closes[i] - closes[i - 1];
                double gain = change > 0 ? change : 0;
                double loss = change < 0 ? -change : 0;

                if (i <= period)
                {
                    avgGain += gain;
                    avgLoss += loss;
                    if (i == period)
                    {
                        avgGain /= period;
                        avgLoss /= period;
                        double rs = avgLoss == 0 ? double.PositiveInfinity : avgGain / avgLoss;
                        rsi[i] = 100 - (100 / (1 + rs));
                    }
                }
                else
                {
                    avgGain = (avgGain * (period - 1) + gain) / period;
                    avgLoss = (avgLoss * (period - 1) + loss) / period;
                    double rs = avgLoss == 0 ? double.PositiveInfinity : avgGain / avgLoss;
                    rsi[i] = 100 - (100 / (1 + rs));
                }
            }
            for (int i = 0; i < Math.Min(period, rsi.Length); i++) rsi[i] = double.NaN;
            return rsi;
        }

        private static double[] ComputeMacd(double[] closes)
        {
            double[] ema12 = ComputeEma(closes, 12);
            double[] ema26 = ComputeEma(closes, 26);
            return closes.Select((_, i) => ema12[i] - ema26[i]).ToArray();
        }

        private static double[] ComputeCci(double[] closes, int period)
        {
            if (closes.Length == 0) return Array.Empty<double>();
            double[] cci = new double[closes.Length];
            for (int i = 0; i < closes.Length; i++)
            {
                if (i < period) { cci[i] = double.NaN; continue; }
                double sma = closes.Skip(i - period + 1).Take(period).Average();
                double meanDev = closes.Skip(i - period + 1).Take(period).Average(v => Math.Abs(v - sma));
                if (meanDev == 0) { cci[i] = 0; continue; }
                cci[i] = (closes[i] - sma) / (0.015 * meanDev);
            }
            return cci;
        }

        private static double[] ComputeStochRsi(double[] closes, int period)
        {
            if (closes.Length < period) return closes.Select(_ => double.NaN).ToArray();
            double[] rsi = ComputeRsi(closes, period);
            double[] stoch = new double[closes.Length];
            for (int i = 0; i < closes.Length; i++)
            {
                if (i < period) { stoch[i] = double.NaN; continue; }
                var window = rsi.Skip(i - period + 1).Take(period).ToArray();
                double min = window.Where(v => !double.IsNaN(v)).DefaultIfEmpty(double.NaN).Min();
                double max = window.Where(v => !double.IsNaN(v)).DefaultIfEmpty(double.NaN).Max();
                if (double.IsNaN(min) || double.IsNaN(max) || max - min == 0) { stoch[i] = double.NaN; continue; }
                stoch[i] = (rsi[i] - min) / (max - min) * 100.0;
            }
            return stoch;
        }

        private static double[] ComputeEma(double[] values, int period)
        {
            double[] ema = new double[values.Length];
            if (values.Length == 0) return ema;
            double k = 2.0 / (period + 1);
            ema[0] = values[0];
            for (int i = 1; i < values.Length; i++)
                ema[i] = values[i] * k + ema[i - 1] * (1 - k);
            return ema;
        }

        protected override string GetPersistString()
        {
            return $"{nameof(ChartDocument)};{Symbol};{Timeframe}";
        }
    }
}
