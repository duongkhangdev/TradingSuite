using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Serilog;
using TradingApp.WinUI.Docking;
using TradingApp.WinUI.Models;
using WeifenLuo.WinFormsUI.Docking;
using TradingApp.WinUI.Factories;

namespace TradingApp.WinUI
{
    public class MainForm : Form
    {
        private readonly DockPanel _dockPanel;
        private readonly IChartDocumentFactory _chartFactory;

        private WatchlistDock? _watchlistDock;
        private PositionsDock? _positionsDock;
        private OrdersDock? _ordersDock;
        private HistoryDock? _historyDock;
        private SignalsDock? _signalsDock;
        private AccountsDock? _accountsDock;
        private LogDock? _logDock;

        private readonly List<ChartDocument> _openCharts = new();

        private string LayoutFilePath =>
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "layout_trading.xml");

        public MainForm() : this(new ChartDocumentFactory(new ChartPro.Services.DemoChartDataService(), new Microsoft.Extensions.Logging.Abstractions.NullLogger<ChartDocument>(), new ChartPro.Services.ChartService()))
        {
        }

        public MainForm(IChartDocumentFactory chartFactory)
        {
            _chartFactory = chartFactory;

            InitializeComponent();

            Text = "Trading Terminal";
            WindowState = FormWindowState.Maximized;

            _dockPanel = new DockPanel
            {
                Dock = DockStyle.Fill,
                DocumentStyle = DocumentStyle.DockingMdi
            };
            _dockPanel.Theme = new VS2015BlueTheme();

            Controls.Add(_dockPanel);

            var menu = new MenuStrip();
            var viewMenu = new ToolStripMenuItem("View");
            var miWatchlist = new ToolStripMenuItem("Watchlist", null,
                (s, e) => ShowWatchlist());
            var miPositions = new ToolStripMenuItem("Positions", null,
                (s, e) => ShowPositions());
            var miOrders = new ToolStripMenuItem("Orders", null,
                (s, e) => ShowOrders());
            var miHistory = new ToolStripMenuItem("History", null,
                (s, e) => ShowHistory());
            var miAccounts = new ToolStripMenuItem("Accounts", null,
                (s, e) => ShowAccounts());
            var miSignals = new ToolStripMenuItem("Signals", null,
                (s, e) => ShowSignals());
            var miLog = new ToolStripMenuItem("Log", null,
                (s, e) => ShowLog());

            viewMenu.DropDownItems.Add(miWatchlist);
            viewMenu.DropDownItems.Add(miPositions);
            viewMenu.DropDownItems.Add(miOrders);
            viewMenu.DropDownItems.Add(miHistory);
            viewMenu.DropDownItems.Add(miAccounts);
            viewMenu.DropDownItems.Add(new ToolStripSeparator());
            viewMenu.DropDownItems.Add(miSignals);
            viewMenu.DropDownItems.Add(new ToolStripSeparator());
            viewMenu.DropDownItems.Add(miLog);

            menu.Items.Add(viewMenu);
            MainMenuStrip = menu;
            Controls.Add(menu);
            //menu.BringToFront();

            Load += MainForm_Load;
            FormClosing += MainForm_FormClosing;
        }

        private void MainForm_Load(object? sender, EventArgs e)
        {
            if (File.Exists(LayoutFilePath))
            {
                try
                {
                    //_dockPanel.LoadFromXml(LayoutFilePath, DeserializeContent);
                    CreateDefaultLayout();
                }
                catch
                {
                    CreateDefaultLayout();
                }
            }
            else
            {
                CreateDefaultLayout();
            }

            LoadDemoData();
        }

