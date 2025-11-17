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
        // --- AtrStop --------------------------------------
        public static List<AtrStopResult>? GetAtrStopResults(this IEnumerable<AppQuote> quotes,
            int lookbackPeriods = 21,
            double multiplier = 3,
            EndType endType = EndType.Close)
        {
            if (quotes.IsNullOrEmpty() || quotes.Count() <= lookbackPeriods) return null;

            return quotes.GetAtrStop(lookbackPeriods, multiplier, endType)
                ?.Where(o => o.AtrStop.HasValue)
                ?.OrderBy(x => x.Date)
                ?.ToList();
        }

        public static AtrStopResult? GetLastAtrStopResult(this IEnumerable<AppQuote> quotes,
            int lookbackPeriods = 21,
            double multiplier = 3,
            EndType endType = EndType.Close)
        {
            if (quotes.IsNullOrEmpty() || quotes.Count() <= lookbackPeriods) return null;

            var result = quotes.GetAtrStopResults(lookbackPeriods, multiplier, endType);
            return result?.LastOrDefault();
        }

        // --- Aroon --------------------------------------
        public static List<AroonResult>? GetAroonResults(this IEnumerable<AppQuote> quotes,
            int lookbackPeriods = 25)
        {
            if (quotes.IsNullOrEmpty() || quotes.Count() <= lookbackPeriods) return null;

            return quotes.GetAroon(lookbackPeriods)
                ?.Where(o => o.Oscillator.HasValue)
                ?.OrderBy(x => x.Date)
                ?.ToList();
        }

        public static AroonResult? GetLastAroonResult(this IEnumerable<AppQuote> quotes,
            int lookbackPeriods = 25)
        {
            if (quotes.IsNullOrEmpty() || quotes.Count() <= lookbackPeriods) return null;

            var result = quotes.GetAroonResults(lookbackPeriods);
            return result?.LastOrDefault();
        }

        // --- Adx --------------------------------------
        public static List<AdxResult>? GetAdxResults(this IEnumerable<AppQuote> quotes,
            int lookbackPeriods = 14)
        {
            if (quotes.IsNullOrEmpty() || quotes.Count() <= lookbackPeriods) return null;

            return quotes.GetAdx(lookbackPeriods)
                ?.Where(o => o.Adx.HasValue)
                ?.OrderBy(x => x.Date)
                ?.ToList();
        }

        public static AdxResult? GetLastAdxResult(this IEnumerable<AppQuote> quotes,
            int lookbackPeriods = 14)
        {
            if (quotes.IsNullOrEmpty() || quotes.Count() <= lookbackPeriods) return null;

            var result = quotes.GetAdxResults(lookbackPeriods);
            return result?.LastOrDefault();
        }

        // --- ElderRay --------------------------------------
        public static List<ElderRayResult>? GetElderRayResults(this IEnumerable<AppQuote> quotes,
            int lookbackPeriods = 13)
        {
            if (quotes.IsNullOrEmpty() || quotes.Count() <= lookbackPeriods) return null;

            return quotes.GetElderRay(lookbackPeriods)
                ?.Where(o => o.Ema.HasValue)
                ?.OrderBy(x => x.Date)
                ?.ToList();
        }

        public static ElderRayResult? GetLastElderRayResult(this IEnumerable<AppQuote> quotes,
            int lookbackPeriods = 13)
        {
            if (quotes.IsNullOrEmpty() || quotes.Count() <= lookbackPeriods) return null;

            var result = quotes.GetElderRayResults(lookbackPeriods);
            return result?.LastOrDefault();
        }

        // --- Gator --------------------------------------
        public static List<GatorResult>? GetGatorResults(this IEnumerable<AppQuote> quotes)
        {
            if (quotes.IsNullOrEmpty()) return null;

            return quotes.GetGator()
                ?.Where(o => o.Upper.HasValue || o.Lower.HasValue)
                ?.OrderBy(x => x.Date)
                ?.ToList();
        }

        public static GatorResult? GetLastGatorResult(this IEnumerable<AppQuote> quotes)
        {
            if (quotes.IsNullOrEmpty()) return null;

            var result = quotes.GetGatorResults();
            return result?.LastOrDefault();
        }

        // --- Hurst --------------------------------------
        public static List<HurstResult>? GetHurstResults(this IEnumerable<AppQuote> quotes, 
            int lookbackPeriods = 100)
        {
            if (quotes.IsNullOrEmpty() || quotes.Count() <= lookbackPeriods) return null;

            return quotes.GetHurst(lookbackPeriods)
                ?.Where(o => o.HurstExponent.HasValue)
                ?.OrderBy(x => x.Date)
                ?.ToList();
        }
        
        public static HurstResult? GetLastHurstResult(this IEnumerable<AppQuote> quotes, 
            int lookbackPeriods = 100)
        {
            if (quotes.IsNullOrEmpty() || quotes.Count() <= lookbackPeriods) return null;

            var result = quotes.GetHurstResults(lookbackPeriods);
            return result?.LastOrDefault();
        }

        // --- Ichimoku --------------------------------------
        public static List<IchimokuResult>? GetIchimokuResults(this IEnumerable<AppQuote> quotes,
            int tenkanPeriods = 9,
            int kijunPeriods = 26,
            int senkouBPeriods = 52)
        {
            if (quotes.IsNullOrEmpty()) return null;

            return quotes.GetIchimoku(tenkanPeriods, kijunPeriods, senkouBPeriods)
                ?.Where(o => o.TenkanSen.HasValue && o.KijunSen.HasValue)
                ?.OrderBy(x => x.Date)
                ?.ToList();
        }

        public static IchimokuResult? GetLastIchimokuResult(this IEnumerable<AppQuote> quotes,
            int tenkanPeriods = 9,
            int kijunPeriods = 26,
            int senkouBPeriods = 52)
        {
            if (quotes.IsNullOrEmpty()) return null;

            var result = quotes.GetIchimokuResults(tenkanPeriods, kijunPeriods, senkouBPeriods);
            return result?.LastOrDefault();
        }

        // --- Macd --------------------------------------
        public static List<Skender.Stock.Indicators.MacdResult>? GetMacdResults(this IEnumerable<AppQuote> quotes,
            int fastPeriods = 12,
            int slowPeriods = 26,
            int signalPeriods = 9)
        {
            if (quotes.IsNullOrEmpty()) return null;

            return quotes.GetMacd(fastPeriods, slowPeriods, signalPeriods)
                ?.Where(o => o.Macd.HasValue)
                ?.OrderBy(x => x.Date)
                ?.ToList();
        }

        public static Skender.Stock.Indicators.MacdResult? GetLastMacdResult(this IEnumerable<AppQuote> quotes,
            int fastPeriods = 12,
            int slowPeriods = 26,
            int signalPeriods = 9)
        {
            if (quotes.IsNullOrEmpty()) return null;

            var result = quotes.GetMacdResults(fastPeriods, slowPeriods, signalPeriods);
            return result?.LastOrDefault();
        }

        // --- SuperTrend --------------------------------------
        public static List<SuperTrendResult>? GetSuperTrendResults(this IEnumerable<AppQuote> quotes,
            int lookbackPeriods = 10,
            double multiplier = 3)
        {
            if (quotes.IsNullOrEmpty() || quotes.Count() <= lookbackPeriods) return null;

            return quotes.GetSuperTrend(lookbackPeriods, multiplier)
                ?.Where(o => o.SuperTrend.HasValue)
                ?.OrderBy(x => x.Date)
                ?.ToList();
        }

        public static SuperTrendResult? GetLastSuperTrendResult(this IEnumerable<AppQuote> quotes,
            int lookbackPeriods = 10,
            double multiplier = 3)
        {
            if (quotes.IsNullOrEmpty() || quotes.Count() <= lookbackPeriods) return null;

            var result = quotes.GetSuperTrendResults(lookbackPeriods, multiplier);
            return result?.LastOrDefault();
        }

        // --- Vortex --------------------------------------
        public static List<VortexResult>? GetVortexResults(this IEnumerable<AppQuote> quotes,
            int lookbackPeriods = 14)
        {
            if (quotes.IsNullOrEmpty() || quotes.Count() <= lookbackPeriods) return null;

            var result = quotes.GetVortex(lookbackPeriods); // review
            return result.ToList();
        }

        public static VortexResult? GetLastVortexResult(this IEnumerable<AppQuote> quotes,
            int lookbackPeriods = 14)
        {
            if (quotes.IsNullOrEmpty() || quotes.Count() <= lookbackPeriods) return null;

            var result = quotes.GetVortexResults(lookbackPeriods);
            return result?.LastOrDefault();
        }

        // --- Alligator --------------------------------------
        public static List<AlligatorResult>? GetAlligatorResults(this IEnumerable<AppQuote> quotes,
            int jawPeriods = 13,
            int jawOffset = 8,
            int teethPeriods = 8,
            int teethOffset = 5,
            int lipsPeriods = 5,
            int lipsOffset = 3)
        {
            if (quotes.IsNullOrEmpty()) return null;

            return quotes.GetAlligator(jawPeriods, jawOffset, teethPeriods, teethOffset, lipsPeriods, lipsOffset)
                ?.Where(o => o.Jaw.HasValue)
                ?.OrderBy(x => x.Date)
                ?.ToList();
        }

        public static AlligatorResult? GetLastAlligatorResult(this IEnumerable<AppQuote> quotes,
            int jawPeriods = 13,
            int jawOffset = 8,
            int teethPeriods = 8,
            int teethOffset = 5,
            int lipsPeriods = 5,
            int lipsOffset = 3)
        {
            if (quotes.IsNullOrEmpty()) return null;

            var result = quotes.GetAlligatorResults(jawPeriods, jawOffset, teethPeriods, teethOffset, lipsPeriods, lipsOffset);
            return result?.LastOrDefault();
        }
    }
}
