using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Fluent;
using TradingApp.WinUI.Models;

namespace TradingApp.WinUI.Docking
{
    public class PositionsDock : FluentDockBase<PositionViewModel>
    {
        public event Action<PositionViewModel>? PositionDoubleClicked;

        private bool _innerListHooked;

        public PositionsDock()
        {
            Text = "Positions";
            TabText = "Positions";
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

            View.Properties.Name = nameof(PositionViewModel.Symbol);
            View.Properties.Icon = nameof(PositionViewModel.SideIcon);
            View.Properties.Description = nameof(PositionViewModel.Comment);

            View.ShowColumns = true;
            View.Properties.Columns = new List<string>
            {
                nameof(PositionViewModel.Side),
                nameof(PositionViewModel.Lots),
                nameof(PositionViewModel.EntryPrice),
                nameof(PositionViewModel.SL),
                nameof(PositionViewModel.TP),
                nameof(PositionViewModel.CurrentPrice),
                nameof(PositionViewModel.Pnl),
                nameof(PositionViewModel.PnlPercent),
                nameof(PositionViewModel.OpenTime)
            };

            View.Properties.ColumnNames = new List<string>
            {
                "Side", "Lots", "Entry", "SL", "TP",
                "Current", "PnL", "PnL %", "Open Time"
            };
        }

        private void InnerList_DoubleClick(object? sender, EventArgs e)
        {
            var selected = View.SelectedItem as PositionViewModel;
            if (selected != null)
                PositionDoubleClicked?.Invoke(selected);
        }

        protected override string GetPersistString()
        {
            return nameof(PositionsDock);
        }
    }
}
