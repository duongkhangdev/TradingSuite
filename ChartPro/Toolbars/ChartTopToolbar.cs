using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks; // for Task in symbol async handler
using System.Windows.Forms;
using ChartPro; // SymbolComboBoxHelper namespace

namespace ChartPro.Toolbars
{
    public class ChartTopToolbar : ToolStrip
    {
        private readonly ToolStripDropDownButton _ddDataSource;
        private readonly ToolStripComboBox _cbSymbol; // using helper now
        private readonly ToolStripLabel _lblTimeframe;
        private readonly ToolStripButton _btnRefresh;
        private readonly ToolStripDropDownButton _ddIndicators;
        private readonly string[] _timeframes = new[] {"M1","M5","M15","M30","H1","H4","D1","W1","MN1"};
        private ToolStripButton? _activeTfButton;

        public event Action<string>? TimeframeSelected;
        public event Action<string>? SymbolChanged;
        public event Action? RefreshRequested;
        public event Action<string, bool>? IndicatorToggled; // name, isVisible
        public event Action<CandleSource, string>? DataSourceChanged; // data source selected

        public sealed record IndicatorToggleDefinition(string Name, bool IsChecked);

        public ChartTopToolbar()
        {
            GripStyle = ToolStripGripStyle.Hidden;
            Dock = DockStyle.Top;
            RenderMode = ToolStripRenderMode.System;
            ImageScalingSize = new Size(16,16);

            // Data Source dropdown
            _ddDataSource = new ToolStripDropDownButton("Data Source")
            {
                Tag = "DataSource",
                AutoToolTip = false,
                DisplayStyle = ToolStripItemDisplayStyle.Text
            };
            AddDataSourceItem("None", CandleSource.None);
            AddDataSourceItem($"{CandleSource.TextFile}", CandleSource.TextFile);
            AddDataSourceItem($"{CandleSource.ExcelFile}", CandleSource.ExcelFile);
            AddDataSourceItem($"{CandleSource.SignalR}", CandleSource.SignalR);
            AddDataSourceItem($"{CandleSource.Websocket}", CandleSource.Websocket);
            AddDataSourceItem($"{CandleSource.RestApi}", CandleSource.RestApi);
            Items.Add(_ddDataSource);
            Items.Add(new ToolStripSeparator());

            // Symbol combo via helper (async handler bridged to synchronous event)
            Items.Add(new ToolStripLabel("Symbol:"));
            _cbSymbol = SymbolComboBoxHelper.Create(
                SymbolComboBoxHelper.DefaultSymbols,
                onSelectionChangedAsync: sym =>
                {
                    SymbolChanged?.Invoke(sym);
                    return Task.CompletedTask;
                },
                selected: SymbolComboBoxHelper.DefaultSymbols.FirstOrDefault() ?? "" ,
                width: 100);
            Items.Add(_cbSymbol);
            Items.Add(new ToolStripSeparator());

            // Timeframe buttons label
            _lblTimeframe = new ToolStripLabel("Timeframe:");
            Items.Add(_lblTimeframe);

            foreach (var tf in _timeframes)
            {
                var btn = new ToolStripButton(tf)
                {
                    Tag = tf,
                    AutoToolTip = false,
                    DisplayStyle = ToolStripItemDisplayStyle.Text
                };
                btn.Click += (s, e) => SetActiveTimeframe((string)btn.Tag!);
                Items.Add(btn);
            }
            Items.Add(new ToolStripSeparator());

            _btnRefresh = new ToolStripButton("Refresh")
            {
                DisplayStyle = ToolStripItemDisplayStyle.Text
            };
            _btnRefresh.Click += (s, e) => RefreshRequested?.Invoke();
            Items.Add(_btnRefresh);

            Items.Add(new ToolStripSeparator());

            // Indicators dropdown
            _ddIndicators = new ToolStripDropDownButton("Indicators")
            {
                AutoToolTip = false,
                DisplayStyle = ToolStripItemDisplayStyle.Text
            };
            Items.Add(_ddIndicators);
            ConfigureIndicatorToggles(new[]
            {
                new IndicatorToggleDefinition("RSI", true),
                new IndicatorToggleDefinition("MACD", false),
                new IndicatorToggleDefinition("CCI", false),
                new IndicatorToggleDefinition("StochRSI", false)
            });
        }

        private void AddDataSourceItem(string text, CandleSource source)
        {
            var mi = new ToolStripMenuItem(text);
            mi.Click += (s, e) =>
            {
                DataSourceChanged?.Invoke(source, text);
            };
            _ddDataSource.DropDownItems.Add(mi);
        }

        private void AddIndicatorToggle(string name, bool isChecked = false)
        {
            var mi = new ToolStripMenuItem(name)
            {
                Checked = isChecked,
                CheckOnClick = true
            };
            mi.CheckedChanged += (s, e) => IndicatorToggled?.Invoke(name, mi.Checked);
            _ddIndicators.DropDownItems.Add(mi);
        }

        public void ConfigureIndicatorToggles(IEnumerable<IndicatorToggleDefinition> definitions)
        {
            _ddIndicators.DropDownItems.Clear();
            if (definitions == null)
                return;

            foreach (var def in definitions)
                AddIndicatorToggle(def.Name, def.IsChecked);
        }

        public void Initialize(string symbol, string timeframe, string[]? symbols = null)
        {
            // Update symbols using helper
            var list = symbols?.Distinct().ToArray() ?? new[] { symbol };
            SymbolComboBoxHelper.SetSymbols(_cbSymbol, list, symbol);
            SetActiveTimeframe(timeframe);
        }

        public void UpdateSymbols(string[] symbols, string? selected = null)
        {
            SymbolComboBoxHelper.SetSymbols(_cbSymbol, symbols, selected);
        }

        private void SetActiveTimeframe(string tf)
        {
            foreach (var item in Items)
            {
                if (item is ToolStripButton b && _timeframes.Contains(b.Text))
                {
                    b.BackColor = SystemColors.Control;
                    b.ForeColor = SystemColors.ControlText;
                    b.Font = new Font(Font, FontStyle.Regular);
                }
            }
            _activeTfButton = Items.OfType<ToolStripButton>().FirstOrDefault(b => (string?)b.Tag == tf);
            if (_activeTfButton != null)
            {
                _activeTfButton.BackColor = Color.Yellow;
                _activeTfButton.Font = new Font(Font, FontStyle.Bold);
            }
            TimeframeSelected?.Invoke(tf);
        }
    }
}
