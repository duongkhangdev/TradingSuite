using Cuckoo.Shared;
using Skender.Stock.Indicators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartPro
{
    public static partial class IndicatorExtensions
    {
        // --- Beta --------------------------------------
        public static List<BetaResult>? GetBetaResults(this IEnumerable<AppQuote> quotes,
            int lookbackPeriods = 50,
            BetaType type = BetaType.Standard)
        {
            if (quotes.IsNullOrEmpty() || quotes.Count() <= lookbackPeriods) return null;

            try
            {
                return quotes.GetBeta(quotes, lookbackPeriods, type)
                    ?.Where(o => o.Beta.HasValue)
                    ?.OrderBy(x => x.Date)
                    ?.ToList();
            }
            catch
            {
                return null;
            }
        }

        public static BetaResult? GetLastBetaResult(this IEnumerable<AppQuote> quotes,
            int lookbackPeriods = 50,
            BetaType type = BetaType.Standard)
        {
            if (quotes.IsNullOrEmpty() || quotes.Count() <= lookbackPeriods) return null;

            var result = quotes.GetBetaResults(lookbackPeriods, type);
            return result?.LastOrDefault();
        }

        // --- Corr --------------------------------------
        public static List<CorrResult>? GetCorrResults(this IEnumerable<AppQuote> quotes, int lookbackPeriods = 20)
        {
            if (quotes.IsNullOrEmpty() || quotes.Count() <= lookbackPeriods) return null;

            try
            {
                return quotes.GetCorrelation(quotes, lookbackPeriods)
                    ?.Where(o => o.Correlation.HasValue)
                    ?.OrderBy(x => x.Date)
                    ?.ToList();
            }
            catch
            {
                return null;
            }
        }

        public static CorrResult? GetLastCorrResult(this IEnumerable<AppQuote> quotes, int lookbackPeriods = 20)
        {
            if (quotes.IsNullOrEmpty() || quotes.Count() <= lookbackPeriods) return null;

            var result = quotes.GetCorrResults(lookbackPeriods);
            return result?.LastOrDefault();
        }

        // --- LinearRegression --------------------------------------
        public static List<SlopeResult>? GetLinearRegressionResults(this IEnumerable<AppQuote> quotes, int lookbackPeriods = 100)
        {
            if (quotes.IsNullOrEmpty() || quotes.Count() <= lookbackPeriods) return null;

            return quotes.GetSlope(lookbackPeriods)
                ?.Where(o => o.Line.HasValue)
                ?.OrderBy(x => x.Date)
                ?.ToList();
        }

        public static SlopeResult? GetLastLinearRegressionResult(this IEnumerable<AppQuote> quotes, int lookbackPeriods = 100)
        {
            if (quotes.IsNullOrEmpty() || quotes.Count() <= lookbackPeriods) return null;

            var result = quotes.GetLinearRegressionResults(lookbackPeriods);
            return result?.LastOrDefault();
        }
    }
}
