using System;
using System.Drawing;

namespace TradingApp.WinUI.Models
{
    public class SymbolViewModel
    {
        public string Symbol { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public Bitmap? Icon { get; set; }

        public double Bid { get; set; }
        public double Ask { get; set; }
        public double Spread { get; set; }
        public double LastPrice { get; set; }
        public double Volume { get; set; }
        public double BidSize { get; set; }
        public double AskSize { get; set; }
        public double ChangePercent { get; set; }
        public double ATR { get; set; }
        public string Session { get; set; } = "";
        public string Description { get; set; } = "";
        public DateTime LastUpdate { get; set; }
    }
}
