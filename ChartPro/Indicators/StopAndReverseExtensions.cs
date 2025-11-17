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
        // --- Chandelier --------------------------------------
        public static List<ChandelierResult>? GetChandelierResults(this IEnumerable<AppQuote> quotes,
            int lookbackPeriods = 22,
            double multiplier = 3,
            ChandelierType chandelierType = ChandelierType.Long)
        {
            if (quotes.IsNullOrEmpty() || quotes.Count() <= lookbackPeriods) return null;

            return quotes.GetChandelier(lookbackPeriods, multiplier, chandelierType)
                ?.Where(o => o.ChandelierExit.HasValue)
                ?.OrderBy(x => x.Date)
                ?.ToList();
        }

        public static ChandelierResult? GetLastChandelierResult(this IEnumerable<AppQuote> quotes,
            int lookbackPeriods = 22,
            double multiplier = 3,
            ChandelierType chandelierType = ChandelierType.Long)
        {
            if (quotes.IsNullOrEmpty() || quotes.Count() <= lookbackPeriods) return null;

            var result = quotes.GetChandelierResults(lookbackPeriods, multiplier, chandelierType);
            return result?.LastOrDefault();
        }

        // --- ParabolicSar --------------------------------------
        public static List<ParabolicSarResult>? GetParabolicSarResults(this IEnumerable<AppQuote> quotes,
            double accelerationStep,
            double maxAccelerationFactor)
        {
            if (quotes.IsNullOrEmpty()) return null;

            return quotes.GetParabolicSar(accelerationStep, maxAccelerationFactor)
                ?.Where(o => o.Sar.HasValue)
                ?.OrderBy(x => x.Date)
                ?.ToList();
        }

        public static ParabolicSarResult? GetLastParabolicSarResult(this IEnumerable<AppQuote> quotes,
            double accelerationStep,
            double maxAccelerationFactor)
        {
            if (quotes.IsNullOrEmpty()) return null;

            var result = quotes.GetParabolicSarResults(accelerationStep, maxAccelerationFactor);
            return result?.LastOrDefault();
        }

        // --- VolatilityStop --------------------------------------
        public static List<VolatilityStopResult>? GetVolatilityStopResults(this IEnumerable<AppQuote> quotes,
            int lookbackPeriods = 20,
            double multiplier = 2)
        {
            if (quotes.IsNullOrEmpty() || quotes.Count() <= lookbackPeriods) return null;

            return quotes.GetVolatilityStop(lookbackPeriods, multiplier)
                ?.Where(o => o.Sar.HasValue)
                ?.OrderBy(x => x.Date)
                ?.ToList();
        }
        
        public static VolatilityStopResult? GetLastVolatilityStopResult(this IEnumerable<AppQuote> quotes,
            int lookbackPeriods = 20,
            double multiplier = 2)
        {
            if (quotes.IsNullOrEmpty() || quotes.Count() <= lookbackPeriods) return null;

            var result = quotes.GetVolatilityStopResults(lookbackPeriods, multiplier);
            return result?.LastOrDefault();
        }
    }
}
