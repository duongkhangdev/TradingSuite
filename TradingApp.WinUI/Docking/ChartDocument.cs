using ChartPro;
using ChartPro.Charting;
using ChartPro.Charting.Interactions;
using ChartPro.Charting.Shapes;
using ChartPro.Services;
using ChartPro.Toolbars;
using Cuckoo.Shared;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ScottPlot;
using ScottPlot.Plottables;
using ScottPlot.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using TradingApp.WinUI;
using TradingSuite.Charting.Extensions;
using TradingSuite.Charting.Services;

namespace TradingApp.WinUI.Docking
{
    public class ChartDocument : DockContent
    {
        private readonly ToolStripContainer _container;
        private readonly ChartTopToolbar _topToolbar;
        private readonly ChartLeftToolbar _leftToolbar;
        private readonly StatusStrip _statusStrip;
        private readonly ToolStripStatusLabel _statusModeLabel;
        private readonly ToolStripStatusLabel _statusCoordinatesLabel;
        private readonly ToolStripStatusLabel _statusShapeLabel;
            
        private readonly TableLayoutPanel _plotsHost;
        private readonly FormsPlot _pricePlot;

        private const int RightAxisWidthPx = 68;

        private sealed record SubplotDefinition(string Key, bool DefaultVisible);

        private sealed class Subplot
        {
            public Subplot(SubplotDefinition definition, FormsPlot plot)
            {
                Definition = definition;
                Plot = plot;
            }

            public SubplotDefinition Definition { get; }
            public FormsPlot Plot { get; }
        }

        private readonly List<Subplot> _subplots = new();
        private readonly Dictionary<string, Subplot> _subplotLookup = new(StringComparer.OrdinalIgnoreCase);
        private readonly List<SubplotDefinition> _subplotDefinitions;
        private readonly Dictionary<FormsPlot, EventHandler<ScottPlot.RenderDetails>> _axisLinkHandlers = new();
       
        private readonly ILogger<ChartDocument> _logger;
        private readonly IQuoteService _quoteService;
        private readonly IChartService _chartService;
        private readonly IChartSubplotService _subPlotService;
        private readonly IChartTechnicalService _chartTechnicalService;
        private readonly IChartInteractions _chartInteractions;
        private CancellationTokenSource? _cts;
        private CandleSource _activeDataSource = CandleSource.None;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Symbol { get; set; } = string.Empty;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Timeframe { get; set; } = string.Empty;

        public ChartDocument(
            IQuoteService quoteService, 
            IChartService chartService,
            IChartSubplotService subPlotService,
            IChartTechnicalService chartTechnicalService,
            IChartInteractions chartInteractions,
            ILogger<ChartDocument> logger)
        {
            _quoteService = quoteService;
            _chartService = chartService;
            _subPlotService = subPlotService;
            _chartTechnicalService = chartTechnicalService;
            _chartInteractions = chartInteractions;
            _logger = logger;   

            TabText = Text;
            KeyPreview = true;
            KeyDown += ChartDocument_KeyDown;

            _container = new ToolStripContainer { Dock = DockStyle.Fill };

            _topToolbar = new ChartTopToolbar();
            _topToolbar.TimeframeSelected += tf => { Timeframe = tf; _ = RefreshChartAsync(); UpdateTitle(); };
            _topToolbar.SymbolChanged += sym =>
            {
                if (string.IsNullOrWhiteSpace(sym) || string.Equals(sym, Symbol, StringComparison.OrdinalIgnoreCase))
                    return;

                Symbol = sym;
                UpdateTitle();
                _ = RefreshChartAsync();
            };
            _topToolbar.RefreshRequested += () =>
            {
                _ = RefreshChartAsync();
            };
            _topToolbar.IndicatorToggled += OnIndicatorToggled;
            _topToolbar.DataSourceChanged += (source, displayName) =>
            {
                _ = HandleDataSourceChangedAsync(source);
            };
            _container.TopToolStripPanel.Controls.Add(_topToolbar);

            _leftToolbar = new ChartLeftToolbar();
            _leftToolbar.DrawModeSelected += mode => HandleDrawModeSelection(mode);
            _leftToolbar.UndoRequested += () => HandleUndo();
            _leftToolbar.RedoRequested += () => HandleRedo();
            _leftToolbar.DeleteRequested += () => HandleDelete();
            _leftToolbar.ClearRequested += () => ClearDrawings();
            _leftToolbar.SnapToggled += enabled => ToggleSnap(enabled);
            _leftToolbar.SnapModeSelected += mode => HandleSnapModeSelection(mode);
            _leftToolbar.SaveAnnotationsRequested += () => HandleSaveAnnotations();
            _leftToolbar.LoadAnnotationsRequested += () => HandleLoadAnnotations();
            _container.LeftToolStripPanel.Controls.Add(_leftToolbar);

            _statusModeLabel = new ToolStripStatusLabel
            {
                BorderSides = ToolStripStatusLabelBorderSides.Right,
                BorderStyle = Border3DStyle.Etched,
                Width = 150
            };
            _statusCoordinatesLabel = new ToolStripStatusLabel
            {
                BorderSides = ToolStripStatusLabelBorderSides.Right,
                BorderStyle = Border3DStyle.Etched,
                Width = 220
            };
            _statusShapeLabel = new ToolStripStatusLabel
            {
                Spring = true,
                TextAlign = ContentAlignment.MiddleLeft
            };
            _statusStrip = new StatusStrip();
            _statusStrip.Items.AddRange(new ToolStripItem[]
            {
                _statusModeLabel,
                _statusCoordinatesLabel,
                _statusShapeLabel
            });
            _container.BottomToolStripPanel.Controls.Add(_statusStrip);

            _plotsHost = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1 };

