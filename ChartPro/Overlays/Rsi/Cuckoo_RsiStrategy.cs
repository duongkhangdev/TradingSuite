using Cuckoo.Shared;
using ScottPlot;
using ScottPlot.Plottables;
using ScottPlot.WinForms;
using Skender.Stock.Indicators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartPro
{

    public class Cuckoo_RsiStrategy
    {
        #region Rsi Reverse

        public static async Task<List<SignalResult>?> Signal_Reverse(List<AppQuote>? quotes, string symbol, string timeframe,
            Dictionary<string, object>? indicators, Dictionary<string, object>? filters = null)
        {
            List<SignalResult> outputs = new();

            if (quotes == null || quotes.Count < 2 || string.IsNullOrEmpty(symbol) || string.IsNullOrEmpty(timeframe))
                return outputs;

            indicators!.TryGet<List<RsiResult>>("Rsi", out var rsis);
            indicators!.TryGet<List<EmaResult>>("Ema5", out var emas5);
            indicators!.TryGet<List<SmaResult>>("Sma7", out var smas7);
            indicators!.TryGet<List<SmaResult>>("Sma25", out var smas25);
            indicators!.TryGet<List<BollingerBandsResult>>("BB12", out var bbs12);
            indicators!.TryGet<List<BollingerBandsResult>>("BB17", out var bbs17);
            indicators!.TryGet<List<BollingerBandsResult>>("BB25", out var bbs25);
            indicators!.TryGet<List<BollingerBandsResult>>("BB35", out var bbs35);
            indicators!.TryGet<List<BollingerBandsResult>>("BB45", out var bbs45);

            if (emas5 == null || bbs12 == null || bbs17 == null || emas5.Count < 2 || bbs12.Count < 2 || bbs17.Count < 2)
                return outputs;

            for (int i = 5; i < rsis!.Count; i++)
            {
                var prev2Rsi = rsis[i - 2];
                var prevRsi = rsis[i - 1];
                var currRsi = rsis[i];

                var prevBB12 = bbs12?.Find(x => x.Date == prevRsi.Date);
                var currBB12 = bbs12?.Find(x => x.Date == currRsi.Date);
                var prevBB17 = bbs17?.Find(x => x.Date == prevRsi.Date);
                var currBB17 = bbs17?.Find(x => x.Date == currRsi.Date);
                var prevBB25 = bbs25?.Find(x => x.Date == prevRsi.Date);
                var currBB25 = bbs25?.Find(x => x.Date == currRsi.Date);

                var prevSma25 = smas25?.Find(x => x.Date == prevRsi.Date);
                var currSma25 = smas25?.Find(x => x.Date == currRsi.Date);

                var prevQuote = quotes.Find(x => x.Date == prevRsi.Date);
                var currQuote = quotes.Find(x => x.Date == currRsi.Date);

                if (currRsi != null && currSma25 != null && currBB12 != null && currBB17 != null && currBB25 != null &&
                    prevRsi != null && prevSma25 != null && prevBB12 != null && prevBB17 != null && prevBB25 != null)
                {
                    var indicatorResult = new Dictionary<string, object>
                    {
                        ["Rsi"] = currRsi!,
                        ["Sma25"] = currSma25!,
                        ["BB12"] = currBB12!,
                        ["BB17"] = currBB17!,
                        ["BB25"] = currBB25!,
                    };

                    bool crossBull = prev2Rsi?.Rsi < 30 && prevRsi.Rsi <= 30 && currRsi.Rsi > 30;
                    bool crossBear = prev2Rsi?.Rsi > 70 && prevRsi.Rsi >= 70 && currRsi.Rsi < 70;

                    if (crossBull)
                    {
                        if (true)
                        {
                            var signal = new SignalResult
                            {
                                Symbol = symbol,
                                Timeframe = timeframe,
                                Kind = SignalKind.BUY,
                                IndicatorResult = indicatorResult,
                                Quote = currQuote,
                                CrossAt = (double)prevSma25.Sma!,
                                Note = "Rsi > 30",
                                Label = "B",
                            };

                            outputs.Add(signal);
                        }
                    }
                    else if (crossBear)
                    {
                        if (true)
                        {
                            var signal = new SignalResult
                            {
                                Symbol = symbol,
                                Timeframe = timeframe,
                                Kind = SignalKind.SELL,
                                IndicatorResult = indicatorResult,
                                Quote = currQuote,
                                CrossAt = (double)prevSma25.Sma!,
                                Note = "Rsi < 30",
                                Label = "S",
                            };

                            outputs.Add(signal);
                        }
                    }
                    else
                    {

                    }
                }
            }

            return await Task.FromResult(outputs);
        }

        #endregion
    }

    public class Cuckoo_RsiStrategyOverlay
    {
        public static async Task<List<PlottableModel>?> DrawAll(FormsPlot formsPlot, CandlestickPlot candlePlot, List<AppQuote>? quotes, List<SignalResult> data)
        {
            List<PlottableModel>? list = new();
            foreach (var item in data)
            {
                var result = await DrawOne(formsPlot, candlePlot, quotes, item);
                if (result != null)
                    list.AddRange(result);
            }

            return list;
        }

        public static async Task<List<PlottableModel>?> DrawOne(FormsPlot formsPlot, CandlestickPlot candlePlot, List<AppQuote>? quotes, SignalResult? data)
        {
            List<PlottableModel>? list = new();

            if (data == null || data.Quote == null)
                return list;

            // Xác định offset theo tỉ lệ span của trục Y mà candlestick dùng (trái/phải)
            var yAxis = candlePlot.Axes.YAxis;
            double span = yAxis.GetRange().Span;
            double dy = span * 0.015; // 1.5% span để đẩy nhãn lệch lên/xuống

            var quote = data.Quote;
            double x = ScottHelper.GetXForIndex(candlePlot, quotes!, quote!);
            double y = data.Kind == SignalKind.BUY ? (double)quote.Low : (double)quote.High;
            double yLabel = data.Kind == SignalKind.BUY ? (double)quote.Low - dy : (double)quote.High + dy;

            string lbl = !string.IsNullOrEmpty(data.Label) ? data.Label : (data.Kind == SignalKind.BUY) ? "B" : (data.Kind == SignalKind.SELL) ? "S" : "None";
            var color = data.Kind == SignalKind.BUY ? Colors.SeaGreen.WithAlpha(.85f) : data.Kind == SignalKind.SELL ? Colors.OrangeRed.WithAlpha(.85f) : Colors.Gray.WithAlpha(.85f);

            var callout = formsPlot.Plot.Add.Callout(lbl, new Coordinates(x, yLabel), new Coordinates(x, y));
            callout.Axes.YAxis = yAxis;
            callout.TextColor = Colors.White;
            callout.TextBackgroundColor = color;
            callout.TextBorderWidth = 0;
            callout.FontSize = 10;
            callout.ArrowLineColor = Colors.Gray;
            callout.ArrowFillColor = Colors.Gray;
            callout.ArrowLineWidth = 0.5f;

            list.Add(new PlottableModel("", $"{1}", $"{"XAUUSD"}", callout, null, PlottType.CallOut));

            return await Task.FromResult(list);
        }
    }
}
