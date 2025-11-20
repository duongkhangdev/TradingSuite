using System;

namespace TradingApp.WinUI.Docking
{
    /// <summary>
    /// Gói thông tin selection chung giữa các dock:
    /// - AccountLogin: tài khoản đang được chọn (ví dụ double-click account)
    /// - Symbol: symbol được chọn (ví dụ double-click symbol ở Watchlist)
    /// Các property đều optional, tùy ngữ cảnh.
    /// </summary>
    public sealed class UiSelectionEvent
    {
        public string? AccountLogin { get; init; }
        public string? Symbol { get; init; }
    }

    /// <summary>
    /// Event bus đơn giản dùng static event:
    /// - Dock hoặc form nào muốn broadcast selection => gọi Publish(...)
    /// - Dock nào muốn phản ứng selection => subscribe Changed.
    /// </summary>
    public static class UiSelectionBus
    {
        public static event Action<UiSelectionEvent>? Changed;

        public static void Publish(UiSelectionEvent evt)
        {
            if (evt == null) throw new ArgumentNullException(nameof(evt));
            Changed?.Invoke(evt);
        }
    }
}