            _pricePlot = new FormsPlot { Dock = DockStyle.Fill };
            _chartInteractions.Attach(_pricePlot);
            _chartInteractions.EnableAll();
            _chartInteractions.DrawModeChanged += (s, mode) =>
            {
                _leftToolbar.SetActiveDrawMode(mode);
                UpdateStatusMode(mode);
            };
            _chartInteractions.MouseCoordinatesChanged += (s, coords) => UpdateStatusCoordinates(coords);
            _chartInteractions.ShapeInfoChanged += (s, info) => UpdateStatusShapeInfo(info);
            SetSnapMode(SnapMode.CandleOHLC);
            _leftToolbar.SetSnapState(_chartInteractions.SnapEnabled);
            _plotsHost.Controls.Add(_pricePlot, 0, 0);

            _subplotDefinitions = CreateDefaultSubplotDefinitions();

            // Register subplots dynamically (order defines row order)
            RegisterSubplots();
            _topToolbar.ConfigureIndicatorToggles(_subplotDefinitions
                .Select(def => new ChartTopToolbar.IndicatorToggleDefinition(def.Key, def.DefaultVisible)));

            _container.ContentPanel.Controls.Add(_plotsHost);
            Controls.Add(_container);
            UpdateStatusMode(ChartDrawMode.None);
            UpdateStatusCoordinates(null);
            UpdateStatusShapeInfo(string.Empty);
            _quoteService.QuotesAddedOrUpdated += OnQuotesAddedOrUpdated;
            _quoteService.SymbolRemoved += OnSymbolRemoved;

            // Apply fixed right-axis width now and on layout changes
            ApplyRightAxisWidthToAll();
            _container.ContentPanel.SizeChanged += (s, e) => ApplyRightAxisWidthToAll();
            this.Resize += (s, e) => ApplyRightAxisWidthToAll();

            // One-way sync: when price plot zoom/pan -> sync indicator X ranges
            _pricePlot.MouseWheel += (s, e) => SyncIndicatorsXFromPrice();
            _pricePlot.MouseUp += (s, e) => SyncIndicatorsXFromPrice();

            Load += ChartDocument_Load;
            Disposed += (s, e) =>
            {
                _cts?.Cancel();
                CleanupAxisLinks();
                _chartInteractions.Dispose();
                _quoteService.QuotesAddedOrUpdated -= OnQuotesAddedOrUpdated;
                _quoteService.SymbolRemoved -= OnSymbolRemoved;
            };
        }

        public List<AppQuote>? AppQuotes;
        private List<OHLC>? _ohlcQuotes;

