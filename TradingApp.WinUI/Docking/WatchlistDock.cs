using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Fluent;
using TradingApp.WinUI.Models;

namespace TradingApp.WinUI.Docking
{
    public class WatchlistDock : FluentDockBase<SymbolViewModel>
    {
        public event Action<SymbolViewModel>? SymbolDoubleClicked;

        private bool _innerListHooked;

        public WatchlistDock()
        {
            Text = "Watchlist";
            TabText = "Watchlist";
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Try immediately, then react if InnerList appears later.
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

            View.Properties.Name = nameof(SymbolViewModel.Symbol);
            View.Properties.Icon = nameof(SymbolViewModel.Icon);
            View.Properties.Description = nameof(SymbolViewModel.Description);

            View.ShowColumns = true;
            View.Properties.Columns = new List<string>
            {
                nameof(SymbolViewModel.Bid),
                nameof(SymbolViewModel.Ask),
                nameof(SymbolViewModel.Spread),
                nameof(SymbolViewModel.ChangePercent),
                nameof(SymbolViewModel.ATR),
                nameof(SymbolViewModel.Session)
            };

            View.Properties.ColumnNames = new List<string>
            {
                "Bid", "Ask", "Spread", "Change %", "ATR", "Session"
            };

            View.EnableDragDropItems = true;
            View.EnableDrop = true;
        }

        private void InnerList_DoubleClick(object? sender, EventArgs e)
        {
            var selected = View.SelectedItem as SymbolViewModel;
            if (selected != null)
                SymbolDoubleClicked?.Invoke(selected);
        }

        protected override string GetPersistString()
        {
            return nameof(WatchlistDock);
        }
    }
}
