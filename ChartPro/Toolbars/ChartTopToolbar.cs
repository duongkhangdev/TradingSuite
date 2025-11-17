using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace ChartPro.Toolbars
{
    public class ChartTopToolbar : ToolStrip
    {
        private readonly ToolStripComboBox _cbDataSource;
        private readonly ToolStripComboBox _cbSymbol;
        private readonly ToolStripLabel _lblSymbol;
        private readonly ToolStripLabel _lblTimeframe;
        private readonly ToolStripButton _btnRefresh;
        private readonly ToolStripDropDownButton _ddIndicators;
        private readonly string[] _timeframes = new[] {"M1","M5","M15","M30","H1","H4","D1","W1","MN1"};
        private ToolStripButton? _activeTfButton;

        public event Action<string>? TimeframeSelected;
        public event Action<string>? SymbolChanged;
        public event Action? RefreshRequested;
        public event Action<string, bool>? IndicatorToggled; // name, isVisible

        public ChartTopToolbar()
        {
            GripStyle = ToolStripGripStyle.Hidden;
            Dock = DockStyle.Top;
            RenderMode = ToolStripRenderMode.System;
            ImageScalingSize = new Size(16,16);

            // DataSource combo
            _cbDataSource = new ToolStripComboBox
            {
                Name = "cbDataSource",
                DropDownStyle = ComboBoxStyle.DropDownList,
                AutoSize = false,
                Width = 120
            };
            _cbDataSource.Items.Add("TextFile");
            _cbDataSource.Items.Add("Demo");
            _cbDataSource.SelectedIndex = 0;
            Items.Add(new ToolStripLabel("DataSource:"));
            Items.Add(_cbDataSource);
            Items.Add(new ToolStripSeparator());

            // Symbol combo
            _lblSymbol = new ToolStripLabel("Symbol:");
            _cbSymbol = new ToolStripComboBox
            {
                Name = "cbSymbol",
                DropDownStyle = ComboBoxStyle.DropDownList,
                AutoSize = false,
                Width = 100
            };
            _cbSymbol.SelectedIndexChanged += (s, e) =>
            {
                if (_cbSymbol.SelectedItem is string sym)
                    SymbolChanged?.Invoke(sym);
            };
            Items.Add(_lblSymbol);
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
            AddIndicatorToggle("RSI");
            AddIndicatorToggle("MACD");
            AddIndicatorToggle("CCI");
            AddIndicatorToggle("StochRSI");
            Items.Add(_ddIndicators);            
        }

        private void AddIndicatorToggle(string name)
        {
            var mi = new ToolStripMenuItem(name)
            {
                Checked = name == "RSI", // ví dụ mặc định chỉ RSI bật
                CheckOnClick = true
            };
            mi.CheckedChanged += (s, e) => IndicatorToggled?.Invoke(name, mi.Checked);
            _ddIndicators.DropDownItems.Add(mi);
        }

        public void Initialize(string symbol, string timeframe, string[]? symbols = null)
        {
            // Populate symbols
            _cbSymbol.Items.Clear();
            if (symbols != null && symbols.Length > 0)
            {
                foreach (var s in symbols.Distinct())
                    _cbSymbol.Items.Add(s);
            }
            else
            {
                _cbSymbol.Items.Add(symbol);
            }
            _cbSymbol.SelectedItem = symbol;
            SetActiveTimeframe(timeframe);
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
