using Cuckoo.Shared;
using ScottPlot;
using ScottPlot.Plottables;
using ScottPlot.WinForms;
using Skender.Stock.Indicators;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace ChartPro
{
    public class ScottHelper
    {
        public static T? AsPlottable<T>(IPlottable p) where T : class, IPlottable
            => p as T;

        public static void FixRightAxisWidth(FormsPlot fp, int px = 68)
        {
            // Khoá cứng bề rộng panel trục phải để đồng nhất pixel
            var right = fp.Plot.Axes.Right;
            right.MinimumSize = px;
            right.MaximumSize = px;
        }

        public static Coordinates GetMouseCoordinates(FormsPlot formsPlot, CandlestickPlot candlestickPlot)
        {
            // Lấy vị trí chuột (screen) -> tọa độ client của formsPlot1
            Pixel px = GetCursorPositionPixel(formsPlot);
            // Chuyển đổi sang tọa độ dữ liệu, dùng đúng trục đang vẽ CandlePlot
            return formsPlot!.Plot.GetCoordinates(
                                pixel: px,
                                xAxis: candlestickPlot?.Axes.XAxis,
                                yAxis: candlestickPlot?.Axes.YAxis);
        }

        public static Coordinates GetMouseCoordinates(FormsPlot formsPlot, CandlestickPlot candlestickPlot, Pixel px)
        {
            return formsPlot!.Plot.GetCoordinates(
                                pixel: px,
                                xAxis: candlestickPlot?.Axes.XAxis,
                                yAxis: candlestickPlot?.Axes.YAxis);
        }

        public static Pixel GetCursorPositionPixel(FormsPlot formsPlot)
        {
            Point client = formsPlot!.PointToClient(System.Windows.Forms.Cursor.Position);
            var px = new ScottPlot.Pixel(client.X, client.Y);
            return px;
        }

        public static void ResetChart(FormsPlot formsPlot)
        {
            // Giả sử bạn chỉ có 1 Candlestick trên chart:
            var candles = formsPlot.Plot.GetPlottables().OfType<CandlestickPlot>().FirstOrDefault();
            var financeAxis = formsPlot.Plot.GetPlottables().OfType<FinancialTimeAxis>().FirstOrDefault();

            var toRemove = formsPlot.Plot.GetPlottables()
                .Where(p => p != candles)
                .ToList();

            foreach (var p in toRemove)
            {
                formsPlot.Plot.Remove(p);
            }

            formsPlot.Refresh();
        }

        public static void ResetChart(FormsPlot formsPlot, CandlestickPlot candlestickPlot)
        {
            // Giả sử bạn đã có biến 'candles' là CandlestickPlot bạn muốn giữ lại
            var toRemove = formsPlot.Plot.GetPlottables()
                .Where(p => p != candlestickPlot) // Giữ lại nến
                .ToList();

            foreach (var p in toRemove)
            {
                formsPlot.Plot.Remove(p);
            }

            formsPlot.Refresh();
        }

        /// <summary>
        /// Reset chart giữ lại duy nhất:
        /// - CandlestickPlot (nến)
        /// - FinancialTimeAxis (axis tài chính nếu có)
        /// - Annotation (danh sách chú thích)
        /// Các plottable khác sẽ bị xóa.
        /// </summary>
        public static void ResetChartX(FormsPlot formsPlot)
        {
            if (formsPlot == null) 
                throw new ArgumentNullException(nameof(formsPlot));
            
            var plt = formsPlot.Plot;
            // Lấy snapshot các plottable hiện tại (sao chép danh sách để tránh sửa đổi khi duyệt)
            var all = plt.GetPlottables().ToList();

            // Các đối tượng cần giữ lại
            var candles = all.OfType<CandlestickPlot>().FirstOrDefault();
            var financeAxis = all.OfType<FinancialTimeAxis>().FirstOrDefault();
            var annotations = all.OfType<Annotation>().ToList();

            // Tạo tập các plottable cần giữ (nullable safe)
            var keep = new HashSet<IPlottable>();
            if (candles != null) 
                keep.Add(candles);
            
            if (financeAxis != null) 
                keep.Add(financeAxis);
            
            foreach (var a in annotations)
            {
                if (a != null)
                    keep.Add(a);
            }

            // Xóa mọi plottable không nằm trong keep
            foreach (var p in all)
            {
                if (!keep.Contains(p))
                    plt.Remove(p);
            }

            // Refresh chart để áp dụng thay đổi
            formsPlot.Refresh();
        }

        public static int GetIndex(List<AppQuote> quotes, DateTime date)
        {
            var idx = quotes.FindIndex(x => x.Date == date);
            return idx;
        }

        public static int GetIndex(List<AppQuote> quotes, AppQuote quoteAt)
        {
            var idx = quotes.FindIndex(x => x.Date == quoteAt.Date);
            return idx;
        }

        public static double GetXForIndex(CandlestickPlot candlePlot, List<AppQuote> quotes, int index)
        {
            if (candlePlot.Sequential)
                return index;
            else
                return NumericConversion.ToNumber(quotes[index].Date);
        }

        public static double GetXForIndex(CandlestickPlot candlePlot, List<AppQuote> quotes, DateTime date)
        {
            var idx1 = quotes.FindIndex(x => x.Date == date);
            if (candlePlot.Sequential)
                return idx1;
            else
                return NumericConversion.ToNumber(quotes[idx1].Date);
        }

        public static double GetXForIndex(CandlestickPlot candlePlot, List<AppQuote> quotes, AppQuote quote)
        {
            var idx = quotes.FindIndex(x => x.Date == quote.Date);
            if (candlePlot.Sequential)
                return idx;
            else
                return NumericConversion.ToNumber(quotes[idx].Date);
        }

        public static double GetXForIndex(bool hasGap, List<AppQuote> quotes, int index)
        {
            if (hasGap)
                return index;
            else
                return NumericConversion.ToNumber(quotes[index].Date);
        }

        public static double GetXForIndex(bool hasGap, List<AppQuote> quotes, AppQuote quote)
        {
            var idx = quotes.FindIndex(x => x.Date == quote.Date);
            if (hasGap)
                return idx;
            else
                return NumericConversion.ToNumber(quotes[idx].Date);
        }

        public static double GetXForIndexOffset(CandlestickPlot candlePlot, IReadOnlyList<OHLC> ohlcs, int index, int barsOffset)
        {
            if (candlePlot.Sequential)
                return index + barsOffset;

            TimeSpan span = ohlcs[index].TimeSpan;
            if (span == TimeSpan.Zero)
            {
                if (index + 1 < ohlcs.Count)
                    span = ohlcs[index + 1].DateTime - ohlcs[index].DateTime;
                else if (index > 0)
                    span = ohlcs[index].DateTime - ohlcs[index - 1].DateTime;
                else
                    span = TimeSpan.FromDays(1);
            }

            DateTime dt = ohlcs[index].DateTime + TimeSpan.FromTicks(span.Ticks * barsOffset);
            return NumericConversion.ToNumber(dt);
        }

        /// <summary>
        /// Tính X cho 1 mốc tương lai sau 'steps' nến mà KHÔNG thêm nến:
        /// - hasGap = true  → X theo index: lastIndex + steps (âm để lùi)
        /// - hasGap = false → X theo thời gian: NumericConversion.ToNumber(DateTime tương lai)
        /// </summary>
        public static double GetFutureXAfterSteps(
            bool hasGap,
            List<AppQuote> quotes,
            string timeframe,
            int steps)
        {
            if (quotes == null || quotes.Count == 0) 
                return 0;

            int lastIndex = quotes.Count - 1;

            if (steps == 0)
            {
                // X của nến hiện tại cuối danh sách
                return hasGap
                    ? lastIndex
                    : SafeToNumber(quotes[lastIndex].Date, lastIndex);
            }

            if (hasGap)
            {
                long x = (long)lastIndex + steps;
                return x; // cho phép âm nếu lùi
            }
            else
            {
                DateTime lastTime = quotes[lastIndex].Date;
                DateTime future = TimeframeHelper.GetNextDateTime(lastTime, timeframe, steps);
                return SafeToNumber(future, lastIndex + steps);
            }
        }

        /// <summary>
        /// Sinh danh sách 'count' toạ độ X tương lai (không gồm mốc hiện tại), không thêm nến:
        /// - hasGap = true  → X = lastIndex + 1..count
        /// - hasGap = false → X = ToNumber(next times…)
        /// </summary>
        public static List<double> GetFutureXs(
            bool hasGap,
            List<AppQuote> quotes,
            string timeframe,
            int count)
        {
            var xs = new List<double>();
            if (quotes == null || quotes.Count == 0 || count <= 0) return xs;

            int lastIndex = quotes.Count - 1;

            if (hasGap)
            {
                for (int i = 1; i <= count; i++)
                    xs.Add(lastIndex + i);
            }
            else
            {
                DateTime t = quotes[lastIndex].Date;
                for (int i = 1; i <= count; i++)
                {
                    t = TimeframeHelper.GetNextDateTime(t, timeframe);
                    xs.Add(SafeToNumber(t, lastIndex + i));
                }
            }

            return xs;
        }

        // Dùng NumericConversion.ToNumber(DateTime) như bạn đang dùng; thêm fallback an toàn.
        private static double SafeToNumber(DateTime dt, int fallbackIndex)
        {
            try
            {
                return NumericConversion.ToNumber(dt);
            }
            catch
            {
                // Nếu conversion lỗi (hiếm), dùng index làm fallback để không crash.
                return fallbackIndex;
            }
        }

        // ==== thêm vào trong ScottHelper ====

        #region New helpers for X<->index mapping

        /// <summary>
        /// Clamp index vào [0, quotes.Count-1].
        /// </summary>
        public static int ClampIndex(List<AppQuote> quotes, int i)
            => Math.Max(0, Math.Min(quotes.Count - 1, i));

        /// <summary>
        /// Map thời gian bất kỳ về index gần nhất trong quotesHigher (dùng khi Sequential=true).
        /// Lấy nến M30 có Date gần nhất nhưng không nhỏ hơn time (kiểu "first candle after OB").
        /// Nếu không tìm thấy -> trả last index.
        /// </summary>
        public static int FindNearestBarIndexByTime(List<AppQuote> quotesHigher, DateTime time)
        {
            // tuyến tính đơn giản; có thể tối ưu binary search nếu cần
            for (int i = 0; i < quotesHigher.Count; i++)
            {
                if (quotesHigher[i].Date >= time)
                    return i;
            }
            return quotesHigher.Count - 1;
        }

        /// <summary>
        /// Tìm index gần nhất với toạ độ X hiện tại của trục, theo loại trục của candlestick:
        /// - Sequential = true  → X chính là index → round & clamp.
        /// - Sequential = false → X là "time number" → chuyển về DateTime rồi tìm index gần nhất theo thời gian.
        /// </summary>
        public static int GetNearestIndexForX(CandlestickPlot candlePlot, List<AppQuote> quotes, double x)
        {
            if (quotes is null || quotes.Count == 0)
                return 0;

            if (candlePlot.Sequential)
            {
                // Trục index: X ~ index
                int i = (int)Math.Round(x);
                return ClampIndex(quotes, i);
            }
            else
            {
                // Trục thời gian: X ~ NumericConversion(DateTime)
                DateTime t;
                try
                {
                    t = NumericConversion.ToDateTime(x);
                }
                catch
                {
                    // Fallback hiếm khi cần: ScottPlot dùng OADate, cố gắng chuyển thẳng
                    t = DateTime.FromOADate(x);
                }

                int i0 = LowerBoundIndexByDate(quotes, t);
                if (i0 <= 0) return 0;
                if (i0 >= quotes.Count) return quotes.Count - 1;

                // Chọn gần hơn giữa i0 và i0-1
                var t0 = quotes[i0].Date.ToUniversalTime();
                var t1 = quotes[i0 - 1].Date.ToUniversalTime();
                var dt0 = Math.Abs((t0 - t).Ticks);
                var dt1 = Math.Abs((t - t1).Ticks);
                return (dt0 <= dt1) ? i0 : (i0 - 1);
            }
        }

        // [EXPLAIN] map Date -> nearest index trên LTF
        /// <summary>
        /// Tìm index gần nhất với toạ độ X trên LTF (khung thời gian nhỏ)
        /// </summary>
        /// <param name="ltfQuotes"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static int FindNearestIndexByTime(List<AppQuote> ltfQuotes, DateTime t)
        {
            // Tìm nhị phân gần đúng (quotes đã time-ascending)
            int lo = 0, hi = ltfQuotes.Count - 1;
            while (lo <= hi)
            {
                int mid = (lo + hi) / 2;
                var mt = ltfQuotes[mid].Date;
                if (mt == t) return mid;
                if (mt < t) lo = mid + 1; else hi = mid - 1;
            }
            // lo là first element > t; chọn gần nhất giữa lo và lo-1
            int i1 = Math.Max(0, Math.Min(ltfQuotes.Count - 1, lo));
            int i0 = Math.Max(0, i1 - 1);
            var d0 = Math.Abs((ltfQuotes[i0].Date - t).TotalSeconds);
            var d1 = Math.Abs((ltfQuotes[i1].Date - t).TotalSeconds);
            return (d0 <= d1) ? i0 : i1;
        }

        /// <summary>
        /// LowerBound: index đầu tiên có Date >= t (so sánh trên UTC).
        /// Nếu t trước chuỗi → trả 0; nếu sau chuỗi → trả quotes.Count.
        /// </summary>
        public static int LowerBoundIndexByDate(List<AppQuote> quotes, DateTime t)
        {
            DateTime tUtc = (t.Kind == DateTimeKind.Local) ? t.ToUniversalTime() : t;
            int lo = 0, hi = quotes.Count - 1, ans = quotes.Count;
            while (lo <= hi)
            {
                int mid = (lo + hi) >> 1;
                var dt = quotes[mid].Date.ToUniversalTime();
                if (dt >= tUtc)
                {
                    ans = mid;
                    hi = mid - 1;
                }
                else
                {
                    lo = mid + 1;
                }
            }
            return ans;
        }

        #endregion

        public static (double xStart, double xEnd) GetDaySpanForDailyCandle(
            CandlestickPlot candlePlotM5,
            List<AppQuote> quotesM5,
            AppQuote dailyCandle)
        {
            if (quotesM5 == null || quotesM5.Count == 0 || dailyCandle == null)
                return (0, 0);

            DateTime day = dailyCandle.Date.Date;
            int firstIdx = -1;
            int lastIdx = -1;

            for (int i = 0; i < quotesM5.Count; i++)
            {
                if (quotesM5[i].Date.Date == day)
                {
                    if (firstIdx == -1)
                        firstIdx = i;
                    lastIdx = i;
                }
            }

            if (firstIdx == -1 || lastIdx == -1)
            {
                // fallback: toàn bộ chart
                int firstAll = 0;
                int lastAll = quotesM5.Count - 1;

                double xStartAll = GetXForIndex(candlePlotM5, quotesM5, firstAll);
                double xEndAll = GetXForIndex(candlePlotM5, quotesM5, lastAll);

                return (xStartAll, xEndAll);
            }

            double xStart = GetXForIndex(candlePlotM5, quotesM5, firstIdx);
            double xEnd = GetXForIndex(candlePlotM5, quotesM5, lastIdx);

            return (xStart, xEnd);
        }

        private static void Usage(FormsPlot formsPlot, List<AppQuote> quotes)
        {
            // hasGap = true  => trục X theo index (gap)
            // hasGap = false => trục X theo thời gian (no-gap), dùng NumericConversion.ToNumber(DateTime)

            bool hasGapIndexAxis = true;
            bool hasNoGapTimeAxis = false;

            // 1) Lấy X sau 10 nến nữa (gap axis)
            double xAfter10_gap = GetFutureXAfterSteps(
                hasGap: hasGapIndexAxis,
                quotes: quotes,
                timeframe: "M1",
                steps: 10);

            // 2) Lấy X sau 10 nến nữa (no-gap time axis)
            double xAfter10_time = GetFutureXAfterSteps(
                hasGap: hasNoGapTimeAxis,
                quotes: quotes,
                timeframe: "H1",
                steps: 10);

            // 3) Danh sách 15 mốc X tương lai (để vẽ nhiều label/đường dọc)
            List<double> futureXs_gap = GetFutureXs(
                hasGap: true, quotes: quotes, timeframe: "M5", count: 15);

            List<double> futureXs_time = GetFutureXs(
                hasGap: false, quotes: quotes, timeframe: "D1", count: 5);

            // 4) Ví dụ vẽ đường dọc “TP target sau 5 nến H1” (ScottPlot, trục thời gian no-gap)
            double xTP = GetFutureXAfterSteps(false, quotes, "H1", 5);
            var vline = formsPlot.Plot.Add.VerticalLine(xTP);
            vline.LineWidth = 1;
            //vline.LineStyle = ScottPlot.LineStyle;
            vline.Color = ScottPlot.Colors.ForestGreen;
            formsPlot.Refresh();
        }
    }
}