        private CandlestickPlot? candlestickPlot = null;

        private void ApplyRightAxisWidthToAll()
        {
            try
            {
                ScottHelper.FixRightAxisWidth(_pricePlot, RightAxisWidthPx);
                foreach (var sp in _subplots)
                    ScottHelper.MatchRightAxisWidth(_pricePlot, sp.Plot);
            }
            catch { }
        }

        private void ConfigureSubplotHost(FormsPlot subplot)
        {
            if (subplot is null || _pricePlot is null)
                return;

            try
            {
                subplot.Margin = _pricePlot.Margin;
                subplot.Padding = _pricePlot.Padding;
            }
            catch { }

            var sourceAxes = _pricePlot.Plot.Axes;
            var targetAxes = subplot.Plot.Axes;

            targetAxes.Left.MinimumSize = sourceAxes.Left.MinimumSize;
            targetAxes.Left.MaximumSize = sourceAxes.Left.MaximumSize;
            targetAxes.Right.MinimumSize = sourceAxes.Right.MinimumSize;
            targetAxes.Right.MaximumSize = sourceAxes.Right.MaximumSize;

            targetAxes.Left.TickGenerator = new ScottPlot.TickGenerators.EmptyTickGenerator();
            targetAxes.Right.TickGenerator = new ScottPlot.TickGenerators.EmptyTickGenerator();
            //targetAxes.Bottom.TickGenerator = new ScottPlot.TickGenerators.EmptyTickGenerator();
            targetAxes.Bottom.Label.Text = string.Empty;
            targetAxes.Left.Label.Text = string.Empty;

            subplot.Plot.Axes.Left.IsVisible = false;
            subplot.Plot.Axes.Right.IsVisible = true;
            subplot.Plot.Axes.Bottom.IsVisible = false;
            subplot.Plot.Grid.YAxis = targetAxes.Right;
            subplot.ContextMenuStrip = null;

            ScottHelper.MatchRightAxisWidth(_pricePlot, subplot);
        }

        private void LinkPriceAxisToSubplot(FormsPlot subplot)
        {
            if (subplot is null || _pricePlot is null || _axisLinkHandlers.ContainsKey(subplot))
                return;

            try
            {
                bool syncing = false;

                void SyncAxes()
                {
                    var lim = _pricePlot.Plot.Axes.GetLimits();
                    subplot.Plot.Axes.SetLimitsX(lim.Left, lim.Right);
                    subplot.Refresh();
                }

                var manager = _pricePlot.Plot.RenderManager;
                if (manager is null)
                {
                    SyncAxes();
                    return;
                }

                EventHandler<ScottPlot.RenderDetails> handler = (s, e) =>
                {
                    if (syncing)
                        return;

                    syncing = true;
                    try
                    {
                        SyncAxes();
                    }
                    finally
                    {
                        syncing = false;
                    }
                };

                manager.AxisLimitsChanged += handler;
                _axisLinkHandlers[subplot] = handler;
                SyncAxes();
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Unable to link subplot axis to price chart; manual sync only.");
            }
        }

        private void CleanupAxisLinks()
        {
            foreach (var kv in _axisLinkHandlers)
            {
                try
                {
                    if (kv.Value is { } handler)
#pragma warning disable CS8601
                        _pricePlot.Plot.RenderManager.AxisLimitsChanged -= handler;
#pragma warning restore CS8601
                }
                catch { }
            }
            _axisLinkHandlers.Clear();
        }

        private Subplot AddSubplot(SubplotDefinition definition)
        {
            var plot = new FormsPlot { Dock = DockStyle.Fill, Visible = definition.DefaultVisible };
            _subPlotService.PrepareSubPlot(plot);
            ConfigureSubplotHost(plot);
            LinkPriceAxisToSubplot(plot);
            var subplot = new Subplot(definition, plot);
            _subplots.Add(subplot);
            _subplotLookup[definition.Key] = subplot;
            int row = _subplots.Count; // price at row 0
            if (_plotsHost.RowCount < row + 1)
                _plotsHost.RowCount = row + 1;
            _plotsHost.Controls.Add(plot, 0, row);
            return subplot;
        }

