using System;
using System.Drawing;

namespace TradingApp.WinUI.Models
{
    public class SignalViewModel
    {
        public Bitmap? Icon { get; set; }
        public DateTime Time { get; set; }

        public string Symbol { get; set; } = "";
        public string Timeframe { get; set; } = "";
        public double Price { get; set; }

        public string Title { get; set; } = "";
        public string Detail { get; set; } = "";
        public string Source { get; set; } = "";
        public bool IsNew { get; set; }
    }
}
