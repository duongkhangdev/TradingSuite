using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using TradingApp.WinUI.Services;
using WeifenLuo.WinFormsUI.Docking;

namespace TradingApp.WinUI.Docking;

public sealed class ConnectionsDock : DockContent
{
    private const string DefaultSignalRUrl = "https://localhost:5001/hubs/trading";
    private const string DefaultWebSocketUrl = "wss://localhost:5001/api/websocket/connect";

    private readonly IRealtimeConnectionService _connectionService;

    private readonly ComboBox _transportCombo;
    private readonly TextBox _signalRUrlText;
    private readonly TextBox _webSocketUrlText;
    private readonly Button _startButton;
    private readonly Button _stopButton;
    private readonly Label _statusLabel;

    private bool _disposed;

    public ConnectionsDock(IRealtimeConnectionService connectionService)
    {
        _connectionService = connectionService;

        Text = "Kết nối Realtime";
        TabText = "Realtime";

        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 5,
            Padding = new Padding(10),
            AutoSize = true,
        };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        _transportCombo = new ComboBox
        {
            Dock = DockStyle.Fill,
            DropDownStyle = ComboBoxStyle.DropDownList,
        };
        _transportCombo.Items.Add(new TransportItem(ConnectionTransport.SignalR, "SignalR"));
        _transportCombo.Items.Add(new TransportItem(ConnectionTransport.WebSocket, "WebSocket"));
        _transportCombo.SelectedIndex = 0;

        _signalRUrlText = new TextBox
        {
            Dock = DockStyle.Fill,
            Text = DefaultSignalRUrl
        };

        _webSocketUrlText = new TextBox
        {
            Dock = DockStyle.Fill,
            Text = DefaultWebSocketUrl
        };

        _startButton = new Button
        {
            Text = "Start",
            Dock = DockStyle.Fill,
            Height = 30
        };
        _startButton.Click += StartButton_Click;

        _stopButton = new Button
        {
            Text = "Stop",
            Dock = DockStyle.Fill,
            Height = 30,
            Enabled = false
        };
        _stopButton.Click += StopButton_Click;

        _statusLabel = new Label
        {
            Dock = DockStyle.Fill,
            Text = "Trạng thái: Disconnected",
            AutoEllipsis = true,
            ForeColor = Color.DarkSlateGray
        };

        panel.Controls.Add(new Label { Text = "Transport", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 0);
        panel.Controls.Add(_transportCombo, 1, 0);

        panel.Controls.Add(new Label { Text = "SignalR Hub", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 1);
        panel.Controls.Add(_signalRUrlText, 1, 1);

        panel.Controls.Add(new Label { Text = "WebSocket", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 2);
        panel.Controls.Add(_webSocketUrlText, 1, 2);

        panel.Controls.Add(_startButton, 0, 3);
        panel.SetColumnSpan(_startButton, 1);
        panel.Controls.Add(_stopButton, 1, 3);

        panel.Controls.Add(_statusLabel, 0, 4);
        panel.SetColumnSpan(_statusLabel, 2);

        Controls.Add(panel);

        _connectionService.StatusChanged += ConnectionService_StatusChanged;
    }

    private async void StartButton_Click(object? sender, EventArgs e)
    {
        ToggleButtons(false);
        try
        {
            var selected = (TransportItem)_transportCombo.SelectedItem!;

            if (selected.Transport == ConnectionTransport.SignalR)
            {
                await _connectionService.StartSignalRAsync(_signalRUrlText.Text.Trim());
            }
            else
            {
                await _connectionService.StartWebSocketAsync(_webSocketUrlText.Text.Trim());
            }
        }
        catch (Exception ex)
        {
            UpdateStatusLabel(ConnectionStatus.Faulted, ex.Message, _connectionService.Transport);
            MessageBox.Show(this, ex.Message, "Không thể khởi động kết nối", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            ToggleButtons(true);
        }
    }

    private async void StopButton_Click(object? sender, EventArgs e)
    {
        ToggleButtons(false);
        try
        {
            await _connectionService.StopAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Không thể dừng kết nối", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        finally
        {
            ToggleButtons(true);
        }
    }

    private void ToggleButtons(bool enabled)
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action<bool>(ToggleButtons), enabled);
            return;
        }

        _startButton.Enabled = enabled;
        _stopButton.Enabled = enabled;
    }

    private void ConnectionService_StatusChanged(object? sender, ConnectionStatusChangedEventArgs e)
    {
        UpdateStatusLabel(e.Status, e.Message, e.Transport);

        if (InvokeRequired)
        {
            BeginInvoke(new Action(() =>
            {
                _stopButton.Enabled = e.Status is ConnectionStatus.Connected or ConnectionStatus.Connecting;
                _startButton.Enabled = e.Status is ConnectionStatus.Disconnected or ConnectionStatus.Faulted;
            }));
            return;
        }

        _stopButton.Enabled = e.Status is ConnectionStatus.Connected or ConnectionStatus.Connecting;
        _startButton.Enabled = e.Status is ConnectionStatus.Disconnected or ConnectionStatus.Faulted;
    }

    private void UpdateStatusLabel(ConnectionStatus status, string message, ConnectionTransport? transport)
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action(() => UpdateStatusLabel(status, message, transport)));
            return;
        }

        var transportText = transport?.ToString() ?? "N/A";
        _statusLabel.Text = $"Trạng thái: {status} ({transportText}) - {message}";
        _statusLabel.ForeColor = status switch
        {
            ConnectionStatus.Connected => Color.ForestGreen,
            ConnectionStatus.Faulted => Color.Firebrick,
            ConnectionStatus.Connecting => Color.DarkOrange,
            _ => Color.DarkSlateGray
        };
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        base.OnFormClosed(e);
        if (_disposed)
            return;

        _disposed = true;
        _connectionService.StatusChanged -= ConnectionService_StatusChanged;
    }

    protected override string GetPersistString()
    {
        return nameof(ConnectionsDock);
    }

    private sealed record TransportItem(ConnectionTransport Transport, string Display)
    {
        public override string ToString() => Display;
    }
}
