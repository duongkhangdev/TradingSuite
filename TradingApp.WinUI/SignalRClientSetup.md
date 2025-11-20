# SignalR Client Integration (WinUI)

## Dock "Realtime" trong ứng dụng

- Mở `View > Realtime` để hiển thị DockPanel bên trái.
- Chọn transport `SignalR` hoặc `WebSocket`, chỉnh URL hub/endpoint nếu cần.
- Nhấn **Start** để kết nối mềm, **Stop** để ngắt. Nhãn trạng thái sẽ hiển thị `Connected / Connecting / Faulted` cùng loại transport hiện tại.
- Dock sử dụng `IRealtimeConnectionService` nên mọi nơi khác trong UI có thể nghe trạng thái chung.

## Sử dụng dịch vụ `IRealtimeConnectionService`

Gói `Microsoft.AspNetCore.SignalR.Client` đã được tham chiếu trong `TradingApp.WinUI.csproj`.

```csharp
public sealed class StreamingConsumer
{
    private readonly IRealtimeConnectionService _connection;

    public StreamingConsumer(IRealtimeConnectionService connection)
    {
        _connection = connection;
        _connection.StatusChanged += (_, args) =>
        {
            // cập nhật UI/view-model tùy ý
            Debug.WriteLine($"{args.Transport}: {args.Status} - {args.Message}");
        };

        _connection.PriceReceived += (_, price) =>
        {
            Debug.WriteLine($"Price {price.Symbol} {price.Bid}/{price.Ask}");
        };

        _connection.OrderReceived += (_, order) =>
        {
            Debug.WriteLine($"Order {order.OrderId} {order.Status}");
        };
    }

    public Task StartSignalRAsync(string hubUrl)
        => _connection.StartSignalRAsync(hubUrl);

    public Task StopAsync()
        => _connection.StopAsync();
}
```

Nếu bạn cần đăng ký handler nhận data (`ReceivePrice`, ...), hãy inject `HubConnection` riêng hoặc mở rộng `RealtimeConnectionService` theo nhu cầu để chuyển sự kiện tới các view-model.

Hiện tại `MainForm` đã lắng nghe các sự kiện `PriceReceived`, `QuoteReceived`, `OrderReceived`, `PositionReceived`, `AccountReceived` và phản chiếu trực tiếp sang các Dock (Watchlist/Orders/Positions/Accounts). Bạn chỉ cần đảm bảo service được Start qua Dock “Realtime”.

## Đẩy dữ liệu từ WebApi

Sử dụng các endpoint `/api/trading-events/{price|order|position|quote|account}` hoặc gọi trực tiếp `ITradingBroadcaster` trong backend để phát tín hiệu cho WinUI.
