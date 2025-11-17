using Cuckoo.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartPro
{
    public class SignalResult
    {
        public string? Symbol { get; set; }
        public string? Timeframe { get; set; }
        public SignalKind Kind { get; set; }
        public Dictionary<string, object> IndicatorResult { get; set; } = new();
        public Dictionary<string, object> Priority { get; set; } = new();
        public AppQuote? Quote { get; set; }
        public double CrossAt { get; set; } = double.NaN;
        public string? Note { get; set; }
        public string? Label { get; set; }
        public List<string> Tags { get; set; } = new();
        public double Score { get; set; } = 0.0;
    }
}
