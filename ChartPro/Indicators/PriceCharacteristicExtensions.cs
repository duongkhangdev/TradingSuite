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
        // --- Atr --------------------------------------
        public static List<AtrResult>? GetAtrResults(this IEnumerable<AppQuote> quotes, int lookbackPeriods = 20)
        {
            if (quotes.IsNullOrEmpty() || quotes.Count() <= lookbackPeriods) return null;

            return quotes.GetAtr(lookbackPeriods)
                ?.Where(o => o.Atr.HasValue)
                ?.OrderBy(x => x.Date)
                ?.ToList();
        }

        public static AtrResult? GetLastAtrResult(this IEnumerable<AppQuote> quotes, int lookbackPeriods = 20)
        {
            if (quotes.IsNullOrEmpty() || quotes.Count() <= lookbackPeriods) return null;

            var result = quotes.GetAtrResults(lookbackPeriods);
            return result?.LastOrDefault();
        }

        // --- Bop --------------------------------------
        public static List<BopResult>? GetBopResults(this IEnumerable<AppQuote> quotes, int smoothPeriods = 14)
        {
            if (quotes.IsNullOrEmpty() || quotes.Count() <= smoothPeriods) return null;

            return quotes.GetBop(smoothPeriods)
                ?.Where(o => o.Bop.HasValue)
                ?.OrderBy(x => x.Date)
                ?.ToList();
        }

        public static BopResult? GetLastBopResult(this IEnumerable<AppQuote> quotes, int smoothPeriods = 14)
        {
            if (quotes.IsNullOrEmpty() || quotes.Count() <= smoothPeriods) return null;

            var result = quotes.GetBopResults(smoothPeriods);
            return result?.LastOrDefault();
        }

        // --- Chop --------------------------------------
        public static List<ChopResult>? GetChopResults(this IEnumerable<AppQuote> quotes, int lookbackPeriods = 14)
        {
            if (quotes.IsNullOrEmpty() || quotes.Count() <= lookbackPeriods) return null;

            return quotes.GetChop(lookbackPeriods)
                ?.Where(o => o.Chop.HasValue)
                ?.OrderBy(x => x.Date)
                ?.ToList();
        }

        public static ChopResult? GetLastChopResult(this IEnumerable<AppQuote> quotes, int lookbackPeriods = 14)
        {
            if (quotes.IsNullOrEmpty() || quotes.Count() <= lookbackPeriods) return null;

            var result = quotes.GetChopResults(lookbackPeriods);
            return result?.LastOrDefault();
        }

        // --- Roc --------------------------------------
        public static List<RocResult>? GetRocResults(this IEnumerable<AppQuote> quotes,
            int lookbackPeriods = 14,
            int? smaPeriods = null)
        {
            if (quotes.IsNullOrEmpty() || quotes.Count() <= lookbackPeriods) return null;

            return quotes.GetRoc(lookbackPeriods, smaPeriods)
                ?.Where(o => o.Roc.HasValue)
                ?.OrderBy(x => x.Date)
                ?.ToList();
        }

        public static RocResult? GetLastRocResult(this IEnumerable<AppQuote> quotes, int lookbackPeriods = 14, int? smaPeriods = null)
        {
            if (quotes.IsNullOrEmpty() || quotes.Count() <= lookbackPeriods) return null;

            var result = quotes.GetRocResults(lookbackPeriods, smaPeriods);
            return result?.LastOrDefault();
        }

        // --- Pmo --------------------------------------
        public static List<PmoResult>? GetPmoResults(this IEnumerable<AppQuote> quotes,
            int timePeriods = 35,
            int smoothPeriods = 20,
            int signalPeriods = 10)
        {
            if (quotes.IsNullOrEmpty()) return null;

            return quotes.GetPmo(timePeriods, smoothPeriods, signalPeriods)
                ?.Where(o => o.Pmo.HasValue)
                ?.OrderBy(x => x.Date)
                ?.ToList();
        }

        public static PmoResult? GetLastPmoResult(this IEnumerable<AppQuote> quotes,
            int timePeriods = 35,
            int smoothPeriods = 20,
            int signalPeriods = 10)
        {
            if (quotes.IsNullOrEmpty()) return null;

            var result = quotes.GetPmoResults(timePeriods, smoothPeriods, signalPeriods);
            return result?.LastOrDefault();
        }

        // --- Prs --------------------------------------
        public static List<PrsResult>? GetPrsResults(this IEnumerable<AppQuote> quotes,
            int? lookbackPeriods = null,
            int? smaPeriods = null)
        {
            if (quotes.IsNullOrEmpty()) return null;
            if (lookbackPeriods > 0)
            {
                if (quotes.Count() <= lookbackPeriods)
                    return null;
            }
            if (smaPeriods > 0)
            {
                if (quotes.Count() <= smaPeriods)
                    return null;
            }

            try
            {
                var result = quotes.GetPrs(quotes, lookbackPeriods, smaPeriods);
                return result?.ToList();
            }
            catch
            {
                return null;
            }            
        }

        public static PrsResult? GetLastPrsResult(this IEnumerable<AppQuote> quotes,
            int? lookbackPeriods = null,
            int? smaPeriods = null)
        {
            if (quotes.IsNullOrEmpty()) return null;
            if (lookbackPeriods > 0)
            {
                if (quotes.Count() <= lookbackPeriods)
                    return null;
            }
            if (smaPeriods > 0)
            {
                if (quotes.Count() <= smaPeriods)
                    return null;
            }

            var result = quotes.GetPrsResults(lookbackPeriods, smaPeriods);
            return result?.LastOrDefault();
        }

        // --- RocWb --------------------------------------
        public static List<RocWbResult>? GetRocWbResults(this IEnumerable<AppQuote> quotes,
            int lookbackPeriods = 12,
            int emaPeriods = 3,
            int stdDevPeriods = 12)
        {
            if (quotes.IsNullOrEmpty() || quotes.Count() <= lookbackPeriods) return null;

            return quotes.GetRocWb(lookbackPeriods, emaPeriods, stdDevPeriods)
                ?.Where(o => o.Roc.HasValue)
                ?.OrderBy(x => x.Date)
                ?.ToList();
        }

        public static RocWbResult? GetLastRocWbResult(this IEnumerable<AppQuote> quotes,
            int lookbackPeriods = 12,
            int emaPeriods = 3,
            int stdDevPeriods = 12)
        {
            if (quotes.IsNullOrEmpty() || quotes.Count() <= lookbackPeriods) return null;

            var result = quotes.GetRocWbResults(lookbackPeriods, emaPeriods, stdDevPeriods);
            return result?.LastOrDefault();
        }

        // --- Tsi --------------------------------------
        public static List<TsiResult>? GetTsiResults(this IEnumerable<AppQuote> quotes,
            int lookbackPeriods = 25,
            int smoothPeriods = 13,
            int signalPeriods = 7)
        {
            if (quotes.IsNullOrEmpty() || quotes.Count() <= lookbackPeriods) return null;

            return quotes.GetTsi(lookbackPeriods, smoothPeriods, signalPeriods)
                ?.Where(o => o.Tsi.HasValue)
                ?.OrderBy(x => x.Date)
                ?.ToList();
        }

        public static TsiResult? GetLastTsiResult(this IEnumerable<AppQuote> quotes,
            int lookbackPeriods = 25,
            int smoothPeriods = 13,
            int signalPeriods = 7)
        {
            if (quotes.IsNullOrEmpty() || quotes.Count() <= lookbackPeriods) return null;

            var result = quotes.GetTsiResults(lookbackPeriods, smoothPeriods, signalPeriods);
            return result?.LastOrDefault();
        }

        // --- UlcerIndex --------------------------------------
        public static List<UlcerIndexResult>? GetUlcerIndexResults(this IEnumerable<AppQuote> quotes, 
            int lookbackPeriods = 14)
        {
            if (quotes.IsNullOrEmpty() || quotes.Count() <= lookbackPeriods) return null;

            return quotes.GetUlcerIndex(lookbackPeriods)
                ?.Where(o => o.UI.HasValue)
                ?.OrderBy(x => x.Date)
                ?.ToList();
        }

        public static UlcerIndexResult? GetLastUlcerIndexResult(this IEnumerable<AppQuote> quotes, int lookbackPeriods = 14)
        {
            if (quotes.IsNullOrEmpty() || quotes.Count() <= lookbackPeriods) return null;

            var result = quotes.GetUlcerIndexResults(lookbackPeriods);
            return result?.LastOrDefault();
        }
    }
}
