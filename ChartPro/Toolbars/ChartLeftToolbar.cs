using ChartPro.Charting;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace ChartPro.Toolbars
{
    public class ChartLeftToolbar : ToolStrip
    {
        private readonly ToolStripButton _btnUndo = null!;
        private readonly ToolStripButton _btnRedo = null!;
        private readonly ToolStripButton _btnDelete = null!;
        private readonly ToolStripButton _btnClearAll = null!;
        private readonly ToolStripButton _btnSnap = null!;
        private readonly ToolStripDropDownButton _btnSnapMode = null!;
        private readonly ToolStripButton _btnSaveAnnotations = null!;
        private readonly ToolStripButton _btnLoadAnnotations = null!;
        private readonly Dictionary<ChartDrawMode, ToolStripButton> _drawButtons = new();
        private readonly Dictionary<SnapMode, ToolStripMenuItem> _snapModeItems = new();

        public event Action<ChartDrawMode>? DrawModeSelected;
        public event Action? UndoRequested;
        public event Action? RedoRequested;
        public event Action? DeleteRequested;
        public event Action? ClearRequested;
        public event Action<bool>? SnapToggled;
        public event Action<SnapMode>? SnapModeSelected;
        public event Action? SaveAnnotationsRequested;
        public event Action? LoadAnnotationsRequested;

        public ChartLeftToolbar()
        {
            GripStyle = ToolStripGripStyle.Hidden;
            Dock = DockStyle.Left;
            LayoutStyle = ToolStripLayoutStyle.VerticalStackWithOverflow;
            AutoSize = false;
            Width = 160;
            RenderMode = ToolStripRenderMode.System;
            Padding = new Padding(6, 8, 6, 8);
            CanOverflow = false;
            Stretch = true;
            GripMargin = new Padding(0);

            Items.Add(CreateSectionLabel("Drawing Tools"));
            Items.AddRange(new ToolStripItem[]
            {
                CreateDrawButton("Select / Move", ChartDrawMode.None),
                CreateDrawButton("Trend Line", ChartDrawMode.TrendLine),
                CreateDrawButton("Horizontal", ChartDrawMode.HorizontalLine),
                CreateDrawButton("Vertical", ChartDrawMode.VerticalLine),
                CreateDrawButton("Rectangle", ChartDrawMode.Rectangle),
                CreateDrawButton("Circle", ChartDrawMode.Circle),
                CreateDrawButton("Fib Retrace", ChartDrawMode.FibonacciRetracement),
                CreateDrawButton("Fib Extension", ChartDrawMode.FibonacciExtension)
            });

            Items.Add(new ToolStripSeparator { Margin = new Padding(0, 8, 0, 4) });
            Items.Add(CreateSectionLabel("Edit"));
            _btnUndo = BuildButton("Undo", (s, e) => UndoRequested?.Invoke());
            _btnRedo = BuildButton("Redo", (s, e) => RedoRequested?.Invoke());
            _btnDelete = BuildButton("Delete", (s, e) => DeleteRequested?.Invoke());
            _btnClearAll = BuildButton("Clear All", (s, e) => ClearRequested?.Invoke());
            Items.AddRange(new ToolStripItem[] { _btnUndo, _btnRedo, _btnDelete, _btnClearAll });

            Items.Add(new ToolStripSeparator { Margin = new Padding(0, 8, 0, 4) });
            Items.Add(CreateSectionLabel("Snap"));
            _btnSnap = BuildButton("Snap", (s, e) => SnapToggled?.Invoke(_btnSnap.Checked));
            _btnSnap.CheckOnClick = true;
            Items.Add(_btnSnap);
            Items.Add(new ToolStripSeparator());
            _btnSnapMode = new ToolStripDropDownButton("Snap Mode")
            {
                AutoToolTip = false,
                DisplayStyle = ToolStripItemDisplayStyle.Text
            };
            AddSnapMenuItem("No Snap", SnapMode.None);
            AddSnapMenuItem("Snap Price", SnapMode.Price);
            AddSnapMenuItem("Snap Candle", SnapMode.CandleOHLC);
            _btnSnapMode.Text = "Snap: None";
            Items.Add(_btnSnapMode);

            Items.Add(new ToolStripSeparator { Margin = new Padding(0, 8, 0, 4) });
            Items.Add(CreateSectionLabel("Annotations"));
            _btnSaveAnnotations = BuildButton("Save...", (s, e) => SaveAnnotationsRequested?.Invoke());
            _btnLoadAnnotations = BuildButton("Load...", (s, e) => LoadAnnotationsRequested?.Invoke());
            Items.Add(_btnSaveAnnotations);
            Items.Add(_btnLoadAnnotations);

            SetActiveDrawMode(ChartDrawMode.None);
        }

        private ToolStripLabel CreateSectionLabel(string text)
        {
            return new ToolStripLabel(text)
            {
                Font = new Font(Font, FontStyle.Bold),
                ForeColor = SystemColors.ControlDarkDark,
                Margin = new Padding(0, 6, 0, 2)
            };
        }

        private ToolStripButton CreateDrawButton(string text, ChartDrawMode mode)
        {
            var button = new ToolStripButton(text)
            {
                Tag = mode,
                AutoToolTip = false,
                DisplayStyle = ToolStripItemDisplayStyle.Text,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoSize = false,
                Width = 140,
                CheckOnClick = false,
                Margin = new Padding(0, 0, 0, 2)
            };
            button.Click += (s, e) => OnDrawButtonClicked(mode);
            _drawButtons[mode] = button;
            return button;
        }

        private ToolStripButton BuildButton(string text, EventHandler onClick)
        {
            var button = new ToolStripButton(text)
            {
                DisplayStyle = ToolStripItemDisplayStyle.Text,
                AutoToolTip = false,
                AutoSize = false,
                Width = 140,
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(0, 0, 0, 2)
            };
            button.Click += onClick;
            return button;
        }

        private void OnDrawButtonClicked(ChartDrawMode mode)
        {
            SetActiveDrawMode(mode);
            DrawModeSelected?.Invoke(mode);
        }

        private void AddSnapMenuItem(string text, SnapMode mode)
        {
            var item = new ToolStripMenuItem(text)
            {
                Tag = mode,
                CheckOnClick = true
            };
            item.Click += (s, e) => OnSnapModeClicked(mode);
            _btnSnapMode.DropDownItems.Add(item);
            _snapModeItems[mode] = item;
        }

        public void SetActiveDrawMode(ChartDrawMode mode)
        {
            foreach (var kvp in _drawButtons)
                kvp.Value.Checked = kvp.Key == mode;
        }

        public void SetSnapState(bool enabled)
        {
            _btnSnap.Checked = enabled;
        }

        private void OnSnapModeClicked(SnapMode mode)
        {
            SetSnapMode(mode);
            SnapModeSelected?.Invoke(mode);
        }

        public void SetSnapMode(SnapMode mode)
        {
            foreach (var kvp in _snapModeItems)
                kvp.Value.Checked = kvp.Key == mode;

            var label = mode switch
            {
                SnapMode.Price => "Snap: Price",
                SnapMode.CandleOHLC => "Snap: Candle",
                _ => "Snap: None"
            };
            _btnSnapMode.Text = label;
        }
    }
}
