using System;
using System.Drawing;

namespace TradingApp.WinUI.Models
{
    public class OrderViewModel
    {
        public Guid OrderId { get; set; }
        public Bitmap? SideIcon { get; set; }
        public string Symbol { get; set; } = "";
        public string Side { get; set; } = "";
        public string Type { get; set; } = "";
        public string Status { get; set; } = "";

        public double Lots { get; set; }
        public double Price { get; set; }
        public double SL { get; set; }
        public double TP { get; set; }

        public DateTime CreatedTime { get; set; }
        public DateTime? ExpireTime { get; set; }

        public string Comment { get; set; } = "";
    }
}
