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
        // --- Alma --------------------------------------
        public static List<AlmaResult>? GetAlmaResults(this IEnumerable<AppQuote> quotes,
            int lookbackPeriods = 9,
            double offset = 0.85,
            double sigma = 6)
        {
            if (quotes.IsNullOrEmpty() || quotes.Count() <= lookbackPeriods) return null;

            return quotes.GetAlma(lookbackPeriods, offset, sigma)
                ?.Where(o => o.Alma.HasValue)
                ?.OrderBy(x => x.Date)
                ?.ToList();
        }

        public static AlmaResult? GetLastAlmaResult(this IEnumerable<AppQuote> quotes,
            int lookbackPeriods = 9,
            double offset = 0.85,
            double sigma = 6)
        {
            if (quotes.IsNullOrEmpty() || quotes.Count() <= lookbackPeriods) return null;

            var result = quotes.GetAlmaResults(lookbackPeriods, offset, sigma);
            return result?.LastOrDefault();
        }

        // --- Dema --------------------------------------
        public static List<DemaResult>? GetDemaResults(this IEnumerable<AppQuote> quotes, int lookbackPeriods = 14)
        {
            if (quotes.IsNullOrEmpty() || quotes.Count() <= lookbackPeriods) return null;

            return quotes.GetDema(lookbackPeriods)
                ?.Where(o => o.Dema.HasValue)
                ?.OrderBy(x => x.Date)
                ?.ToList();
        }

        public static DemaResult? GetLastDemaResult(this IEnumerable<AppQuote> quotes, int lookbackPeriods = 14)
        {
            if (quotes.IsNullOrEmpty() || quotes.Count() <= lookbackPeriods) return null;

            var result = quotes.GetDemaResults(lookbackPeriods);
            return result?.LastOrDefault();
        }

        // --- Epma --------------------------------------
        public static List<EpmaResult>? GetEpmaResults(this IEnumerable<AppQuote> quotes, int lookbackPeriods = 15)
        {
            if (quotes.IsNullOrEmpty() || quotes.Count() <= lookbackPeriods) return null;

            var result = quotes.GetEpma(lookbackPeriods); // review
            return result.ToList();
        }

        public static EpmaResult? GetLastEpmaResult(this IEnumerable<AppQuote> quotes, int lookbackPeriods = 15)
        {
            if (quotes.IsNullOrEmpty() || quotes.Count() <= lookbackPeriods) return null;

            var result = quotes.GetEpmaResults(lookbackPeriods);
            return result?.LastOrDefault();
        }

        // --- Ema --------------------------------------
        public static List<EmaResult>? GetEmaResults(this IEnumerable<AppQuote> quotes, int lookbackPeriods)
        {
            if (quotes.IsNullOrEmpty() || quotes.Count() <= lookbackPeriods) return null;

            return quotes.GetEma(lookbackPeriods)
                ?.Where(o => o.Ema.HasValue)
                ?.OrderBy(x => x.Date)
                ?.ToList();
        }

        public static EmaResult? GetLastEmaResult(this IEnumerable<AppQuote> quotes, int lookbackPeriods)
        {
            if (quotes.IsNullOrEmpty() || quotes.Count() <= lookbackPeriods) return null;

            var result = quotes.GetEmaResults(lookbackPeriods);
            return result?.LastOrDefault();
        }

        // --- Htl --------------------------------------
        public static List<HtlResult>? GetHtlResults(this IEnumerable<AppQuote> quotes)
        {
            if (quotes.IsNullOrEmpty()) return null;

            return quotes.GetHtTrendline()
                ?.Where(o => o.Trendline.HasValue && o.SmoothPrice.HasValue)
                ?.OrderBy(x => x.Date)
                ?.ToList();
        }

        public static HtlResult? GetLastHtlResult(this IEnumerable<AppQuote> quotes)
        {
            if (quotes.IsNullOrEmpty()) return null;

            var result = quotes.GetHtlResults();
            return result?.LastOrDefault();
        }

        // --- Hma --------------------------------------
        public static List<HmaResult>? GetHmaResults(this IEnumerable<AppQuote> quotes, int lookbackPeriods)
        {
            if (quotes.IsNullOrEmpty() || quotes.Count() <= lookbackPeriods) return null;

            return quotes.GetHma(lookbackPeriods)
                ?.Where(o => o.Hma.HasValue)
                ?.OrderBy(x => x.Date)
                ?.ToList();
        }

        public static HmaResult? GetLastHmaResult(this IEnumerable<AppQuote> quotes, int lookbackPeriods)
        {
            if (quotes.IsNullOrEmpty() || quotes.Count() <= lookbackPeriods) return null;

            var result = quotes.GetHmaResults(lookbackPeriods);
            return result?.LastOrDefault();
        }

        // --- Kama --------------------------------------
        public static List<KamaResult>? GetKamaResults(this IEnumerable<AppQuote> quotes,
            int erPeriods = 10,
            int fastPeriods = 2,
            int slowPeriods = 30)
        {
            if (quotes.IsNullOrEmpty() || quotes.Count() <= slowPeriods) return null;

            return quotes.GetKama(erPeriods, fastPeriods, slowPeriods)
                ?.Where(o => o.Kama.HasValue)
                ?.OrderBy(x => x.Date)
                ?.ToList();
        }

        public static KamaResult? GetLastKamaResult(this IEnumerable<AppQuote> quotes,
            int erPeriods = 10,
            int fastPeriods = 2,
            int slowPeriods = 30)
        {
            if (quotes.IsNullOrEmpty() || quotes.Count() <= slowPeriods) return null;

            var result = quotes.GetKamaResults(erPeriods, fastPeriods, slowPeriods);
            return result?.LastOrDefault();
        }

        // --- Mama --------------------------------------
        public static List<MamaResult>? GetMamaResults(this IEnumerable<AppQuote> quotes,
            double fastLimit = 0.5,
            double slowLimit = 0.05)
        {
            if (quotes.IsNullOrEmpty()) return null;

            return quotes.GetMama(fastLimit, slowLimit)
                ?.Where(o => o.Mama.HasValue)
                ?.OrderBy(x => x.Date)
                ?.ToList();
        }

        public static MamaResult? GetLastMamaResult(this IEnumerable<AppQuote> quotes,
            double fastLimit = 0.5,
            double slowLimit = 0.05)
        {
            if (quotes.IsNullOrEmpty()) return null;

            var result = quotes.GetMamaResults(fastLimit, slowLimit);
            return result?.LastOrDefault();
        }

        // --- Dynamic --------------------------------------
        public static List<DynamicResult>? GetDynamicResults(this IEnumerable<AppQuote> quotes,
            int lookbackPeriods = 20,
            double kFactor = 0.6)
        {
            if (quotes.IsNullOrEmpty() || quotes.Count() <= lookbackPeriods) return null;

            return quotes.GetDynamic(lookbackPeriods, kFactor)
                ?.Where(o => o.Dynamic.HasValue)
                ?.OrderBy(x => x.Date)
                ?.ToList();
        }

        public static DynamicResult? GetLastDynamicResult(this IEnumerable<AppQuote> quotes,
            int lookbackPeriods = 20,
            double kFactor = 0.6)
        {
            if (quotes.IsNullOrEmpty() || quotes.Count() <= lookbackPeriods) return null;

            var result = quotes.GetDynamicResults(lookbackPeriods, kFactor);
            return result?.LastOrDefault();
        }

        // --- Smma --------------------------------------
        public static List<SmmaResult>? GetSmmaResults(this IEnumerable<AppQuote> quotes, int lookbackPeriods)
        {
            if (quotes.IsNullOrEmpty() || quotes.Count() <= lookbackPeriods) return null;

            return quotes.GetSmma(lookbackPeriods)
                ?.Where(o => o.Smma.HasValue)
                ?.OrderBy(x => x.Date)
                ?.ToList();
        }

        public static SmmaResult? GetLastSmmaResult(this IEnumerable<AppQuote> quotes, int lookbackPeriods)
        {
            if (quotes.IsNullOrEmpty() || quotes.Count() <= lookbackPeriods) return null;

            var result = quotes.GetSmmaResults(lookbackPeriods);
            return result?.LastOrDefault();
        }

        // --- Sma --------------------------------------
        public static List<SmaResult>? GetSmaResults(this IEnumerable<AppQuote> quotes, int lookbackPeriods)
        {
            if (quotes.IsNullOrEmpty() || quotes.Count() <= lookbackPeriods) return null;

            return quotes.GetSma(lookbackPeriods)
                ?.Where(o => o.Sma.HasValue)
                ?.OrderBy(x => x.Date)
                ?.ToList();
        }

        public static SmaResult? GetLastSmaResult(this IEnumerable<AppQuote> quotes, int lookbackPeriods)
        {
            if (quotes.IsNullOrEmpty() || quotes.Count() <= lookbackPeriods) return null;

            var result = quotes.GetSmaResults(lookbackPeriods);
            return result?.LastOrDefault();
        }

        // --- T3 --------------------------------------
        public static List<T3Result>? GetT3Results(this IEnumerable<AppQuote> quotes,
            int lookbackPeriods,
            double volumeFactor = 0.7)
        {
            if (quotes.IsNullOrEmpty() || quotes.Count() <= lookbackPeriods) return null;

            return quotes.GetT3(lookbackPeriods, volumeFactor)
                ?.Where(o => o.T3.HasValue)
                ?.OrderBy(x => x.Date)
                ?.ToList();
        }

        public static T3Result? GetLastT3Result(this IEnumerable<AppQuote> quotes,
            int lookbackPeriods,
            double volumeFactor = 0.7)
        {
            if (quotes.IsNullOrEmpty() || quotes.Count() <= lookbackPeriods) return null;

            var result = quotes.GetT3Results(lookbackPeriods, volumeFactor);
            return result?.LastOrDefault();
        }

        // --- Tema --------------------------------------
        public static List<TemaResult>? GetTemaResults(this IEnumerable<AppQuote> quotes, int lookbackPeriods)
        {
            if (quotes.IsNullOrEmpty() || quotes.Count() <= lookbackPeriods) return null;

            return quotes.GetTema(lookbackPeriods)
                ?.Where(o => o.Tema.HasValue)
                ?.OrderBy(x => x.Date)
                ?.ToList();
        }

        public static TemaResult? GetLastTemaResult(this IEnumerable<AppQuote> quotes, int lookbackPeriods)
        {
            if (quotes.IsNullOrEmpty() || quotes.Count() <= lookbackPeriods) return null;

            var result = quotes.GetTemaResults(lookbackPeriods);
            return result?.LastOrDefault();
        }

        // --- Vwap --------------------------------------
        public static List<VwapResult>? GetVwapResults(this IEnumerable<AppQuote> quotes, DateTime? startDate = null)
        {
            if (quotes.IsNullOrEmpty()) return null;

            return quotes.GetVwap(startDate)
                ?.Where(o => o.Vwap.HasValue)
                ?.OrderBy(x => x.Date)
                ?.ToList();
        }

        public static VwapResult? GetLastVwapResult(this IEnumerable<AppQuote> quotes, DateTime? startDate = null)
        {
            if (quotes.IsNullOrEmpty()) return null;

            var result = quotes.GetVwapResults(startDate);
            return result?.LastOrDefault();
        }

        // --- Vwma --------------------------------------
        public static List<VwmaResult>? GetVwmaResults(this IEnumerable<AppQuote> quotes, int lookbackPeriods)
        {
            if (quotes.IsNullOrEmpty() || quotes.Count() <= lookbackPeriods) return null;

            return quotes.GetVwma(lookbackPeriods)
                ?.Where(o => o.Vwma.HasValue)
                ?.OrderBy(x => x.Date)
                ?.ToList();
        }

        public static VwmaResult? GetLastVwmaResult(this IEnumerable<AppQuote> quotes, int lookbackPeriods)
        {
            if (quotes.IsNullOrEmpty() || quotes.Count() <= lookbackPeriods) return null;

            var result = quotes.GetVwmaResults(lookbackPeriods);
            return result?.LastOrDefault();
        }

        // --- Wma --------------------------------------
        public static List<WmaResult>? GetWmaResults(this IEnumerable<AppQuote> quotes, int lookbackPeriods)
        {
            if (quotes.IsNullOrEmpty() || quotes.Count() <= lookbackPeriods) return null;

            return quotes.GetWma(lookbackPeriods)
                ?.Where(o => o.Wma.HasValue)
                ?.OrderBy(x => x.Date)
                ?.ToList();
        }

        public static WmaResult? GetLastWmaResult(this IEnumerable<AppQuote> quotes, int lookbackPeriods)
        {
            if (quotes.IsNullOrEmpty() || quotes.Count() <= lookbackPeriods) return null;

            var result = quotes.GetWmaResults(lookbackPeriods);
            return result?.LastOrDefault();
        }
    }
}
