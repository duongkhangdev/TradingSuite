using ScottPlot;
using ScottPlot.Plottables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartPro
{
    /// <summary>
    /// Scott Plot 5.1.17 overlay drawing utilities
    /// </summary>
    public class ScottOverlay
    {
        public static IPlottable? Marker(Plot plt, CandlestickPlot candlePlot, double x, double y, ScottPlot.Color color, MarkerShape shape = MarkerShape.FilledCircle)
        {
            // Xác định offset theo tỉ lệ span của trục Y mà candlestick dùng (trái/phải)
            var xAxis = candlePlot.Axes.XAxis;
            var yAxis = candlePlot.Axes.YAxis;
            double span = yAxis.GetRange().Span;
            double dy = span * 0.015; // 1.5% span để đẩy nhãn lệch lên/xuống

            var mk = plt.Add.Marker(x, y);
            mk.Axes.XAxis = xAxis;
            mk.Axes.YAxis = yAxis;
            mk.MarkerStyle.Shape = shape;
            mk.MarkerStyle.Size = 8;
            mk.MarkerStyle.FillColor = color;
            mk.MarkerStyle.OutlineColor = color; // viền (tuỳ chọn)
            mk.MarkerStyle.OutlineWidth = 1;
            
            return mk;
        }

        public static IPlottable? Text(Plot plt, CandlestickPlot candlePlot, double x, double y, string label, ScottPlot.Color color, ScottPlot.Alignment alignment = Alignment.LowerCenter)
        {
            // Xác định offset theo tỉ lệ span của trục Y mà candlestick dùng (trái/phải)
            var xAxis = candlePlot.Axes.XAxis;
            var yAxis = candlePlot.Axes.YAxis;
            double span = yAxis.GetRange().Span;
            double dy = span * 0.015; // 1.5% span để đẩy nhãn lệch lên/xuống

            var txt = plt.Add.Text(label!, x, y);
            txt.Axes.XAxis = xAxis;
            txt.Axes.YAxis = yAxis;
            txt.Alignment = alignment;
            txt.LabelFontColor = color;
            txt.LabelBackgroundColor = ScottPlot.Colors.White.WithAlpha(.85);
            txt.LabelPixelPadding = new ScottPlot.PixelPadding(0, 8);
            
            return txt;
        }

        public static IPlottable? HLine(Plot plt, CandlestickPlot candlePlot, double y, ScottPlot.Color color, float width, string? label = null)
        {
            // Xác định offset theo tỉ lệ span của trục Y mà candlestick dùng (trái/phải)
            var xAxis = candlePlot.Axes.XAxis;
            var yAxis = candlePlot.Axes.YAxis;
            double span = yAxis.GetRange().Span;
            double dy = span * 0.015; // 1.5% span để đẩy nhãn lệch lên/xuống

            AxisLine hl = plt.Add.HorizontalLine(y);
            hl.Axes.XAxis = xAxis;
            hl.Axes.YAxis = yAxis;
            hl.LineStyle.Color = color;
            hl.LineStyle.Width = width;
            if (!string.IsNullOrEmpty(label))
                hl.LabelText = label;                        // xuất hiện trong Legend nếu Legend bật
            
            return hl;
        }

        public static IPlottable? VLine(Plot plt, CandlestickPlot candlePlot, double x, ScottPlot.Color color, float width, string? label = null)
        {
            // Xác định offset theo tỉ lệ span của trục Y mà candlestick dùng (trái/phải)
            var xAxis = candlePlot.Axes.XAxis;
            var yAxis = candlePlot.Axes.YAxis;
            double span = yAxis.GetRange().Span;
            double dy = span * 0.015; // 1.5% span để đẩy nhãn lệch lên/xuống

            AxisLine vl = plt.Add.VerticalLine(x);
            vl.Axes.XAxis = candlePlot.Axes.XAxis;
            vl.LineStyle.Color = color;
            vl.LineStyle.Width = width;
            if (!string.IsNullOrEmpty(label))
                vl.LabelText = label;                       // xuất hiện trong Legend nếu Legend bật

            return vl;
        }

        public static IPlottable? Line(Plot plt, CandlestickPlot candlePlot, double x1, double y1, double x2, double y2, ScottPlot.Color color, float width, string? label = null)
        {
            // Xác định offset theo tỉ lệ span của trục Y mà candlestick dùng (trái/phải)
            var xAxis = candlePlot.Axes.XAxis;
            var yAxis = candlePlot.Axes.YAxis;
            double span = yAxis.GetRange().Span;
            double dy = span * 0.015; // 1.5% span để đẩy nhãn lệch lên/xuống

            var ln = plt.Add.Line(x1, y1, x2, y2);
            ln.Axes.XAxis = xAxis;
            ln.Axes.YAxis = yAxis;
            ln.LineStyle.Color = color;
            ln.LineStyle.Width = width;
            //if (!string.IsNullOrEmpty(label)) 
                //ln.Label = label;

            return ln;
        }

        public static IPlottable? Arrow(Plot plt, CandlestickPlot candlePlot, double xBase, double yBase, double xTip, double yTip, ScottPlot.Color color)
        {
            // Xác định offset theo tỉ lệ span của trục Y mà candlestick dùng (trái/phải)
            var xAxis = candlePlot.Axes.XAxis;
            var yAxis = candlePlot.Axes.YAxis;
            double span = yAxis.GetRange().Span;
            double dy = span * 0.015; // 1.5% span để đẩy nhãn lệch lên/xuống

            var arr = plt.Add.Arrow(new ScottPlot.Coordinates(xBase, yBase),
                                    new ScottPlot.Coordinates(xTip, yTip));

            arr.Axes.XAxis = xAxis;
            arr.Axes.YAxis = yAxis;
            arr.ArrowLineColor = color;
            arr.ArrowWidth = 1.5f;

            // Kích thước đầu mũi tên (thuộc tính tồn tại ở 5.1.x)
            arr.ArrowheadLength = 10;
            arr.ArrowheadWidth = 7;

            return arr;
        }

        public static IPlottable? Rectangle(
        Plot plt,
        CandlestickPlot candlePlot,
        double x1, double y1, double x2, double y2,
        ScottPlot.Color edgeColor,
        float edgeWidth = 1.5f,
        ScottPlot.Color? fillColor = null,
        string? label = null)
        {
            // Xác định offset theo tỉ lệ span của trục Y mà candlestick dùng (trái/phải)
            var xAxis = candlePlot.Axes.XAxis;
            var yAxis = candlePlot.Axes.YAxis;
            double span = yAxis.GetRange().Span;
            double dy = span * 0.015; // 1.5% span để đẩy nhãn lệch lên/xuống

            double xl = Math.Min(x1, x2);
            double xr = Math.Max(x1, x2);
            double yb = Math.Min(y1, y2);
            double yt = Math.Max(y1, y2);

            var xs = new double[] { xl, xr, xr, xl };
            var ys = new double[] { yb, yb, yt, yt };

            var poly = plt.Add.Polygon(xs, ys);
            poly.Axes.XAxis = xAxis;
            poly.Axes.YAxis = yAxis;
            poly.LineStyle.Color = edgeColor;
            poly.LineStyle.Width = edgeWidth;
            poly.FillStyle.Color = fillColor ?? ScottPlot.Colors.Transparent;

            return poly;
        }

        public static IPlottable? Circle(
        Plot plt,
        CandlestickPlot candlePlot,
        double xc, double yc,
        double radiusX, double radiusY,
        ScottPlot.Color edgeColor,
        float edgeWidth = 1.5f,
        ScottPlot.Color? fillColor = null,
        Angle? rotation = null,
        string? label = null)
        {
            // Xác định offset theo tỉ lệ span của trục Y mà candlestick dùng (trái/phải)
            var xAxis = candlePlot.Axes.XAxis;
            var yAxis = candlePlot.Axes.YAxis;
            double span = yAxis.GetRange().Span;
            double dy = span * 0.015; // 1.5% span để đẩy nhãn lệch lên/xuống

            // ScottPlot 5.x: Ellipse nhận width/height (đường kính), không phải bán kính
            double width = Math.Abs(radiusX) * 2.0;
            double height = Math.Abs(radiusY) * 2.0;

            var el = plt.Add.Ellipse(xc, yc, width, height);
            el.Axes.XAxis = xAxis;
            el.Axes.YAxis = yAxis;
            el.LineStyle.Color = edgeColor;
            el.LineStyle.Width = edgeWidth;
            el.FillStyle.Color = fillColor ?? ScottPlot.Colors.Transparent;
            el.Rotation =  rotation ?? Angle.FromDegrees(0); // 0 = không xoay

            return el;
        }

        /// <summary>
        /// Vẽ hình vuông trục song song trục toạ độ, xác định bằng TÂM và cạnh.
        /// </summary>
        public static IPlottable? SquareByCenter(
            Plot plt,
            CandlestickPlot candlePlot,
            double xc, double yc,
            double side,
            ScottPlot.Color edgeColor,
            float edgeWidth = 1.5f,
            ScottPlot.Color? fillColor = null,
            string? label = null)
        {
            if (side <= 0) 
                return null;

            // Xác định offset theo tỉ lệ span của trục Y mà candlestick dùng (trái/phải)
            var xAxis = candlePlot.Axes.XAxis;
            var yAxis = candlePlot.Axes.YAxis;
            double span = yAxis.GetRange().Span;
            double dy = span * 0.015; // 1.5% span để đẩy nhãn lệch lên/xuống

            double half = side / 2.0;
            double x1 = xc - half, x2 = xc + half;
            double y1 = yc - half, y2 = yc + half;

            var xs = new double[] { x1, x2, x2, x1 };
            var ys = new double[] { y1, y1, y2, y2 };

            var poly = plt.Add.Polygon(xs, ys);
            poly.Axes.XAxis = xAxis;
            poly.Axes.YAxis = yAxis;
            poly.LineStyle.Color = edgeColor;
            poly.LineStyle.Width = edgeWidth;
            poly.FillStyle.Color = fillColor ?? ScottPlot.Colors.Transparent;

            return poly;
        }

        /// <summary>
        /// Vẽ hình vuông trục song song trục toạ độ, xác định bằng GÓC TRÁI-DƯỚI (x1,y1) và độ dài cạnh.
        /// </summary>
        public static IPlottable? Square(
            Plot plt,
            CandlestickPlot candlePlot,
            double x1, double y1,
            double side,
            ScottPlot.Color edgeColor,
            float edgeWidth = 1.5f,
            ScottPlot.Color? fillColor = null,
            string? label = null)
        {
            if (side <= 0) 
                return null;

            // Xác định offset theo tỉ lệ span của trục Y mà candlestick dùng (trái/phải)
            var xAxis = candlePlot.Axes.XAxis;
            var yAxis = candlePlot.Axes.YAxis;
            double span = yAxis.GetRange().Span;
            double dy = span * 0.015; // 1.5% span để đẩy nhãn lệch lên/xuống

            double x2 = x1 + side;
            double y2 = y1 + side;

            var xs = new double[] { x1, x2, x2, x1 };
            var ys = new double[] { y1, y1, y2, y2 };

            var poly = plt.Add.Polygon(xs, ys);
            poly.Axes.XAxis = xAxis;
            poly.Axes.YAxis = yAxis;
            poly.LineStyle.Color = edgeColor;
            poly.LineStyle.Width = edgeWidth;
            poly.FillStyle.Color = fillColor ?? ScottPlot.Colors.Transparent;

            return poly;
        }

        /// <summary>
        /// Vẽ Scatter (điểm +/− đường nối) từ mảng X/Y.
        /// </summary>
        public static IPlottable? Scatter(
            Plot plt,
            CandlestickPlot candlePlot,
            double[] xs,
            double[] ys,
            ScottPlot.Color color,
            float lineWidth = 1.5f,
            LinePattern? linePattern = null,
            bool showLine = true,
            bool showMarkers = true,
            MarkerShape markerShape = MarkerShape.FilledCircle,
            float markerSize = 4f,
            string? label = null)
        {
            if (xs == null || ys == null || xs.Length == 0 || ys.Length == 0 || xs.Length != ys.Length)
                return null;

            // Xác định offset theo tỉ lệ span của trục Y mà candlestick dùng (trái/phải)
            var xAxis = candlePlot.Axes.XAxis;
            var yAxis = candlePlot.Axes.YAxis;
            double span = yAxis.GetRange().Span;
            double dy = span * 0.015; // 1.5% span để đẩy nhãn lệch lên/xuống

            var sc = plt.Add.Scatter(xs, ys);
            sc.Axes.XAxis = xAxis;
            sc.Axes.YAxis = yAxis;

            // Line style
            sc.LineStyle.Color = color;
            sc.LineStyle.Width = showLine ? lineWidth : 0f;
            sc.LineStyle.Pattern = linePattern ?? LinePattern.Solid;

            // Marker style
            sc.MarkerStyle.Shape = markerShape;
            sc.MarkerStyle.Size = showMarkers ? markerSize : 0f;
            sc.MarkerStyle.FillColor = color;
            sc.MarkerStyle.OutlineColor = color;
            sc.MarkerStyle.OutlineWidth = showMarkers ? 1f : 0f;

            if (!string.IsNullOrEmpty(label))
                sc.LegendText = label;

            return sc;
        }

        /// <summary>
        /// Vẽ Scatter khi chỉ có mảng Y, X tạo bởi dãy đều: X = xStart + i * xStep.
        /// </summary>
        public static IPlottable? ScatterSeries(
            Plot plt,
            CandlestickPlot candlePlot,
            double[] ys,
            double xStart,
            double xStep,
            ScottPlot.Color color,
            float lineWidth = 1.5f,
            LinePattern? linePattern = null,
            bool showLine = true,
            bool showMarkers = false,
            MarkerShape markerShape = MarkerShape.FilledCircle,
            float markerSize = 3f,
            string? label = null)
        {
            if (ys == null || ys.Length == 0 || xStep == 0)
                return null;

            var xs = new double[ys.Length];
            for (int i = 0; i < ys.Length; i++)
                xs[i] = xStart + i * xStep;

            return Scatter(plt, candlePlot, xs, ys, color, lineWidth, linePattern, showLine, showMarkers, markerShape, markerSize, label);
        }

        /// <summary>
        /// Vẽ Callout: hộp chữ tại (xText,yText) và leader line từ anchor (xAnchor,yAnchor) tới hộp.
        /// </summary>
        public static IPlottable? CalloutAt(
            Plot plt,
            CandlestickPlot candlePlot,
            double xAnchor, double yAnchor,
            double xText, double yText,
            string label,
            ScottPlot.Color color,
            bool drawLeader = true,
            bool drawAnchorMarker = true,
            MarkerShape anchorMarkerShape = MarkerShape.FilledCircle,
            float anchorMarkerSize = 6f,
            ScottPlot.Color? fillColor = null,
            ScottPlot.Color? borderColor = null,
            float borderWidth = 1f,
            Alignment textAlignment = Alignment.UpperLeft)
        {
            // Xác định offset theo tỉ lệ span của trục Y mà candlestick dùng (trái/phải)
            var xAxis = candlePlot.Axes.XAxis;
            var yAxis = candlePlot.Axes.YAxis;
            double span = yAxis.GetRange().Span;
            double dy = span * 0.015; // 1.5% span để đẩy nhãn lệch lên/xuống

            // 1) Leader line (tuỳ chọn)
            if (drawLeader)
            {
                var ln = plt.Add.Line(xAnchor, yAnchor, xText, yText);
                ln.Axes.XAxis = xAxis;
                ln.Axes.YAxis = yAxis;
                ln.LineStyle.Color = color;
                ln.LineStyle.Width = 1.2f;
                ln.LineStyle.Pattern = LinePattern.Solid;
            }

            // 2) Marker tại anchor (tuỳ chọn)
            if (drawAnchorMarker)
            {
                var mk = plt.Add.Marker(xAnchor, yAnchor);
                mk.Axes.XAxis = xAxis;
                mk.Axes.YAxis = yAxis;
                mk.MarkerStyle.Shape = anchorMarkerShape;
                mk.MarkerStyle.Size = anchorMarkerSize;
                mk.MarkerStyle.FillColor = color;
                mk.MarkerStyle.OutlineColor = color;
                mk.MarkerStyle.OutlineWidth = 1f;
            }

            // 3) Text box (plottable chính)
            var txt = plt.Add.Text(label, xText, yText);
            txt.Axes.XAxis = xAxis;
            txt.Axes.YAxis = yAxis;
            txt.Alignment = textAlignment;

            // Thuộc tính hộp chữ: màu chữ, nền, viền, margin
            txt.LabelFontColor = color;
            txt.LabelBackgroundColor = (fillColor ?? ScottPlot.Colors.White.WithAlpha(220));
            txt.LabelBorderColor = borderColor ?? color;
            txt.LabelBorderWidth = borderWidth;
            txt.LabelPixelPadding = new ScottPlot.PixelPadding(4, 2);

            return txt;
        }

        /// <summary>
        /// Vẽ Callout: hộp chữ đặt theo offset (dx,dy) so với anchor.
        /// </summary>
        public static IPlottable? CalloutOffset(
            Plot plt,
            CandlestickPlot candlePlot,
            double xAnchor, double yAnchor,
            double dx, double dy,
            string label,
            ScottPlot.Color color,
            bool drawLeader = true,
            bool drawAnchorMarker = true,
            MarkerShape anchorMarkerShape = MarkerShape.FilledCircle,
            float anchorMarkerSize = 6f,
            ScottPlot.Color? fillColor = null,
            ScottPlot.Color? borderColor = null,
            float borderWidth = 1f,
            Alignment textAlignment = Alignment.UpperLeft)
        {
            double xText = xAnchor + dx;
            double yText = yAnchor + dy;

            return CalloutAt(
                plt,
                candlePlot,
                xAnchor, yAnchor,
                xText, yText,
                label,
                color,
                drawLeader,
                drawAnchorMarker,
                anchorMarkerShape,
                anchorMarkerSize,
                fillColor,
                borderColor,
                borderWidth,
                textAlignment);
        }

        public static IPlottable? Callout(Plot plt, CandlestickPlot candlePlot, double x, double y, double yLabel, string label, 
            ScottPlot.Color textColor, 
            ScottPlot.Color textBackgroundColor, 
            ScottPlot.Color arrowLineColor,
            ScottPlot.Color arrowFillColor,
            float arrowLineWidth = 1.0f,
            MarkerShape shape = MarkerShape.FilledCircle)
        {
            // Xác định offset theo tỉ lệ span của trục Y mà candlestick dùng (trái/phải)
            var xAxis = candlePlot.Axes.XAxis;
            var yAxis = candlePlot.Axes.YAxis;
            double span = yAxis.GetRange().Span;
            double dy = span * 0.015; // 1.5% span để đẩy nhãn lệch lên/xuống

            var callout = plt.Add.Callout(label!, new Coordinates(x, yLabel), new Coordinates(x, y));
            callout.Axes.XAxis = xAxis;
            callout.Axes.YAxis = yAxis;
            callout.TextColor = textColor;
            callout.TextBackgroundColor = textBackgroundColor;
            callout.TextBorderWidth = 0;
            callout.FontSize = 10;
            callout.ArrowLineColor = arrowLineColor;
            callout.ArrowFillColor = arrowFillColor;
            callout.ArrowLineWidth = arrowLineWidth;

            return callout;
        }

        public static IPlottable? Callout(Plot plot, IYAxis yAxis, string text,
            double textX, double textY, double tipX, double tipY, ScottPlot.Color color)
        {
            var call = plot.Add.Callout(text, new Coordinates(textX, textY), new Coordinates(tipX, tipY));
           
            call.Axes.YAxis = yAxis;
            call.TextColor = Colors.White;
            call.TextBackgroundColor = color.WithAlpha(0.9);
            call.TextBorderWidth = 0;
            call.FontSize = 10;
            call.ArrowLineColor = color;
            call.ArrowFillColor = color;
            call.ArrowLineWidth = 1.2f;

            return call;
        }

        /// <summary>
        /// Vẽ Polygon từ mảng X/Y.
        /// Lưu ý: Polygon trong ScottPlot 5.x tự đóng đường bao.
        /// </summary>
        public static IPlottable? Polygon(
            Plot plt,
            CandlestickPlot candlePlot,
            double[] xs,
            double[] ys,
            ScottPlot.Color edgeColor,
            float edgeWidth = 1.5f,
            ScottPlot.Color? fillColor = null,
            LinePattern? edgePattern = null,
            string? label = null)
        {
            if (xs == null || ys == null || xs.Length == 0 || ys.Length == 0 || xs.Length != ys.Length)
                return null;

            var xAxis = candlePlot.Axes.XAxis;
            var yAxis = candlePlot.Axes.YAxis;

            var poly = plt.Add.Polygon(xs, ys);

            // Khóa trục Y cùng với candlestick (đúng với style hiện tại của bạn)
            poly.Axes.XAxis = xAxis;
            poly.Axes.YAxis = yAxis;

            poly.LineStyle.Color = edgeColor;
            poly.LineStyle.Width = edgeWidth;
            poly.LineStyle.Pattern = edgePattern ?? LinePattern.Solid;

            poly.FillStyle.Color = fillColor ?? ScottPlot.Colors.Transparent;

            return poly;
        }

        /// <summary>
        /// Vẽ Polygon từ danh sách Coordinates.
        /// </summary>
        public static IPlottable? Polygon(
            Plot plt,
            CandlestickPlot candlePlot,
            IReadOnlyList<Coordinates> points,
            ScottPlot.Color edgeColor,
            float edgeWidth = 1.5f,
            ScottPlot.Color? fillColor = null,
            LinePattern? edgePattern = null,
            string? label = null)
        {
            if (points == null || points.Count == 0)
                return null;

            // Chuyển sang mảng X/Y
            var xs = new double[points.Count];
            var ys = new double[points.Count];
            for (int i = 0; i < points.Count; i++)
            {
                xs[i] = points[i].X;
                ys[i] = points[i].Y;
            }

            return Polygon(plt, candlePlot, xs, ys, edgeColor, edgeWidth, fillColor, edgePattern, label);
        }

        public static List<IPlottable>? ArrowLike(Plot plt, CandlestickPlot candlePlot, double x, double yFrom, double yTo, ScottPlot.Color color)
        {
            List<IPlottable> plottables = new List<IPlottable>();

            // Xác định offset theo tỉ lệ span của trục Y mà candlestick dùng (trái/phải)
            var xAxis = candlePlot.Axes.XAxis;
            var yAxis = candlePlot.Axes.YAxis;
            double span = yAxis.GetRange().Span;
            double dy = span * 0.015; // 1.5% span để đẩy nhãn lệch lên/xuống

            // line
            var sc = plt.Add.Scatter(new[] { x, x }, new[] { yFrom, yTo });
            sc.Axes.XAxis = xAxis;
            sc.Axes.YAxis = yAxis;
            sc.LineStyle.Color = color;
            sc.LineStyle.Width = 1.5f;
            sc.MarkerStyle.Size = 0;
            plottables.Add(sc);

            // tip
            var tip = plt.Add.Marker(x, yTo);
            tip.Axes.XAxis = xAxis;
            tip.Axes.YAxis = yAxis;
            tip.MarkerStyle.Shape = MarkerShape.OpenTriangleUp;
            tip.MarkerStyle.Size = 8;
            tip.MarkerStyle.FillColor = color;
            tip.MarkerStyle.OutlineColor = color;
            plottables.Add(tip);

            return plottables;
        }

        public static IPlottable? Annotation(Plot plt, string text, ScottPlot.Alignment alignment = Alignment.LowerLeft, float labelFontSize = 18f)
        {
            var anno = plt.Add.Annotation(text);
            anno.LabelFontSize = labelFontSize;
            anno.LabelFontName = Fonts.Serif;
            anno.LabelBackgroundColor = Colors.Yellow.WithAlpha(.3);
            anno.LabelFontColor = Colors.RebeccaPurple;
            anno.LabelBorderColor = Colors.Green;
            anno.LabelBorderWidth = 1;
            anno.LabelShadowColor = Colors.Transparent;
            //anno.OffsetY = 40;
            //anno.OffsetX = 20;
            anno.Alignment = alignment;

            return anno;
        }
    }
}
