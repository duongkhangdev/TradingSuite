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
        // --- Adl --------------------------------------
        public static List<AdlResult>? GetAdlResults(this IEnumerable<AppQuote> quotes,
            int? smaPeriods = 3)
        {
            if (quotes.IsNullOrEmpty()) return null;

            var result = quotes.GetAdl(smaPeriods); // review
            return result.ToList();
        }

        public static AdlResult? GetLastAdlResult(this IEnumerable<AppQuote> quotes, int? smaPeriods = 3)
        {
            if (quotes.IsNullOrEmpty()) return null;

            var result = quotes.GetAdlResults(smaPeriods);
            return result?.LastOrDefault();
        }

        // --- Cmf --------------------------------------
        public static List<CmfResult>? GetCmfResults(this IEnumerable<AppQuote> quotes,
            int lookbackPeriods = 20)
        {
            if (quotes.IsNullOrEmpty() || quotes.Count() <= lookbackPeriods) return null;

            return quotes.GetCmf(lookbackPeriods)
                ?.Where(o => o.Cmf.HasValue)
                ?.OrderBy(x => x.Date)
                ?.ToList();
        }

        public static CmfResult? GetLastCmfResult(this IEnumerable<AppQuote> quotes, int lookbackPeriods = 20)
        {
            if (quotes.IsNullOrEmpty() || quotes.Count() <= lookbackPeriods) return null;

            var result = quotes.GetCmfResults(lookbackPeriods);
            return result?.LastOrDefault();
        }

        // --- ChaikinOsc --------------------------------------
        public static List<ChaikinOscResult>? GetChaikinOscResults(this IEnumerable<AppQuote> quotes, 
            int fastPeriods = 3, 
            int slowPeriods = 10)
        {
            if (quotes.IsNullOrEmpty()) return null;

            return quotes.GetChaikinOsc(fastPeriods, slowPeriods)
                ?.Where(o => o.Oscillator.HasValue)
                ?.OrderBy(x => x.Date)
                ?.ToList();
        }

        public static ChaikinOscResult? GetLastChaikinOscResult(this IEnumerable<AppQuote> quotes, 
            int fastPeriods = 3, 
            int slowPeriods = 10)
        {
            if (quotes.IsNullOrEmpty()) return null;

            var result = quotes.GetChaikinOscResults(fastPeriods, slowPeriods);
            return result?.LastOrDefault();
        }

        // --- ForceIndex --------------------------------------
        public static List<ForceIndexResult>? GetForceIndexResults(this IEnumerable<AppQuote> quotes, 
            int lookbackPeriods = 13)
        {
            if (quotes.IsNullOrEmpty() || quotes.Count() <= lookbackPeriods) return null;

            return quotes.GetForceIndex(lookbackPeriods)
                ?.Where(o => o.ForceIndex.HasValue)
                ?.OrderBy(x => x.Date)
                ?.ToList();
        }

        public static ForceIndexResult? GetLastForceIndexResult(this IEnumerable<AppQuote> quotes, 
            int lookbackPeriods = 13)
        {
            if (quotes.IsNullOrEmpty() || quotes.Count() <= lookbackPeriods) return null;

            var result = quotes.GetForceIndexResults(lookbackPeriods);
            return result?.LastOrDefault();
        }

        // --- Kvo --------------------------------------
        public static List<KvoResult>? GetKvoResults(this IEnumerable<AppQuote> quotes,
            int fastPeriods = 34,
            int slowPeriods = 55,
            int signalPeriods = 13)
        {
            if (quotes.IsNullOrEmpty()) return null;

            return quotes.GetKvo(fastPeriods, slowPeriods, signalPeriods)
                ?.Where(o => o.Oscillator.HasValue)
                ?.OrderBy(x => x.Date)
                ?.ToList();
        }

        public static KvoResult? GetLastKvoResult(this IEnumerable<AppQuote> quotes,
            int fastPeriods = 34,
            int slowPeriods = 55,
            int signalPeriods = 13)
        {
            if (quotes.IsNullOrEmpty()) return null;

            var result = quotes.GetKvoResults(fastPeriods, slowPeriods, signalPeriods);
            return result?.LastOrDefault();
        }

        // --- Mfi --------------------------------------
        public static List<MfiResult>? GetMfiResults(this IEnumerable<AppQuote> quotes, 
            int lookbackPeriods = 14)
        {
            if (quotes.IsNullOrEmpty(lookbackPeriods)) return null;

            return quotes.GetMfi(lookbackPeriods)
                ?.Where(o => o.Mfi.HasValue)
                ?.OrderBy(x => x.Date)
                ?.ToList();
        }

        public static MfiResult? GetLastMfiResult(this IEnumerable<AppQuote> quotes, int lookbackPeriods = 14)
        {
            if (quotes.IsNullOrEmpty() || quotes.Count() <= lookbackPeriods) return null;

            var result = quotes.GetMfiResults(lookbackPeriods);
            return result?.LastOrDefault();
        }

        // --- Obv --------------------------------------
        public static List<ObvResult>? GetObvResults(this IEnumerable<AppQuote> quotes, 
            int smaPeriods = 14)
        {
            if (quotes.IsNullOrEmpty()) return null;

            return quotes.GetObv(smaPeriods)
                ?.Where(o => o.ObvSma.HasValue)
                ?.OrderBy(x => x.Date)
                ?.ToList();
        }

        public static ObvResult? GetLastObvResult(this IEnumerable<AppQuote> quotes, 
            int smaPeriods = 14)
        {
            if (quotes.IsNullOrEmpty()) return null;

            var result = quotes.GetObvResults(smaPeriods);
            return result?.LastOrDefault();
        }

        // --- Pvo --------------------------------------
        public static List<PvoResult>? GetPvoResults(this IEnumerable<AppQuote> quotes,
            int fastPeriods = 12,
            int slowPeriods = 26,
            int signalPeriods = 9)
        {
            if (quotes.IsNullOrEmpty()) return null;

            return quotes.GetPvo(fastPeriods, slowPeriods, signalPeriods)
                ?.Where(o => o.Pvo.HasValue)
                ?.OrderBy(x => x.Date)
                ?.ToList();
        }

        public static PvoResult? GetLastPvoResult(this IEnumerable<AppQuote> quotes,
            int fastPeriods = 12,
            int slowPeriods = 26,
            int signalPeriods = 9)
        {
            if (quotes.IsNullOrEmpty()) return null;

            var result = quotes.GetPvoResults(fastPeriods, slowPeriods, signalPeriods);
            return result?.LastOrDefault();
        }
    }
}