        private void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            _dockPanel.SaveAsXml(LayoutFilePath);
        }

        private IDockContent? DeserializeContent(string persistString)
        {
            if (string.IsNullOrWhiteSpace(persistString))
                return null;

            var parts = persistString.Split(';');

            switch (parts[0])
            {
                case nameof(WatchlistDock):
                    return ShowWatchlist(false);
                case nameof(PositionsDock):
                    return ShowPositions(false);
                case nameof(OrdersDock):
                    return ShowOrders(false);
                case nameof(HistoryDock):
                    return ShowHistory(false);
                case nameof(SignalsDock):
                    return ShowSignals(false);
                case nameof(AccountsDock):
                    return ShowAccounts(false);
                case nameof(LogDock):
                    return ShowLog(false);
                case nameof(ChartDocument):
                    if (parts.Length >= 3)
                    {
                        string symbol = parts[1];
                        string timeframe = parts[2];
                        return OpenChart(symbol, timeframe, false);
                    }
                    return OpenChart("XAUUSD", "M15", false);
            }

            return null;
        }

        private void CreateDefaultLayout()
        {
            _ = OpenChart("XAUUSD", "M15", true);

            var watch = ShowWatchlist(true);
            watch?.Show(_dockPanel, DockState.DockLeftAutoHide);

            var accounts = ShowAccounts(true);
            if (watch != null)
                accounts?.Show(watch.Pane, null);
            else
                accounts?.Show(_dockPanel, DockState.DockLeft);

            var pos = ShowPositions(true);
            pos?.Show(_dockPanel, DockState.DockBottom);

            var orders = ShowOrders(true);
            var history = ShowHistory(true);

            if (pos != null)
            {
                orders?.Show(pos.Pane, null);
                history?.Show(pos.Pane, null);
            }

            var log = ShowLog(true);
            log?.Show(_dockPanel, DockState.DockBottomAutoHide);

            var sig = ShowSignals(true);
            sig?.Show(_dockPanel, DockState.DockRightAutoHide);
        }

        #region Show helpers

        private WatchlistDock ShowWatchlist(bool createIfNull = true)
        {
            if (_watchlistDock == null && createIfNull)
            {
                _watchlistDock = new WatchlistDock();
                _watchlistDock.SymbolDoubleClicked += Watchlist_SymbolDoubleClicked;
                _watchlistDock.Show(_dockPanel, DockState.DockLeft);
            }

            _watchlistDock?.Activate();
            return _watchlistDock!;
        }

        private PositionsDock ShowPositions(bool createIfNull = true)
        {
            if (_positionsDock == null && createIfNull)
            {
                _positionsDock = new PositionsDock();
                _positionsDock.PositionDoubleClicked += Positions_PositionDoubleClicked;
                _positionsDock.Show(_dockPanel, DockState.DockBottom);
            }

            _positionsDock?.Activate();
            return _positionsDock!;
        }

        private OrdersDock ShowOrders(bool createIfNull = true)
        {
            if (_ordersDock == null && createIfNull)
            {
                _ordersDock = new OrdersDock();
                _ordersDock.OrderDoubleClicked += Orders_OrderDoubleClicked;
                _ordersDock.Show(_dockPanel, DockState.DockBottom);
            }

            _ordersDock?.Activate();
            return _ordersDock!;
        }

        private HistoryDock ShowHistory(bool createIfNull = true)
        {
            if (_historyDock == null && createIfNull)
            {
                _historyDock = new HistoryDock();
                _historyDock.TradeDoubleClicked += History_TradeDoubleClicked;
                _historyDock.Show(_dockPanel, DockState.DockBottom);
            }

            _historyDock?.Activate();
            return _historyDock!;
        }

        private SignalsDock ShowSignals(bool createIfNull = true)
        {
            if (_signalsDock == null && createIfNull)
            {
                _signalsDock = new SignalsDock();
                _signalsDock.SignalDoubleClicked += Signals_SignalDoubleClicked;
                _signalsDock.Show(_dockPanel, DockState.DockRight);
            }

            _signalsDock?.Activate();
            return _signalsDock!;
        }

        private AccountsDock ShowAccounts(bool createIfNull = true)
        {
            if (_accountsDock == null && createIfNull)
            {
                _accountsDock = new AccountsDock();
                _accountsDock.AccountDoubleClicked += Accounts_AccountDoubleClicked;
                _accountsDock.Show(_dockPanel, DockState.DockLeft);
            }

            _accountsDock?.Activate();
            return _accountsDock!;
        }

        private LogDock ShowLog(bool createIfNull = true)
        {
            if (_logDock == null && createIfNull)
            {
                _logDock = new LogDock();
                _logDock.Show(_dockPanel, DockState.DockBottomAutoHide);
            }

            _logDock?.Activate();
            return _logDock!;
        }

        #endregion

        #region Dock events

        private void Watchlist_SymbolDoubleClicked(SymbolViewModel symbolVm)
        {
            OpenChart(symbolVm.Symbol, "M15", true);
        }

        private void Positions_PositionDoubleClicked(PositionViewModel posVm)
        {
            OpenChart(posVm.Symbol, "M15", true);
        }

        private void Orders_OrderDoubleClicked(OrderViewModel orderVm)
        {
            OpenChart(orderVm.Symbol, "M15", true);
        }

        private void History_TradeDoubleClicked(HistoryTradeViewModel tradeVm)
        {
            OpenChart(tradeVm.Symbol, "M15", true);
        }

        private void Signals_SignalDoubleClicked(SignalViewModel sigVm)
        {
            OpenChart(sigVm.Symbol, sigVm.Timeframe, true);
        }

        private void Accounts_AccountDoubleClicked(AccountViewModel accVm)
        {
            Log.Information("Switch account to {AccountId} ({Broker})",
                accVm.AccountId, accVm.Broker);
        }

        #endregion

        private ChartDocument OpenChart(string symbol, string timeframe, bool show)
        {
            var existing = _openCharts.Find(c =>
                c.Symbol.Equals(symbol, StringComparison.OrdinalIgnoreCase) &&
                c.Timeframe.Equals(timeframe, StringComparison.OrdinalIgnoreCase));

            if (existing != null)
            {
                if (show)
                    existing.Show(_dockPanel);
                return existing;
            }

            var doc = _chartFactory.Create(symbol, timeframe);

            doc.Show(_dockPanel, DockState.Document);
            _openCharts.Add(doc);
            return doc;
        }

        private void InitializeComponent()
        {
            SuspendLayout();
            // 
            // MainForm
            // 
            ClientSize = new Size(882, 521);
            IsMdiContainer = true;
            Name = "MainForm";
            ResumeLayout(false);

        }

        #region Demo data

        private void LoadDemoData()
        {
            var accounts = new List<AccountViewModel>
            {
                new() { Broker = "Exness", AccountId = "12345678",
                        Name = "Main Real", Type = "Real", Currency = "USD",
                        Leverage = "1:2000", Balance = 10000, Equity = 9820,
                        FreeMargin = 7500, MarginLevel = 180, IsCurrent = true,
                        Description = "Main real account" },

                new() { Broker = "ICMarkets", AccountId = "87654321",
                        Name = "Scalping Demo", Type = "Demo", Currency = "USD",
                        Leverage = "1:500", Balance = 5000, Equity = 5000,
                        FreeMargin = 5000, MarginLevel = 999, IsCurrent = false,
                        Description = "Demo for testing strategies" }
            };

            _accountsDock?.SetItems(accounts);

            var symbols = new List<SymbolViewModel>
            {
                new() { Symbol = "XAUUSD", DisplayName = "Gold", Bid = 2320.5, Ask = 2320.7,
                        Spread = 0.2, ChangePercent = 0.45, ATR = 18.3, Session = "London",
                        Description = "H4 uptrend, M15 OB + FVG" },
                new() { Symbol = "EURUSD", DisplayName = "Euro / USD", Bid = 1.0845, Ask = 1.0847,
                        Spread = 0.2, ChangePercent = -0.12, ATR = 0.009, Session = "NY",
                        Description = "Range, wait BOS" },
                new() { Symbol = "US30", DisplayName = "Dow Jones", Bid = 39500, Ask = 39502,
                        Spread = 2, ChangePercent = 0.25, ATR = 180, Session = "NY",
                        Description = "Pullback to premium" }
            };

            _watchlistDock?.SetItems(symbols);

            var positions = new List<PositionViewModel>
            {
                new() { Symbol = "XAUUSD", Side = "Buy", Lots = 0.5,
                        EntryPrice = 2300, SL = 2290, TP = 2330, CurrentPrice = 2320.7,
                        Pnl = 1030, PnlPercent = 2.0, Comment = "ICT OB long",
                        OpenTime = DateTime.UtcNow.AddHours(-3) },

                new() { Symbol = "EURUSD", Side = "Sell", Lots = 1,
                        EntryPrice = 1.0900, SL = 1.0930, TP = 1.0820, CurrentPrice = 1.0845,
                        Pnl = 550, PnlPercent = 1.1, Comment = "Divergence short",
                        OpenTime = DateTime.UtcNow.AddHours(-5) },
            };

            _positionsDock?.SetItems(positions);

            var orders = new List<OrderViewModel>
            {
                new() { Symbol = "XAUUSD", Side = "Buy", Type = "BuyLimit",
                        Lots = 0.3, Price = 2310, SL = 2300, TP = 2340,
                        CreatedTime = DateTime.UtcNow.AddMinutes(-20),
                        ExpireTime = DateTime.UtcNow.AddHours(4),
                        Comment = "Limit at OB" },

                new() { Symbol = "EURUSD", Side = "Sell", Type = "SellStop",
                        Lots = 0.5, Price = 1.0830, SL = 1.0860, TP = 1.0760,
                        CreatedTime = DateTime.UtcNow.AddMinutes(-5),
                        ExpireTime = null,
                        Comment = "Breakout short" }
            };

            _ordersDock?.SetItems(orders);

            var history = new List<HistoryTradeViewModel>
            {
                new() { Symbol = "XAUUSD", Side = "Buy", Lots = 0.5,
                        EntryPrice = 2280, ExitPrice = 2310, SL = 2270, TP = 2320,
                        Pnl = 1500, PnlPercent = 3.0,
                        OpenTime = DateTime.UtcNow.AddDays(-1).AddHours(-2),
                        CloseTime = DateTime.UtcNow.AddDays(-1),
                        Strategy = "ICT OB + FVG",
                        Comment = "Nice killzone long" },

                new() { Symbol = "US30", Side = "Sell", Lots = 0.2,
                        EntryPrice = 39400, ExitPrice = 39250, SL = 39550, TP = 39100,
                        Pnl = 300, PnlPercent = 0.6,
                        OpenTime = DateTime.UtcNow.AddDays(-2).AddHours(-3),
                        CloseTime = DateTime.UtcNow.AddDays(-2).AddHours(-1),
                        Strategy = "Breaker + Liquidity sweep",
                        Comment = "Short at premium" }
            };

            _historyDock?.SetItems(history);

            var signals = new List<SignalViewModel>
            {
                new() { Time = DateTime.UtcNow.AddMinutes(-3),
                        Symbol = "XAUUSD", Timeframe = "M15", Price = 2320.5,
                        Title = "BOS + Order Block (Premium)",
                        Detail = "BOS up, OB at 2310-2312, look for long",
                        Source = "ICT Scanner", IsNew = true },

                new() { Time = DateTime.UtcNow.AddMinutes(-10),
                        Symbol = "EURUSD", Timeframe = "M5", Price = 1.0840,
                        Title = "RSI Divergence",
                        Detail = "Bearish divergence M5 at HTF resistance",
                        Source = "RSI Divergence", IsNew = false }
            };

            _signalsDock?.SetItems(signals);

            Log.Information("Demo data loaded");
        }

        #endregion
    }
}
