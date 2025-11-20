namespace TradingApp.WinUI.Models
{
    public class AccountViewModel
    {
        public string Broker { get; set; } = "";
        public string AccountId { get; set; } = "";
        public string Name { get; set; } = "";
        public string Type { get; set; } = "";
        public string Currency { get; set; } = "USD";
        public string Leverage { get; set; } = "";

        public double Balance { get; set; }
        public double Equity { get; set; }
        public double FreeMargin { get; set; }
        public double MarginUsed { get; set; }
        public double MarginLevel { get; set; }
        public bool IsCurrent { get; set; }

        public string Description { get; set; } = "";
    }
}
