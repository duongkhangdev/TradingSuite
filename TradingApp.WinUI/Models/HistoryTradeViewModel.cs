using System;

namespace TradingApp.WinUI.Models
{
    public class HistoryTradeViewModel
    {
        public string Symbol { get; set; } = "";
        public string Side { get; set; } = "";
        public double Lots { get; set; }

        public double EntryPrice { get; set; }
        public double ExitPrice { get; set; }
        public double SL { get; set; }
        public double TP { get; set; }

        public double Pnl { get; set; }
        public double PnlPercent { get; set; }

        public DateTime OpenTime { get; set; }
        public DateTime CloseTime { get; set; }

        public string Strategy { get; set; } = "";
        public string Comment { get; set; } = "";
    }
}
