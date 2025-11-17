using Cuckoo.Shared;
using ScottPlot;
using ScottPlot.Colormaps;
using ScottPlot.Plottables;
using ScottPlot.WinForms;
using Serilog;
using Skender.Stock.Indicators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartPro
{
    public class Cuckoo_RsiDetector
    {
        public static async Task<List<PlottableModel>?> Draw_OverboughtOversoldAsync(FormsPlot formsPlot, CandlestickPlot candlePlot,
            List<AppQuote>? quotes, Dictionary<string, object>? indicators, Dictionary<string, object>? filters, string symbol, string timeFrame)
        {
            List<PlottableModel>? list = new();

            indicators!.TryGet<List<RsiResult>>("Rsi", out var rsis);
            filters!.TryGet<double>("Rsi-OB", out var overboughtThreshold);
            filters!.TryGet<double>("Rsi-OS", out var oversoldThreshold);

            var throttler = new SemaphoreSlim(initialCount: 4);
            for (int i = 1; i < quotes!.Count; i++)
            {
                await throttler.WaitAsync();
                try
                {
                    var prevQuote = quotes[i - 1];
                    var currQuote = quotes[i];
                    
                    var prevRsi = rsis?.Find(x => x.Date == prevQuote.Date);
                    var currRsi = rsis?.Find(x => x.Date == currQuote.Date);

                    if (currRsi != null)
                    {
                        if (currRsi.Rsi >= overboughtThreshold)
                        {
                            if (prevRsi != null && prevRsi.Rsi < overboughtThreshold)
                            {
                                Log.Information($"RsiDetector: Overbought signal at {currQuote.Date} with RSI = {currRsi.Rsi}");
                                var result = await Draw_PriceChart(formsPlot, candlePlot, quotes, i, false);
                                if (result != null && result.Any())
                                {
                                    list.AddRange(result);
                                }
                            }                            
                        }
                        else if (currRsi.Rsi <= oversoldThreshold)
                        {
                            if (prevRsi != null && prevRsi.Rsi > oversoldThreshold)
                            {
                                Log.Information($"RsiDetector: Oversold signal at {currQuote.Date} with RSI = {currRsi.Rsi}");
                                var result = await Draw_PriceChart(formsPlot, candlePlot, quotes, i, true);
                                if (result != null && result.Any())
                                {
                                    list.AddRange(result);
                                }
                            }                            
                        }
                        else
                        {

                        }
                    }
                }
                finally
                {
                    throttler.Release();
                }                
            }

            return list;
        }

        public static async Task<List<PlottableModel>?> Draw_PriceChart(FormsPlot formsPlot, CandlestickPlot candlePlot, List<AppQuote>? quotes, int index, bool isOS)
        {
            List<PlottableModel>? list = new();

            // Xác định offset theo tỉ lệ span của trục Y mà candlestick dùng (trái/phải)
            var yAxis = candlePlot.Axes.YAxis;
            double span = yAxis.GetRange().Span;
            double dy = span * 0.015; // 1.5% span để đẩy nhãn lệch lên/xuống

            double x = ScottHelper.GetXForIndex(candlePlot, quotes, index);
            var quote = quotes[index];
            double yLabel = isOS ? (double)quote.Low - dy : (double)quote.High + dy;
            double y = isOS ? (double)quote.Low : (double)quote.High;
            string lbl = isOS ? "Rsi-OS" : "Rsi-OB";
                       
            var callout = formsPlot.Plot.Add.Callout(lbl, new Coordinates(x, yLabel), new Coordinates(x, y));
            //callout = plot.Add.Text(sp.Label, new Coordinates(x, yLabel), new Coordinates(x, sp.Price));
            callout.Axes.YAxis = yAxis;
            callout.TextColor = Colors.White;
            callout.TextBackgroundColor = isOS ? Colors.OrangeRed.WithAlpha(.85f) : Colors.SeaGreen.WithAlpha(.85f);
            callout.TextBorderWidth = 0;
            callout.FontSize = 10;
            callout.ArrowLineColor = Colors.Gray;
            callout.ArrowFillColor = Colors.Gray;
            callout.ArrowLineWidth = 1f;

            list.Add(new PlottableModel("", $"{1}", $"{"XAUUSD"}", callout, null, PlottType.CallOut));
            
            await Task.CompletedTask;
            return list;
        }

        /// <summary>
        /// Vẽ RSI trên subplot riêng (formsPlot) dùng cùng trục X với candlestick để khớp thời gian.
        /// Trục Y phải (0-100). Không dùng trục giá của candlestick.
        /// </summary>
        public static async Task<List<PlottableModel>?> Draw_SubChart(
            FormsPlot formsPlot,
            CandlestickPlot candlePlot,
            List<AppQuote> quotes,
            string symbol,
            string timeFrame)
        {
            List<PlottableModel>? list = new();
            if (quotes.IsNullOrEmpty())
                return null;

            // Lấy dữ liệu RSI (period 14)
            var data = quotes.GetRsiResults(14);
            if (data.IsNullOrEmpty())
                return null;

            var plt = formsPlot.Plot;
            // Chuẩn bị layout subplot giống ChartSubplotService
            plt.Clear();
            plt.Axes.DateTimeTicksBottom();
            plt.Axes.Bottom.IsVisible = false; // ẩn nhãn thời gian ở subplot
            plt.Axes.Left.IsVisible = false;
            plt.Axes.Right.IsVisible = true;
            // đặt giới hạn Y 0-100 trực tiếp qua Axes.SetLimitsY
            plt.Axes.SetLimitsY(0, 100);
            // Ẩn lưới cho subplot
            plt.Grid.IsVisible = false;
            plt.Grid.XAxisStyle.IsVisible = false;
            plt.Grid.YAxisStyle.IsVisible = false;

            // Thêm các đường mức 70 / 30
            var h70 = plt.Add.HorizontalLine(70); h70.Color = ScottPlot.Colors.SeaGreen; h70.Axes.YAxis = plt.Axes.Right; h70.Axes.XAxis = plt.Axes.Bottom;
            var h30 = plt.Add.HorizontalLine(30); h30.Color = ScottPlot.Colors.OrangeRed; h30.Axes.YAxis = plt.Axes.Right; h30.Axes.XAxis = plt.Axes.Bottom;

            // Phân đoạn theo vùng OB/OS để đổi màu
            var segments = new List<RsiSegment>();
            RsiSegment? currentSeg = null;

            for (int i = 0; i < quotes.Count; i++)
            {
                var q = quotes[i];
                var rsiPoint = data!.Find(d => d.Date == q.Date);
                if (rsiPoint?.Rsi is null)
                    continue;

                double xVal = ScottHelper.GetXForIndex(candlePlot.Sequential, quotes, q);
                double yVal = (double)rsiPoint.Rsi!;

                int sign = yVal >= 70 ? +1 : (yVal <= 30 ? -1 : 0);

                if (currentSeg == null)
                {
                    currentSeg = new RsiSegment(sign);
                    currentSeg.Xs.Add(xVal);
                    currentSeg.Ys.Add(yVal);
                }
                else if (sign == currentSeg.Sign)
                {
                    currentSeg.Xs.Add(xVal);
                    currentSeg.Ys.Add(yVal);
                }
                else
                {
                    segments.Add(currentSeg);
                    currentSeg = new RsiSegment(sign);
                    currentSeg.Xs.Add(xVal);
                    currentSeg.Ys.Add(yVal);
                }
            }
            if (currentSeg != null && currentSeg.Xs.Count > 0)
                segments.Add(currentSeg);

            // Vẽ từng segment
            for (int si = 0; si < segments.Count; si++)
            {
                var seg = segments[si];
                var sc = plt.Add.ScatterLine(seg.Xs.ToArray(), seg.Ys.ToArray());
                sc.MarkerSize = 0;
                sc.Axes.XAxis = plt.Axes.Bottom;
                sc.Axes.YAxis = plt.Axes.Right;
                sc.Color = seg.Sign switch
                {
                    > 0 => ScottPlot.Colors.SeaGreen,
                    < 0 => ScottPlot.Colors.OrangeRed,
                    _ => ScottPlot.Colors.Gray
                };
                list.Add(new PlottableModel("", si.ToString(), symbol, sc, null, PlottType.Scatter));
            }

            // đảm bảo giới hạn cuối cùng 0-100
            plt.Axes.SetLimitsY(0, 100);
            plt.Axes.AutoScaleX(); // chỉ autoscale trục X
            formsPlot.Refresh();

            await Task.CompletedTask;
            return list;
        }

        private class RsiSegment
        {
            public int Sign { get; }
            public List<double> Xs { get; } = new();
            public List<double> Ys { get; } = new();
            public RsiSegment(int sign) => Sign = sign;
        }
    }
}