        private void RegisterSubplots()
        {
            foreach (var definition in _subplotDefinitions)
                AddSubplot(definition);
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
            var subplot = EnsureSubplot(name);
            subplot.Plot.Visible = visible;
            ApplyRightAxisWidthToAll();
            RecalculateRowHeights();

            if (visible)
                _ = RefreshChartAsync();
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

        private void HandleDrawModeSelection(ChartDrawMode mode)
        {
            _chartInteractions.SetDrawMode(mode);
            UpdateStatusMode(mode);
            _logger.LogInformation("Draw mode {Mode} selected on {Symbol} {Timeframe}", mode, Symbol, Timeframe);
        }

        private void HandleUndo()
        {
            if (_chartInteractions.Undo())
                _logger.LogInformation("Undo drawing action on {Symbol} {Timeframe}", Symbol, Timeframe);
        }

        private void HandleRedo()
        {
            if (_chartInteractions.Redo())
                _logger.LogInformation("Redo drawing action on {Symbol} {Timeframe}", Symbol, Timeframe);
        }

        private void HandleDelete()
        {
            _chartInteractions.DeleteSelectedShapes();
            _pricePlot.Refresh();
            _logger.LogInformation("Delete selected drawings on {Symbol} {Timeframe}", Symbol, Timeframe);
        }

        private void ClearDrawings()
        {
            var manager = _chartInteractions.ShapeManager;
            var shapes = manager.Shapes.ToList();
            foreach (var shape in shapes)
                manager.DeleteShape(shape);

            _pricePlot.Refresh();
            _logger.LogInformation("Clear drawings: {Symbol} {Timeframe}", Symbol, Timeframe);
        }

        private void ToggleSnap(bool enabled)
        {
            _chartInteractions.SnapEnabled = enabled;
            _leftToolbar.SetSnapState(enabled);
            _logger.LogInformation("Snap {State} for {Symbol} {Timeframe}", enabled ? "enabled" : "disabled", Symbol, Timeframe);
        }

        private void HandleSnapModeSelection(SnapMode mode)
        {
            SetSnapMode(mode);
        }

        private void SetSnapMode(SnapMode mode)
        {
            _chartInteractions.SnapMode = mode;
            _leftToolbar.SetSnapMode(mode);
            _logger.LogInformation("Snap mode {Mode} for {Symbol} {Timeframe}", mode, Symbol, Timeframe);
        }

        private void HandleSaveAnnotations()
        {
            using var dialog = new SaveFileDialog
            {
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                FileName = $"{Symbol}_{Timeframe}_annotations.json",
                RestoreDirectory = true,
                Title = "Save Annotations"
            };

            if (dialog.ShowDialog(this) != DialogResult.OK)
                return;

            try
            {
                _chartInteractions.SaveShapesToFile(dialog.FileName);
                MessageBox.Show(this, $"Annotations saved to:\n{dialog.FileName}", "Annotations", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save annotations");
                MessageBox.Show(this, ex.Message, "Save Annotations", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void HandleLoadAnnotations()
        {
            using var dialog = new OpenFileDialog
            {
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                RestoreDirectory = true,
                Title = "Load Annotations"
            };

            if (dialog.ShowDialog(this) != DialogResult.OK)
                return;

            try
            {
                _chartInteractions.LoadShapesFromFile(dialog.FileName);
                _pricePlot.Refresh();
                MessageBox.Show(this, $"Annotations loaded from:\n{dialog.FileName}", "Annotations", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (FileNotFoundException ex)
            {
                MessageBox.Show(this, ex.Message, "Load Annotations", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load annotations");
                MessageBox.Show(this, ex.Message, "Load Annotations", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void ChartDocument_Load(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Symbol))
                Symbol = "XAUUSD";
            if (string.IsNullOrWhiteSpace(Timeframe))
                Timeframe = "M15";

            var symbols = BuildSymbolList();
            _topToolbar.Initialize(Symbol, Timeframe, symbols);
            UpdateTitle();
            RecalculateRowHeights();
            await RefreshChartAsync();
            ApplyRightAxisWidthToAll();
            SyncIndicatorsXFromPrice();
        }

        public async Task RefreshChartAsync()
        {
            try
            {
                _cts?.Cancel();
                _cts = new CancellationTokenSource();
                var ct = _cts.Token;

                var candles = await _quoteService.GetAsync(Symbol, Timeframe);
                if ((candles == null || candles.Count == 0) && await LoadQuotesFromSamplesAsync(Symbol, Timeframe))
                    candles = await _quoteService.GetAsync(Symbol, Timeframe);

                if (candles is null || candles.Count == 0)
                {
                    _pricePlot.Plot.Clear();
                    _pricePlot.Refresh();
                    ApplyRightAxisWidthToAll();
                    return;
                }

                // Keep reference of latest quotes for centralized renderer
                AppQuotes = candles;
                var interval = BrokerHelper.GetInterval(Timeframe);
                _ohlcQuotes = candles.ToOhlcs(interval);
                _chartInteractions.BindCandles(_ohlcQuotes);

                // Compute technicals using the injected service
                await _chartTechnicalService.IndicatorsCompute(Symbol, Timeframe, candles);

                // Render price chart using centralized chart service
                bool hasGap = true;
                candlestickPlot = await _chartService.LoadAndRender(_pricePlot!, Symbol, Timeframe, hasGap, AppQuotes);

                // Render visible subplots (tránh cross-thread: thao tác FormsPlot phải ở UI thread)
                foreach (var subplot in _subplots)
                {
                    if (!subplot.Plot.Visible)
                        continue;

                    await _subPlotService.LoadAndRender(
                        subplot.Plot,
                        candlestickPlot,
                        AppQuotes,
                        Symbol,
                        Timeframe,
                        subplot.Definition.Key);
                }

                ApplyRightAxisWidthToAll();
                SyncIndicatorsXFromPrice();
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

        private Subplot EnsureSubplot(string key)
        {
            if (_subplotLookup.TryGetValue(key, out var subplot))
                return subplot;

            var definition = _subplotDefinitions.FirstOrDefault(d => string.Equals(d.Key, key, StringComparison.OrdinalIgnoreCase));
            if (definition is null)
            {
                definition = new SubplotDefinition(key, false);
                _subplotDefinitions.Add(definition);
            }

            return AddSubplot(definition);
        }

        private List<SubplotDefinition> CreateDefaultSubplotDefinitions()
        {
            return new List<SubplotDefinition>
            {
                new("RSI", true),
                new("MACD", false),
                new("CCI", false),
                new("StochRSI", false)
            };
        }

        private void ChartDocument_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.Z && !e.Shift)
            {
                if (_chartInteractions.Undo())
                {
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                }
                return;
            }

            if ((e.Control && e.KeyCode == Keys.Y) || (e.Control && e.Shift && e.KeyCode == Keys.Z))
            {
                if (_chartInteractions.Redo())
                {
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                }
                return;
            }

            if (e.KeyCode == Keys.Delete)
            {
                HandleDelete();
                e.Handled = true;
                e.SuppressKeyPress = true;
                return;
            }

            if (e.KeyCode == Keys.Escape)
            {
                HandleDrawModeSelection(ChartDrawMode.None);
                e.Handled = true;
                e.SuppressKeyPress = true;
                return;
            }

            ChartDrawMode? mode = e.KeyCode switch
            {
                Keys.D1 or Keys.NumPad1 => ChartDrawMode.TrendLine,
                Keys.D2 or Keys.NumPad2 => ChartDrawMode.HorizontalLine,
                Keys.D3 or Keys.NumPad3 => ChartDrawMode.VerticalLine,
                Keys.D4 or Keys.NumPad4 => ChartDrawMode.Rectangle,
                Keys.D5 or Keys.NumPad5 => ChartDrawMode.Circle,
                Keys.D6 or Keys.NumPad6 => ChartDrawMode.FibonacciRetracement,
                Keys.D7 or Keys.NumPad7 => ChartDrawMode.FibonacciExtension,
                _ => null
            };

            if (mode.HasValue)
            {
                HandleDrawModeSelection(mode.Value);
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void UpdateStatusMode(ChartDrawMode mode)
        {
            var text = mode switch
            {
                ChartDrawMode.TrendLine => "Trend Line",
                ChartDrawMode.HorizontalLine => "Horizontal",
                ChartDrawMode.VerticalLine => "Vertical",
                ChartDrawMode.Rectangle => "Rectangle",
                ChartDrawMode.Circle => "Circle",
                ChartDrawMode.FibonacciRetracement => "Fib Retrace",
                ChartDrawMode.FibonacciExtension => "Fib Extension",
                _ => "None"
            };
            _statusModeLabel.Text = $"Mode: {text}";
        }

        private void UpdateStatusCoordinates(Coordinates? coords)
        {
            if (coords == null)
            {
                _statusCoordinatesLabel.Text = "X: -, Y: -";
                return;
            }

            _statusCoordinatesLabel.Text = $"X: {coords.Value.X:F2}, Y: {coords.Value.Y:F2}";
        }

        private void UpdateStatusShapeInfo(string info)
        {
            _statusShapeLabel.Text = info;
        }

        private async Task HandleDataSourceChangedAsync(CandleSource source)
        {
            _activeDataSource = source;
            switch (source)
            {
                case CandleSource.None:
                    _logger.LogInformation("Data source set to None for {Symbol} {Timeframe}", Symbol, Timeframe);
                    break;
                case CandleSource.TextFile:
                case CandleSource.ExcelFile:
                    if (await LoadQuotesFromSamplesAsync(Symbol, Timeframe))
                        await RefreshChartAsync();
                    else
                        MessageBox.Show(this, $"No local samples found for {Symbol} {Timeframe}.", "Data Source", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
                default:
                    MessageBox.Show(this, $"Data source {source} not implemented yet.", "Data Source", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
            }
        }

        private async Task<bool> LoadQuotesFromSamplesAsync(string symbol, string timeframe)
        {
            try
            {
                var quotes = await AppHelper.ReadFile(symbol, timeframe);
                if (quotes is not { Count: > 0 })
                    return false;

                _quoteService.AddOrUpdate(symbol, timeframe, quotes);
                _logger.LogInformation("Loaded {Count} quotes from samples for {Symbol} {Timeframe}", quotes.Count, symbol, timeframe);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load samples for {Symbol} {Timeframe}", symbol, timeframe);
                return false;
            }
        }

        private void RefreshAvailableSymbols()
        {
            var symbols = BuildSymbolList();
            if (symbols.Length == 0)
                return;

            _topToolbar.UpdateSymbols(symbols, Symbol);
        }

        private string[] BuildSymbolList()
        {
            var fromService = _quoteService.GetSymbols()?
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(s => s, StringComparer.OrdinalIgnoreCase)
                .ToList() ?? new List<string>();

            if (!string.IsNullOrWhiteSpace(Symbol) && !fromService.Any(s => string.Equals(s, Symbol, StringComparison.OrdinalIgnoreCase)))
                fromService.Insert(0, Symbol);

            if (fromService.Count == 0 && !string.IsNullOrWhiteSpace(Symbol))
                fromService.Add(Symbol);

            return fromService.ToArray();
        }

        private void OnQuotesAddedOrUpdated(string symbol, string timeframe, List<AppQuote> quotes)
        {
            RunOnUiThread(() =>
            {
                RefreshAvailableSymbols();

                if (!string.Equals(symbol, Symbol, StringComparison.OrdinalIgnoreCase))
                    return;
                if (!string.Equals(timeframe, Timeframe, StringComparison.OrdinalIgnoreCase))
                    return;

                _ = RefreshChartAsync();
            });
        }

        private void OnSymbolRemoved(string symbol)
        {
            RunOnUiThread(() =>
            {
                RefreshAvailableSymbols();

                if (!string.Equals(symbol, Symbol, StringComparison.OrdinalIgnoreCase))
                    return;

                _pricePlot.Plot.Clear();
                _pricePlot.Refresh();
            });
        }

        private void RunOnUiThread(Action action)
        {
            if (action == null || IsDisposed)
                return;

            if (InvokeRequired)
            {
                try
                {
                    BeginInvoke(action);
                }
                catch (ObjectDisposedException)
                {
                }
                catch (InvalidOperationException)
                {
                }
            }
            else
            {
                action();
            }
        }
    }
}
