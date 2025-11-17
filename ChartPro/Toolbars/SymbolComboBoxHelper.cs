using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChartPro
{
    public static class SymbolComboBoxHelper
    {
        public static readonly string[] DefaultSymbols =
        {
            "XAUUSD", "BTCUSD", "DXY", "EURUSD", "GBPUSD",
            "USTEC", "US30", "US500", "XAGUSD", "USOIL"
        };

        public static ToolStripComboBox Create(
            IEnumerable<string> symbols,
            Func<string, Task> onSelectionChangedAsync,
            string? selected = null,
            int width = 120,
            ComboBoxStyle style = ComboBoxStyle.DropDownList,
            string? name = "cbSymbol")
        {
            var cb = new ToolStripComboBox
            {
                Name = name ?? "cbSymbol",
                DropDownStyle = style,
                AutoSize = false,
                Width = width
            };

            SetSymbols(cb, symbols, selected);

            cb.SelectedIndexChanged += async (s, e) =>
            {
                if (cb.SelectedItem is string sym)
                {
                    try { await onSelectionChangedAsync(sym); }
                    catch { /* keep UI responsive if handler throws */ }
                }
            };

            return cb;
        }

        public static ToolStripComboBox AddToToolStrip(
            ToolStrip toolStrip,
            IEnumerable<string> symbols,
            Func<string, Task> onSelectionChangedAsync,
            string labelText = "Symbol:",
            string? selected = null,
            int width = 120,
            ComboBoxStyle style = ComboBoxStyle.DropDownList,
            string? name = "cbSymbol")
        {
            if (!string.IsNullOrWhiteSpace(labelText))
                toolStrip.Items.Add(new ToolStripLabel(labelText));

            var cb = Create(symbols, onSelectionChangedAsync, selected, width, style, name);
            toolStrip.Items.Add(cb);
            return cb;
        }

        public static void SetSymbols(ToolStripComboBox cb, IEnumerable<string> symbols, string? selected = null)
        {
            cb.Items.Clear();
            var list = symbols?
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToArray() ?? Array.Empty<string>();
            if (list.Length > 0)
                cb.Items.AddRange(list);

            if (!string.IsNullOrWhiteSpace(selected) && list.Contains(selected))
                cb.SelectedItem = selected;
            else if (cb.Items.Count > 0)
                cb.SelectedIndex = 0;
        }

        public static void TrySelect(ToolStripComboBox cb, string symbol)
        {
            if (cb.Items.Contains(symbol))
                cb.SelectedItem = symbol;
        }

        public static string? GetSelected(ToolStripComboBox cb) => cb.SelectedItem as string;
    }
}