using Cuckoo.Shared;
using Skender.Stock.Indicators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingSuite.Charting.Indicators
{
    public static partial class IndicatorExtensions
    {
        // --- Pivots --------------------------
        public static List<PivotsResult>? GetPivotsResults(this IEnumerable<AppQuote> quotes,
            int leftSpan = 20,
            int rightSpan = 20,
            int maxTrendPeriods = 50,
            EndType endType = EndType.HighLow)
        {
            if (quotes.IsNullOrEmpty()) return null;

            var result = quotes.GetPivots(leftSpan, rightSpan, maxTrendPeriods, endType);
            return result?.Where(o =>
            o.HighPoint.HasValue || o.HighLine.HasValue || o.HighTrend.HasValue ||
            o.LowPoint.HasValue || o.LowLine.HasValue || o.LowTrend.HasValue)?.ToList();
        }

        public static PivotsResult? GetLastPivotsResult(this IEnumerable<AppQuote> quotes,
            int leftSpan = 2,
            int rightSpan = 2,
            int maxTrendPeriods = 20,
            EndType endType = EndType.HighLow)
        {
            if (quotes.IsNullOrEmpty()) return null;

            var result = quotes.GetPivotsResults(leftSpan, rightSpan, maxTrendPeriods, endType);
            return result?.LastOrDefault();
        }

        // --- Fractal: left-right span --------------------------
        public static List<FractalResult>? GetFractalResults(this IEnumerable<AppQuote> quotes,
            int leftSpan,
            int rightSpan,
            EndType endType = EndType.HighLow)
        {
            if (quotes.IsNullOrEmpty()) return null;

            var result = quotes.GetFractal(leftSpan, rightSpan, endType);
            return result?.Where(o => o.FractalBull.HasValue || o.FractalBear.HasValue)?.ToList();
        }

        public static FractalResult? GetLastFractalResult(this IEnumerable<AppQuote> quotes,
            int leftSpan,
            int rightSpan,
            EndType endType = EndType.HighLow)
        {
            if (quotes.IsNullOrEmpty()) return null;

            var result = quotes.GetFractalResults(leftSpan, rightSpan, endType);
            return result?.LastOrDefault();
        }

        // --- Fractal: window span --------------------------
        public static List<FractalResult>? GetFractalResults(this IEnumerable<AppQuote> quotes,
            int windowSpan = 2,
            EndType endType = EndType.HighLow)
        {
            if (quotes.IsNullOrEmpty()) return null;

            var result = quotes.GetFractal(windowSpan, endType);
            return result?.Where(o => o.FractalBull.HasValue || o.FractalBear.HasValue)?.ToList();
        }

        public static FractalResult? GetLastFractalResult(this IEnumerable<AppQuote> quotes,
            int windowSpan = 2,
            EndType endType = EndType.HighLow)
        {
            if (quotes.IsNullOrEmpty()) return null;

            var result = quotes.GetFractalResults(windowSpan, endType);
            return result?.LastOrDefault();
        }
    }
}
