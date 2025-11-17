using TradingApp.WinUI.Docking;

namespace TradingApp.WinUI.Factories
{
    public interface IChartDocumentFactory
    {
        ChartDocument Create(string symbol, string timeframe);
    }
}
