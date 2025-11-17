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
using ChartPro;
using Cuckoo.Shared;
using Microsoft.Extensions.Logging.Abstractions;

namespace TradingApp.WinUI.Docking
{
    public class ChartDocument : DockContent
    {
        private readonly ToolStripContainer _container;
        private readonly ChartTopToolbar _topToolbar;
        private readonly ChartLeftToolbar _leftToolbar;
            
        private readonly TableLayoutPanel _plotsHost;
        private readonly FormsPlot _pricePlot;

        private const int RightAxisWidthPx = 68;

        private sealed class Subplot
        {
            public string Key { get; init; } = string.Empty;
            public FormsPlot Plot { get; init; } = new FormsPlot();
            public Action<Plot, double[], double[]> Render { get; init; } = (_, __, ___) => { };
        }

        private readonly List<Subplot> _subplots = new();
       
        private readonly ILogger<ChartDocument> _logger;
        private readonly IQuoteService _quoteService;
        private readonly IChartService _chartService;
        private readonly IChartSubplotService _subPlotService;
        private CancellationTokenSource? _cts;

        public string Symbol;
        public string Timeframe;

        public ChartDocument(
            IQuoteService quoteService, 
            IChartService chartService,
            IChartSubplotService subPlotService,
            ILogger<ChartDocument> logger)
        {
            _quoteService = quoteService;
            _chartService = chartService;
            _subPlotService = subPlotService;
            _logger = logger;   

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
            RegisterSubplots();

            _container.ContentPanel.Controls.Add(_plotsHost);
            Controls.Add(_container);

            // Apply fixed right-axis width now and on layout changes
            ApplyRightAxisWidthToAll();
            _container.ContentPanel.SizeChanged += (s, e) => ApplyRightAxisWidthToAll();
            this.Resize += (s, e) => ApplyRightAxisWidthToAll();

            // One-way sync: when price plot zoom/pan -> sync indicator X ranges
            _pricePlot.MouseWheel += (s, e) => SyncIndicatorsXFromPrice();
            _pricePlot.MouseUp += (s, e) => SyncIndicatorsXFromPrice();

            Load += ChartDocument_Load;
            Disposed += (s, e) => _cts?.Cancel();
        }

        private void ApplyRightAxisWidthToAll()
        {
            try
            {
                ScottHelper.FixRightAxisWidth(_pricePlot, RightAxisWidthPx);
                foreach (var sp in _subplots)
                    ScottHelper.FixRightAxisWidth(sp.Plot, RightAxisWidthPx);
            }
            catch { }
        }

        private void AddSubplot(string key, bool initiallyVisible, Action<Plot, double[], double[]> render)
        {
            var plot = new FormsPlot { Dock = DockStyle.Fill, Visible = initiallyVisible };
            var sp = new Subplot
            {
                Key = key,
                Plot = plot,
                Render = render
            };
            _subplots.Add(sp);
            int row = _subplots.Count; // price at row 0
            if (_plotsHost.RowCount < row + 1)
                _plotsHost.RowCount = row + 1;
            _plotsHost.Controls.Add(plot, 0, row);
        }

        private void RegisterSubplots()
        {
            AddSubplot("RSI", initiallyVisible: true, (plt, times, vals) => _subPlotService.PlotRsi(plt, times, vals));
            AddSubplot("MACD", initiallyVisible: false, (plt, times, vals) => _subPlotService.PlotMacd(plt, times, vals));
            AddSubplot("CCI", initiallyVisible: false, (plt, times, vals) => _subPlotService.PlotCci(plt, times, vals));
            AddSubplot("StochRSI", initiallyVisible: false, (plt, times, vals) => _subPlotService.PlotStochRsi(plt, times, vals));
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
                ApplyRightAxisWidthToAll();
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
            ApplyRightAxisWidthToAll();
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

        private double[]? _lastTimes;
        private double[]? _lastCloses;

        public async Task RefreshChartAsync()
        {
            try
            {
                _cts?.Cancel();
                _cts = new CancellationTokenSource();
                var ct = _cts.Token;

                var candles = await _quoteService.GetAsync(Symbol, Timeframe);
                var interval = BrokerHelper.GetInterval(Timeframe);
                var ohlcs = candles.ToOHLCs(interval);

                // compute technicals using service
                var tech = new ChartTechnicalService(new Microsoft.Extensions.Logging.Abstractions.NullLogger<ChartTechnicalService>());
                await tech.IndicatorsCompute(Symbol, Timeframe, candles);
                var dict = await tech.GetIndicatorsDictionary(Symbol, Timeframe);

                var pplt = _pricePlot.Plot;
                pplt.Clear();
                _chartService.ApplyDefaultLayout(pplt);
                var cs = pplt.Add.Candlestick(ohlcs);
                _chartService.AssignPriceAxisRight(cs, pplt);
                pplt.Axes.AutoScale();

                // Get times and closes for subplots
                _lastTimes = dict != null && dict.TryGetValue("Times", out var tObj) && tObj is double[] tArr ? tArr : candles.Select(q => q.Date.ToOADate()).ToArray();
                _lastCloses = dict != null && dict.TryGetValue("Closes", out var cObj) && cObj is double[] cArr ? cArr : candles.Select(q => Convert.ToDouble(q.Close)).ToArray();

                foreach (var sp in _subplots)
                {
                    if (!sp.Plot.Visible) continue;

                    double[] values = Array.Empty<double>();
                    switch (sp.Key)
                    {
                        case "RSI":
                            values = dict != null && dict.TryGetValue("RsiArr", out var rsiObj) && rsiObj is double[] rsiArr ? rsiArr : Array.Empty<double>();
                            break;
                        case "MACD":
                            values = dict != null && dict.TryGetValue("MacdArr", out var macdObj) && macdObj is double[] macdArr ? macdArr : Array.Empty<double>();
                            break;
                        case "CCI":
                            values = dict != null && dict.TryGetValue("CciArr", out var cciObj) && cciObj is double[] cciArr ? cciArr : Array.Empty<double>();
                            break;
                        case "StochRSI":
                            values = dict != null && dict.TryGetValue("StochRsiArr", out var stObj) && stObj is double[] stArr ? stArr : Array.Empty<double>();
                            break;
                    }

                    sp.Render(sp.Plot.Plot, _lastTimes, values);
                    sp.Plot.Refresh();
                }

                _pricePlot.Refresh();
                ApplyRightAxisWidthToAll();
            }
            catch (OperationCanceledException)
            {
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
