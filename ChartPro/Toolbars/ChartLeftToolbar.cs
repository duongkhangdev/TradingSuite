using System;
using System.Drawing;
using System.Windows.Forms;

namespace ChartPro.Toolbars
{
    public class ChartLeftToolbar : ToolStrip
    {
        private readonly ToolStripDropDownButton _btnDraw;
        private readonly ToolStripMenuItem _miPattern;
        private readonly ToolStripMenuItem _miIndicator;
        private readonly ToolStripMenuItem _miLine;
        private readonly ToolStripMenuItem _miClear;

        public event Action<string>? DrawToolSelected;
        public event Action? ClearRequested;

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
                DisplayStyle = ToolStripItemDisplayStyle.Text
            };

            _miPattern = new ToolStripMenuItem("Pattern");
            _miIndicator = new ToolStripMenuItem("Indicator");
            _miLine = new ToolStripMenuItem("Line");
            _miClear = new ToolStripMenuItem("Clear");

            _miPattern.Click += (s, e) => DrawToolSelected?.Invoke("Pattern");
            _miIndicator.Click += (s, e) => DrawToolSelected?.Invoke("Indicator");
            _miLine.Click += (s, e) => DrawToolSelected?.Invoke("Line");
            _miClear.Click += (s, e) => ClearRequested?.Invoke();

            _btnDraw.DropDownItems.Add(_miPattern);
            _btnDraw.DropDownItems.Add(_miIndicator);
            _btnDraw.DropDownItems.Add(_miLine);
            _btnDraw.DropDownItems.Add(new ToolStripSeparator());
            _btnDraw.DropDownItems.Add(_miClear);

            Items.Add(_btnDraw);
        }
    }
}
