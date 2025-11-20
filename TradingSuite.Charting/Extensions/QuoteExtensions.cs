using Cuckoo.Shared;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TradingSuite.Charting.Extensions
{
    public static class QuoteExtensions
    {
        public static List<OHLC> ToOhlcs(this IEnumerable<AppQuote> quotes, Interval interval)
        {
            return quotes.Select(q => q.ToOhlc(interval.ToTimeSpan())).ToList();
        }

        public static OHLC ToOhlc(this AppQuote quote, TimeSpan span)
        {
            return new OHLC(
                open: (double)quote.Open,
                high: (double)quote.High,
                low: (double)quote.Low,
                close: (double)quote.Close,
                start: quote.Date,
                span: span);
        }
    }
}
