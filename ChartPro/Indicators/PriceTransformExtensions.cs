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
        // --- BasicData --------------------------------------
        public static List<BasicData>? GetBasicDataResults(this IEnumerable<AppQuote> quotes, CandlePart candlePart)
        {
            if (quotes.IsNullOrEmpty()) return null;

            var result = quotes.GetBaseQuote(candlePart);
            return result.ToList();
        }

        public static BasicData? GetLastBasicDataResult(this IEnumerable<AppQuote> quotes, CandlePart candlePart)
        {
            if (quotes.IsNullOrEmpty()) return null;

            var result = quotes.GetBasicDataResults(candlePart);
            return result?.LastOrDefault();
        }

        // --- FisherTransform --------------------------------------
        public static List<FisherTransformResult>? GetFisherTransformResults(this IEnumerable<AppQuote> quotes,
            int lookbackPeriods = 10)
        {
            if (quotes.IsNullOrEmpty() || quotes.Count() <= lookbackPeriods) return null;

            var result = quotes.GetFisherTransform(lookbackPeriods);
            return result.ToList();
        }

        public static FisherTransformResult? GetLastFisherTransformResult(this IEnumerable<AppQuote> quotes,
            int lookbackPeriods = 10)
        {
            if (quotes.IsNullOrEmpty() || quotes.Count() <= lookbackPeriods) return null;

            var result = quotes.GetFisherTransformResults(lookbackPeriods);
            return result?.LastOrDefault();
        }

        // --- HeikinAshi --------------------------------------
        public static List<HeikinAshiResult>? GetHeikinAshiResults(this IEnumerable<AppQuote> quotes)
        {
            if (quotes.IsNullOrEmpty()) return null;

            return quotes.GetHeikinAshi()
                ?.Where(o => o.Open > 0)
                ?.OrderBy(x => x.Date)
                ?.ToList();
        }

        //public static List<AppQuote>? GetAppQuoteFromHeikinAshi(this IEnumerable<AppQuote> quotes)
        //{
        //    if (quotes.IsNullOrEmpty()) return null;

        //    return quotes.GetHeikinAshi()
        //        ?.Where(o => o.Open > 0)                
        //        ?.ToAppQuotes()
        //        ?.OrderBy(x => x.Date)
        //        ?.ToList();
        //}


        public static HeikinAshiResult? GetLastHeikinAshiResult(this IEnumerable<AppQuote> quotes)
        {
            if (quotes.IsNullOrEmpty()) return null;

            var result = quotes.GetHeikinAshiResults();
            return result?.LastOrDefault();
        }

        // --- Renko --------------------------------------
        public static List<RenkoResult>? GetRenkoResults(this IEnumerable<AppQuote> quotes,
            decimal brickSize,
            EndType endType = EndType.Close)
        {
            if (quotes.IsNullOrEmpty()) return null;

            var result = quotes.GetRenko(brickSize, endType);
            return result.ToList();
        }

        public static RenkoResult? GetLastRenkoResult(this IEnumerable<AppQuote> quotes,
            decimal brickSize,
            EndType endType = EndType.Close)
        {
            if (quotes.IsNullOrEmpty()) return null;

            var result = quotes.GetRenkoResults(brickSize, endType);
            return result?.LastOrDefault();
        }

        // --- ZigZag --------------------------------------
        public static List<ZigZagResult>? GetZigZagResults(this IEnumerable<AppQuote> quotes,
            EndType endType = EndType.HighLow,
            decimal percentChange = 5)
        {
            if (quotes.IsNullOrEmpty()) return null;

            var result = quotes.GetZigZag(endType, percentChange);
            return result.ToList();
        }

        public static List<ZigZagResult>? GetZigZagFilterResults(this IEnumerable<AppQuote> quotes,
            EndType endType = EndType.HighLow,
            decimal percentChange = 5)
        {
            if (quotes.IsNullOrEmpty()) return null;

            var result = quotes.GetZigZag(endType, percentChange);

            var zigZagPointTypes = result.WhereIf(result.IsNotNullAndNotEmpty(), x => x.PointType != null)?.OrderBy(x => x.Date)?.ToList();

            // Đỉnh
            var hightPoints = result.WhereIf(result.IsNotNullAndNotEmpty(), x => x.PointType == "H")?.OrderBy(x => x.Date)?.ToList();
            // Đáy
            var lowPoints = result.WhereIf(result.IsNotNullAndNotEmpty(), x => x.PointType == "L")?.OrderBy(x => x.Date)?.ToList();
            // Đỉnh, đáy gần kline đang run
            var nearest = (zigZagPointTypes.IsNotNullAndNotEmpty()) ? zigZagPointTypes.LastOrDefault().PointType : "";

            var items = new List<ZigZagResult>();
            if (hightPoints.IsNotNullAndNotEmpty())
                items.AddRange(hightPoints);
            if (lowPoints.IsNotNullAndNotEmpty())
                items.AddRange(lowPoints);

            return items?.OrderBy(o => o.Date)?.ToList();
        }

        public static ZigZagResult? GetLastZigZagResult(this IEnumerable<AppQuote> quotes,
            EndType endType = EndType.HighLow,
            decimal percentChange = 5)
        {
            if (quotes.IsNullOrEmpty()) return null;

            var result = quotes.GetZigZagResults(endType, percentChange);
            return result?.LastOrDefault();
        }
    }
}
