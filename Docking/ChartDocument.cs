using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ScottPlot;
using ScottPlot.WinForms;
using WeifenLuo.WinFormsUI.Docking;

namespace TradingApp.WinUI.Docking
{
    public class ChartDocument : DockContent
    {
        private readonly FormsPlot _formsPlot;
        private readonly ToolStrip _toolStrip;

        public string Symbol { get; }
        public string Timeframe { get; }

        public ChartDocument(string symbol, string timeframe)
        {
            //AutoScaleMode = AutoScaleMode.Dpi;
            //DockAreas = DockAreas.Document | DockAreas.Float;

            Symbol = symbol;
            Timeframe = timeframe;

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
            btnRefresh.Click += (s, e) => RefreshChart();

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
        }

        private void ChartDocument_Load(object? sender, EventArgs e)
        {
            var plt = _formsPlot.Plot;
            plt.Clear();

            double[] xs = Enumerable.Range(0, 200).Select(i => (double)i).ToArray();
            double[] ys = xs.Select(x => Math.Sin(x / 10.0)).ToArray();

            plt.Add.Scatter(xs, ys);
            plt.Axes.AutoScale();

            _formsPlot.Refresh();
        }

        public void RefreshChart()
        {
            _formsPlot.Refresh();
        }

        protected override string GetPersistString()
        {
            return $"{nameof(ChartDocument)};{Symbol};{Timeframe}";
        }
    }
}
