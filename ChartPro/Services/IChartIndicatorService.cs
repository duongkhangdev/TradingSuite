using Cuckoo.Shared;
using ScottPlot.Plottables;
using ScottPlot.WinForms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartPro
{
    public interface IChartIndicatorService
    {
        // ===================== NEW: Events để báo cho form biết =====================
        /// <summary>
        /// Gọi khi service đã thêm (hoặc cập nhật) plottables của 1 indicator lên chart.
        /// form có thể Subscribe để log hoặc refresh UI phụ.
        ///
        /// string  -> symbol
        /// string  -> time_frame
        /// List<PlottableModel>? -> danh sách plottables vừa add/update (có thể null)
        /// </summary>
        public event Action<string, string, List<PlottableModel>?>? PlottablesAdded;

        /// <summary>
        /// Gọi khi service vừa xóa 1 indicator cụ thể ra khỏi chart.
        ///
        /// string -> symbol
        /// string -> time_frame
        /// string -> unique_key (ví dụ "ICHIMOKU", "EMA21")
        /// </summary>
        public event Action<string, string, string, List<PlottableModel>?>? PlottablesRemoved;

        void AddOrUpdate(string symbol, string time_frame, string unique_key, bool state);

        bool Remove(string symbol, string time_frame, string unique_key);

        Task HandleSingleNodeIndicatorToggle(FormsPlot formsPlot, CandlestickPlot candlestickPlot, List<AppQuote>? AppQuotes, string symbol, string time_frame, Dictionary<string, object>? indicators, TreeNode node);

        Task SaveIndicatorStateToDisk(string symbol, string time_frame);
        Task<List<IndicatorStateFileModel>?> LoadIndicatorStateFromDisk(string symbol, string time_frame);
        Task ApplySavedStateToTreeView(
            TreeView tv,
            string symbol,
            string time_frame,
            bool suppressEvents,
            Func<TreeNode, Task>? onReapplyCheckedNodeAsync = null);

    }
}
