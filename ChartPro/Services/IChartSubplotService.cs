using ScottPlot;

namespace ChartPro.Services
{
    public interface IChartSubplotService
    {
        void PlotRsi(Plot plt, double[] times, double[] rsi);
        void PlotMacd(Plot plt, double[] times, double[] macd);
        void PlotCci(Plot plt, double[] times, double[] cci);
        void PlotStochRsi(Plot plt, double[] times, double[] stochRsi);
    }
}
