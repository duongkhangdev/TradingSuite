using System;
using System.Drawing;

namespace TradingApp.WinUI.Models
{
    public class PositionViewModel
    {
        public Bitmap? SideIcon { get; set; }
        public string Symbol { get; set; } = "";
        public string Side { get; set; } = "";
        public double Lots { get; set; }

        public double EntryPrice { get; set; }
        public double SL { get; set; }
        public double TP { get; set; }
        public double CurrentPrice { get; set; }

        public double Pnl { get; set; }
        public double PnlPercent { get; set; }
        public string Comment { get; set; } = "";
        public DateTime OpenTime { get; set; }
    }
}
