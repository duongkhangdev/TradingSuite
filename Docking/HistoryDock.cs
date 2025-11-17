using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Fluent;
using TradingApp.WinUI.Models;

namespace TradingApp.WinUI.Docking
{
    public class HistoryDock : FluentDockBase<HistoryTradeViewModel>
    {
        public event Action<HistoryTradeViewModel>? TradeDoubleClicked;

        private bool _innerListHooked;

        public HistoryDock()
        {
            Text = "History";
            TabText = "History";
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            TryAttachInnerListDoubleClick();

            View.HandleCreated += View_HandleCreated;
            View.ControlAdded += View_ControlAdded;
        }

        private void View_HandleCreated(object? sender, EventArgs e)
        {
            TryAttachInnerListDoubleClick();
        }

        private void View_ControlAdded(object? sender, ControlEventArgs e)
        {
            TryAttachInnerListDoubleClick();
        }

        private void TryAttachInnerListDoubleClick()
        {
            if (_innerListHooked)
                return;

            var inner = View.InnerList;
            if (inner != null)
            {
                inner.DoubleClick += InnerList_DoubleClick;
                _innerListHooked = true;
            }
        }

        protected override void ConfigureView()
        {
            View.Theme = OLVTheme.VistaExplorer;
            View.ItemFont = new Font("Segoe UI", 9.5f);

            View.Properties.Name = nameof(HistoryTradeViewModel.Symbol);
            View.Properties.Description = nameof(HistoryTradeViewModel.Comment);

            View.ShowColumns = true;
            View.Properties.Columns = new List<string>
            {
                nameof(HistoryTradeViewModel.Side),
                nameof(HistoryTradeViewModel.Lots),
                nameof(HistoryTradeViewModel.EntryPrice),
                nameof(HistoryTradeViewModel.ExitPrice),
                nameof(HistoryTradeViewModel.SL),
                nameof(HistoryTradeViewModel.TP),
                nameof(HistoryTradeViewModel.Pnl),
                nameof(HistoryTradeViewModel.PnlPercent),
                nameof(HistoryTradeViewModel.OpenTime),
                nameof(HistoryTradeViewModel.CloseTime),
                nameof(HistoryTradeViewModel.Strategy)
            };

            View.Properties.ColumnNames = new List<string>
            {
                "Side", "Lots", "Entry", "Exit",
                "SL", "TP", "PnL", "PnL %",
                "Open", "Close", "Strategy"
            };
        }

        private void InnerList_DoubleClick(object? sender, EventArgs e)
        {
            var selected = View.SelectedItem as HistoryTradeViewModel;
            if (selected != null)
                TradeDoubleClicked?.Invoke(selected);
        }

        protected override string GetPersistString()
        {
            return nameof(HistoryDock);
        }
    }
}
