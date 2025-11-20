using ChartPro.Charting;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace ChartPro.Toolbars
{
    public class ChartLeftToolbar : ToolStrip
    {
        private readonly ToolStripDropDownButton _btnDraw = null!;
        private readonly ToolStripButton _btnUndo = null!;
        private readonly ToolStripButton _btnRedo = null!;
        private readonly ToolStripButton _btnDelete = null!;
        private readonly ToolStripButton _btnClearAll = null!;
        private readonly ToolStripButton _btnSnap = null!;
        private readonly ToolStripDropDownButton _btnSnapMode = null!;
        private readonly ToolStripDropDownButton _btnAnnotations = null!;
        private readonly Dictionary<ChartDrawMode, ToolStripMenuItem> _drawItems = new();
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
            Width = 80;
            RenderMode = ToolStripRenderMode.System;

            _btnDraw = new ToolStripDropDownButton("Draw")
            {
                AutoToolTip = false,
                DisplayStyle = ToolStripItemDisplayStyle.Text,
            };

            AddDrawMenuItem("Select/Move", ChartDrawMode.None);
            _btnDraw.DropDownItems.Add(new ToolStripSeparator());
            AddDrawMenuItem("Trend Line", ChartDrawMode.TrendLine);
            AddDrawMenuItem("Horizontal Line", ChartDrawMode.HorizontalLine);
            AddDrawMenuItem("Vertical Line", ChartDrawMode.VerticalLine);
            AddDrawMenuItem("Rectangle", ChartDrawMode.Rectangle);
            AddDrawMenuItem("Circle", ChartDrawMode.Circle);
            AddDrawMenuItem("Fib Retracement", ChartDrawMode.FibonacciRetracement);
            AddDrawMenuItem("Fib Extension", ChartDrawMode.FibonacciExtension);

            _btnUndo = BuildButton("Undo", (s, e) => UndoRequested?.Invoke());
            _btnRedo = BuildButton("Redo", (s, e) => RedoRequested?.Invoke());
            _btnDelete = BuildButton("Delete", (s, e) => DeleteRequested?.Invoke());
            _btnClearAll = BuildButton("Clear All", (s, e) => ClearRequested?.Invoke());

            _btnSnap = BuildButton("Snap", (s, e) => SnapToggled?.Invoke(_btnSnap.Checked));
            _btnSnap.CheckOnClick = true;

            _btnSnapMode = new ToolStripDropDownButton("Snap Mode")
            {
                AutoToolTip = false,
                DisplayStyle = ToolStripItemDisplayStyle.Text
            };
            AddSnapMenuItem("No Snap", SnapMode.None);
            AddSnapMenuItem("Snap Price", SnapMode.Price);
            AddSnapMenuItem("Snap Candle", SnapMode.CandleOHLC);
            _btnSnapMode.Text = "Snap: None";

            _btnAnnotations = new ToolStripDropDownButton("Annotations")
            {
                AutoToolTip = false,
                DisplayStyle = ToolStripItemDisplayStyle.Text
            };
            var saveItem = new ToolStripMenuItem("Save...");
            saveItem.Click += (s, e) => SaveAnnotationsRequested?.Invoke();
            var loadItem = new ToolStripMenuItem("Load...");
            loadItem.Click += (s, e) => LoadAnnotationsRequested?.Invoke();
            _btnAnnotations.DropDownItems.Add(saveItem);
            _btnAnnotations.DropDownItems.Add(loadItem);

            Items.Add(_btnDraw);
            Items.Add(new ToolStripSeparator());
            Items.Add(_btnUndo);
            Items.Add(_btnRedo);
            Items.Add(_btnDelete);
            Items.Add(_btnClearAll);
            Items.Add(new ToolStripSeparator());
            Items.Add(_btnSnap);
            Items.Add(_btnSnapMode);
            Items.Add(_btnAnnotations);
        }

        private void AddDrawMenuItem(string text, ChartDrawMode mode)
        {
            var item = new ToolStripMenuItem(text)
            {
                Tag = mode,
                CheckOnClick = true
            };

            item.Click += (s, e) => OnDrawItemClicked(mode);
            _btnDraw.DropDownItems.Add(item);
            _drawItems[mode] = item;
        }

        private ToolStripButton BuildButton(string text, EventHandler onClick)
        {
            var button = new ToolStripButton(text)
            {
                DisplayStyle = ToolStripItemDisplayStyle.Text,
                AutoToolTip = false
            };
            button.Click += onClick;
            return button;
        }

        private void OnDrawItemClicked(ChartDrawMode mode)
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
            foreach (var kvp in _drawItems)
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
