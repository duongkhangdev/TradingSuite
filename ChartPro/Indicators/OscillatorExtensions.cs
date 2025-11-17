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
        // --- Awesome --------------------------
        public static List<AwesomeResult>? GetAwesomeResults(this IEnumerable<AppQuote> quotes,
            int fastPeriods = 5,
            int slowPeriods = 34)
        {
            if (quotes.IsNullOrEmpty()) return null;

            return quotes.GetAwesome(fastPeriods, slowPeriods)
                ?.Where(o => o.Oscillator.HasValue)
                ?.OrderBy(x => x.Date)
                ?.ToList();
        }

        public static AwesomeResult? GetLastAwesomeResult(this IEnumerable<AppQuote> quotes,
            int fastPeriods = 5,
            int slowPeriods = 34)
        {
            if (quotes.IsNullOrEmpty()) return null;

            var result = quotes.GetAwesomeResults(fastPeriods, slowPeriods);
            return result?.LastOrDefault();
        }

        // --- Cmo --------------------------
        public static List<CmoResult>? GetCmoResults(this IEnumerable<AppQuote> quotes, int lookbackPeriods = 14)
        {
            if (quotes.IsNullOrEmpty() || quotes.Count() <= lookbackPeriods) return null;

            var result = quotes.GetCmo(lookbackPeriods); // review
            return result.ToList();
        }

        public static CmoResult? GetLastCmoResult(this IEnumerable<AppQuote> quotes, int lookbackPeriods = 14)
        {
            if (quotes.IsNullOrEmpty() || quotes.Count() <= lookbackPeriods) return null;

            var result =  quotes.GetCmoResults(lookbackPeriods);
            return result?.LastOrDefault();
        }

        // --- Cci --------------------------
        public static List<CciResult>? GetCciResults(this IEnumerable<AppQuote> quotes, int lookbackPeriods = 20)
        {
            if (quotes.IsNullOrEmpty() || quotes.Count() <= lookbackPeriods) return null;

            return quotes.GetCci(lookbackPeriods)
                ?.Where(o => o.Cci.HasValue)
                ?.OrderBy(x => x.Date)
                ?.ToList();
        }

        public static CciResult? GetLastCciResult(this IEnumerable<AppQuote> quotes, int lookbackPeriods = 20)
        {
            if (quotes.IsNullOrEmpty() || quotes.Count() <= lookbackPeriods) return null;

            var result =  quotes.GetCciResults(lookbackPeriods);
            return result?.LastOrDefault();
        }

        // --- ConnorsRsi --------------------------
        public static List<ConnorsRsiResult>? GetConnorsRsiResults(this IEnumerable<AppQuote> quotes,
            int rsiPeriods = 3,
            int streakPeriods = 2,
            int rankPeriods = 100)
        {
            if (quotes.IsNullOrEmpty()) return null;

            return quotes.GetConnorsRsi(rsiPeriods, streakPeriods, rankPeriods)
                ?.Where(o => o.ConnorsRsi.HasValue)
                ?.OrderBy(x => x.Date)
                ?.ToList();
        }

        public static ConnorsRsiResult? GetLastConnorsRsiResult(this IEnumerable<AppQuote> quotes,
            int rsiPeriods = 3,
            int streakPeriods = 2,
            int rankPeriods = 100)
        {
            if (quotes.IsNullOrEmpty()) return null;

            var result = quotes.GetConnorsRsiResults(rsiPeriods, streakPeriods, rankPeriods);
            return result?.LastOrDefault();
        }

        // --- Dpo --------------------------
        public static List<DpoResult>? GetDpoResults(this IEnumerable<AppQuote> quotes, int lookbackPeriods = 20)
        {
            if (quotes.IsNullOrEmpty() || quotes.Count() <= lookbackPeriods) return null;

            return quotes.GetDpo(lookbackPeriods)
                ?.Where(o => o.Dpo.HasValue)
                ?.OrderBy(x => x.Date)
                ?.ToList();
        }

        public static DpoResult? GetLastDpoResult(this IEnumerable<AppQuote> quotes, int lookbackPeriods = 20)
        {
            if (quotes.IsNullOrEmpty() || quotes.Count() <= lookbackPeriods) return null;

            var result = quotes.GetDpoResults(lookbackPeriods);
            return result?.LastOrDefault();
        }

        // --- Stoch --------------------------
        public static List<StochResult>? GetStochResults(this IEnumerable<AppQuote> quotes,
            int lookbackPeriods = 14,
            int signalPeriods = 3,
            int smoothPeriods = 3)
        {
            if (quotes.IsNullOrEmpty() || quotes.Count() <= lookbackPeriods) return null;

            return quotes.GetStoch(lookbackPeriods, signalPeriods, smoothPeriods)
                ?.Where(o => o.Oscillator.HasValue)
                ?.OrderBy(x => x.Date)
                ?.ToList();
        }

        public static StochResult? GetLastStochResult(this IEnumerable<AppQuote> quotes,
            int lookbackPeriods = 14,
            int signalPeriods = 3,
            int smoothPeriods = 3)
        {
            if (quotes.IsNullOrEmpty() || quotes.Count() <= lookbackPeriods) return null;

            var result = quotes.GetStochResults(lookbackPeriods, signalPeriods, smoothPeriods);
            return result?.LastOrDefault();
        }

        // --- Rsi --------------------------
        public static List<RsiResult>? GetRsiResults(this IEnumerable<AppQuote> quotes, int lookbackPeriods = 14)
        {
            if (quotes.IsNullOrEmpty() || quotes.Count() <= lookbackPeriods) return null;

            return quotes.GetRsi(lookbackPeriods)
                ?.Where(o => o.Rsi.HasValue)
                ?.OrderBy(x => x.Date)
                ?.ToList();
        }

        public static RsiResult? GetLastRsiResult(this IEnumerable<AppQuote> quotes, int lookbackPeriods = 14)
        {
            if (quotes.IsNullOrEmpty() || quotes.Count() <= lookbackPeriods) return null;

            var result = quotes.GetRsiResults(lookbackPeriods);
            return result?.LastOrDefault();
        }

        // --- Stc --------------------------
        public static List<StcResult>? GetStcResults(this IEnumerable<AppQuote> quotes,
            int cyclePeriods = 10,
            int fastPeriods = 23,
            int slowPeriods = 50)
        {
            if (quotes.IsNullOrEmpty()) return null;

            var result = quotes.GetStc(cyclePeriods, fastPeriods, slowPeriods);
            return result.ToList();
        }

        public static StcResult? GetLastStcResult(this IEnumerable<AppQuote> quotes,
            int cyclePeriods = 10,
            int fastPeriods = 23,
            int slowPeriods = 50)
        {
            if (quotes.IsNullOrEmpty()) return null;

            var result = quotes.GetStcResults(cyclePeriods, fastPeriods, slowPeriods);
            return result?.LastOrDefault();
        }

        // --- Smi --------------------------
        public static List<SmiResult>? GetSmiResults(this IEnumerable<AppQuote> quotes,
            int lookbackPeriods = 10,
            int firstSmoothPeriods = 3,
            int secondSmoothPeriods = 3,
            int signalPeriods = 3)
        {
            if (quotes.IsNullOrEmpty() || quotes.Count() <= lookbackPeriods) return null;

            return quotes.GetSmi(lookbackPeriods, firstSmoothPeriods, secondSmoothPeriods, signalPeriods)
                ?.Where(o => o.Smi.HasValue)
                ?.OrderBy(x => x.Date)
                ?.ToList();
        }

        public static SmiResult? GetLastSmiResult(this IEnumerable<AppQuote> quotes,
            int lookbackPeriods = 10,
            int firstSmoothPeriods = 3,
            int secondSmoothPeriods = 3,
            int signalPeriods = 3)
        {
            if (quotes.IsNullOrEmpty() || quotes.Count() <= lookbackPeriods) return null;

            var result = quotes.GetSmiResults(lookbackPeriods, firstSmoothPeriods, secondSmoothPeriods, signalPeriods);
            return result?.LastOrDefault();
        }

        // --- StochRsi --------------------------
        public static List<StochRsiResult>? GetStochRsiResults(this IEnumerable<AppQuote> quotes,
            int rsiPeriods = 14,
            int stochPeriods = 14,
            int signalPeriods = 3,
            int smoothPeriods = 1)
        {
            if (quotes.IsNullOrEmpty()) return null;

            return quotes.GetStochRsi(rsiPeriods, stochPeriods, signalPeriods, smoothPeriods)
                ?.Where(o => o.StochRsi.HasValue)
                ?.OrderBy(x => x.Date)
                ?.ToList();
        }

        public static StochRsiResult? GetLastStochRsiResult(this IEnumerable<AppQuote> quotes,
            int rsiPeriods = 14,
            int stochPeriods = 14,
            int signalPeriods = 3,
            int smoothPeriods = 1)
        {
            if (quotes.IsNullOrEmpty()) return null;

            var result = quotes.GetStochRsiResults(rsiPeriods, stochPeriods, signalPeriods, smoothPeriods);
            return result?.LastOrDefault();
        }

        // --- Trix --------------------------
        public static List<TrixResult>? GetTrixResults(this IEnumerable<AppQuote> quotes,
            int lookbackPeriods = 15,
            int? signalPeriods = 9)
        {
            if (quotes.IsNullOrEmpty() || quotes.Count() <= lookbackPeriods) return null;

            return quotes.GetTrix(lookbackPeriods, signalPeriods)
                ?.Where(o => o.Trix.HasValue)
                ?.OrderBy(x => x.Date)
                ?.ToList();
        }

        public static TrixResult? GetLastTrixResult(this IEnumerable<AppQuote> quotes,
            int lookbackPeriods = 15,
            int? signalPeriods = 9)
        {
            if (quotes.IsNullOrEmpty() || quotes.Count() <= lookbackPeriods) return null;

            var result = quotes.GetTrixResults(lookbackPeriods, signalPeriods);
            return result?.LastOrDefault();
        }

        // --- Ultimate --------------------------
        public static List<UltimateResult>? GetUltimateResults(this IEnumerable<AppQuote> quotes,
            int shortPeriods = 7,
            int middlePeriods = 14,
            int longPeriods = 28)
        {
            if (quotes.IsNullOrEmpty()) return null;

            return quotes.GetUltimate(shortPeriods, middlePeriods, longPeriods)
                ?.Where(o => o.Ultimate.HasValue)
                ?.OrderBy(x => x.Date)
                ?.ToList();
        }

        public static UltimateResult? GetLastUltimateResult(this IEnumerable<AppQuote> quotes,
            int shortPeriods = 7,
            int middlePeriods = 14,
            int longPeriods = 28)
        {
            if (quotes.IsNullOrEmpty()) return null;

            var result = quotes.GetUltimateResults(shortPeriods, middlePeriods, longPeriods);
            return result?.LastOrDefault();
        }

        // --- Williams --------------------------
        public static List<WilliamsResult>? GetWilliamsResults(this IEnumerable<AppQuote> quotes, int lookbackPeriods = 14)
        {
            if (quotes.IsNullOrEmpty() || quotes.Count() <= lookbackPeriods) return null;

            return quotes.GetWilliamsR(lookbackPeriods)
                ?.Where(o => o.WilliamsR.HasValue)
                ?.OrderBy(x => x.Date)
                ?.ToList();
        }

        public static WilliamsResult? GetLastWilliamsResult(this IEnumerable<AppQuote> quotes, int lookbackPeriods = 14)
        {
            if (quotes.IsNullOrEmpty() || quotes.Count() <= lookbackPeriods) return null;

            var result = quotes.GetWilliamsResults(lookbackPeriods);
            return result?.LastOrDefault();
        }
    }
}
