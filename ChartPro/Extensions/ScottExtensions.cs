using Cuckoo.Shared;
using ScottPlot;
using ScottPlot.WinForms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartPro
{
    public static class ScottExtensions
    {
        // Scott 5
        public static List<OHLC> ToOHLCs(this List<AppQuote> quotes, Interval interval)
        {
            return quotes.Select(x => x.ToOHLC(timeSpan: interval.ToTimeSpan()))?.ToList();
        }

        public static OHLC ToOHLC(this AppQuote quote, TimeSpan timeSpan)
        {
            return new OHLC
            (
                open: (double)quote.Open,
                high: (double)quote.High,
                low: (double)quote.Low,
                close: (double)quote.Close,
                start: quote.Date,          // open time
                span: timeSpan              // width of the OHLC
            );
        }

        //
        public static void Configs(this FormsPlot formsPlot)
        {
            // disable left & bottom ticks
            formsPlot.Plot.Axes.Left.TickGenerator = new ScottPlot.TickGenerators.EmptyTickGenerator();
            formsPlot.Plot.Axes.Bottom.TickGenerator = new ScottPlot.TickGenerators.EmptyTickGenerator();
            // extra padding on the bottom and left for the rotated labels
            formsPlot.Plot.Axes.Bottom.MinimumSize = 60;
            formsPlot.Plot.Axes.Left.MinimumSize = 60;
        }
    }
}
