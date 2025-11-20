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
        // --- BollingerBands --------------------------------------
        public static List<BollingerBandsResult>? GetBollingerBandsResults(this IEnumerable<AppQuote> quotes,
            int lookbackPeriods = 20,
            double standardDeviations = 2)
        {
            if (quotes.IsNullOrEmpty() || quotes.Count() <= lookbackPeriods)
                return null;

            return quotes.GetBollingerBands(lookbackPeriods, standardDeviations)
                ?.Where(o => o.Sma.HasValue && o.UpperBand.HasValue && o.LowerBand.HasValue)
                ?.OrderBy(x => x.Date)
                ?.ToList();
        }

        public static BollingerBandsResult? GetLastBollingerBandsResult(this IEnumerable<AppQuote> quotes,
            int lookbackPeriods = 20,
            double standardDeviations = 2)
        {
            if (quotes.IsNullOrEmpty() || quotes.Count() <= lookbackPeriods) return null;

            var result = quotes.GetBollingerBandsResults(lookbackPeriods, standardDeviations);
            return result?.LastOrDefault();
        }

        // --- Donchian --------------------------------------
        public static List<DonchianResult>? GetDonchianResults(this IEnumerable<AppQuote> quotes,
            int lookbackPeriods = 20)
        {
            if (quotes.IsNullOrEmpty() || quotes.Count() <= lookbackPeriods) return null;

            return quotes.GetDonchian(lookbackPeriods)
                ?.Where(x=>x.Centerline.HasValue && x.UpperBand.HasValue && x.LowerBand.HasValue)
                ?.OrderBy(x => x.Date)
                ?.ToList();
        }

        public static DonchianResult? GetLastDonchianResult(this IEnumerable<AppQuote> quotes,
            int lookbackPeriods = 20)
        {
            if (quotes.IsNullOrEmpty() || quotes.Count() <= lookbackPeriods) return null;

            var result = quotes.GetDonchianResults(lookbackPeriods);
            return result?.LastOrDefault();
        }

        // --- Fcb --------------------------------------
        public static List<FcbResult>? GetFcbResults(this IEnumerable<AppQuote> quotes,
            int windowSpan = 2)
        {
            if (quotes.IsNullOrEmpty()) return null;

            var result = quotes.GetFcb(windowSpan);
            return result.ToList();
        }

        public static FcbResult? GetLastFcbResult(this IEnumerable<AppQuote> quotes,
            int windowSpan = 2)
        {
            if (quotes.IsNullOrEmpty()) return null;

            var result = quotes.GetFcbResults(windowSpan);
            return result?.LastOrDefault();
        }

        // --- Keltner --------------------------------------
        public static List<KeltnerResult>? GetKeltnerResults(this IEnumerable<AppQuote> quotes,
            int emaPeriods = 20,
            double multiplier = 2,
            int atrPeriods = 10)
        {
            if (quotes.IsNullOrEmpty()) return null;

            var result = quotes.GetKeltner(emaPeriods, multiplier, atrPeriods);
            return result.ToList();
        }

        public static KeltnerResult? GetLastKeltnerResult(this IEnumerable<AppQuote> quotes,
            int emaPeriods = 20,
            double multiplier = 2,
            int atrPeriods = 10)
        {
            if (quotes.IsNullOrEmpty()) return null;

            var result = quotes.GetKeltnerResults(emaPeriods, multiplier, atrPeriods);
            return result?.LastOrDefault();
        }

        // --- MaEnvelope --------------------------------------
        public static List<MaEnvelopeResult>? GetMaEnvelopeResults(this IEnumerable<AppQuote> quotes,
            int lookbackPeriods = 20,
            double percentOffset = 2.5,
            MaType movingAverageType = MaType.SMA)
        {
            if (quotes.IsNullOrEmpty() || quotes.Count() <= lookbackPeriods) return null;

            var result = quotes.GetMaEnvelopes(lookbackPeriods, percentOffset, movingAverageType); // review
            return result.ToList();
        }

        public static MaEnvelopeResult? GetLastMaEnvelopeResult(this IEnumerable<AppQuote> quotes,
            int lookbackPeriods = 20,
            double percentOffset = 2.5,
            MaType movingAverageType = MaType.SMA)
        {
            if (quotes.IsNullOrEmpty() || quotes.Count() <= lookbackPeriods) return null;

            var result = quotes.GetMaEnvelopeResults(lookbackPeriods, percentOffset, movingAverageType);
            return result?.LastOrDefault();
        }

        // --- PivotPoints --------------------------------------
        public static List<PivotPointsResult>? GetPivotPointsResults(this IEnumerable<AppQuote> quotes, PeriodSize windowSize)
        {
            if (quotes.IsNullOrEmpty()) return null;

            var result = quotes.GetPivotPoints(windowSize, PivotPointType.Standard); // review
            return result.ToList();
        }

        public static PivotPointsResult? GetLastPivotPointsResult(this IEnumerable<AppQuote> quotes, PeriodSize windowSize)
        {
            if (quotes.IsNullOrEmpty()) return null;

            var result = quotes.GetPivotPointsResults(windowSize);
            return result?.LastOrDefault();
        }

        // --- StarcBands --------------------------------------
        public static List<StarcBandsResult>? GetStarcBandsResults(this IEnumerable<AppQuote> quotes,
            int smaPeriods = 20,
            double multiplier = 2,
            int atrPeriods = 10)
        {
            if (quotes.IsNullOrEmpty()) return null;

            var result = quotes.GetStarcBands(smaPeriods, multiplier, atrPeriods);
            return result.ToList();
        }

        public static StarcBandsResult? GetLastStarcBandsResult(this IEnumerable<AppQuote> quotes,
            int smaPeriods = 20,
            double multiplier = 2,
            int atrPeriods = 10)
        {
            if (quotes.IsNullOrEmpty()) return null;

            var result = quotes.GetStarcBandsResults(smaPeriods, multiplier, atrPeriods);
            return result?.LastOrDefault();
        }

        // --- StdDevChannels --------------------------------------
        public static List<StdDevChannelsResult>? GetStdDevChannelsResults(this IEnumerable<AppQuote> quotes,
            int? lookbackPeriods = 20,
            double stdDeviations = 2)
        {
            if (quotes.IsNullOrEmpty() || quotes.Count() <= lookbackPeriods) return null;

            var result = quotes.GetStdDevChannels(lookbackPeriods, stdDeviations);
            return result
                ?.Where(x => x.Centerline.HasValue && x.UpperChannel.HasValue && x.LowerChannel.HasValue)
                ?.ToList();
        }

        public static StdDevChannelsResult? GetLastStdDevChannelsResult(this IEnumerable<AppQuote> quotes,
            int? lookbackPeriods = 20,
            double stdDeviations = 2)
        {
            if (quotes.IsNullOrEmpty() || quotes.Count() <= lookbackPeriods) return null;

            var result = quotes.GetStdDevChannelsResults(lookbackPeriods, stdDeviations);
            return result
                ?.OrderBy(x => x.Date).LastOrDefault();
        }
    }
}
