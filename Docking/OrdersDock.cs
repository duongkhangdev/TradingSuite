using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Fluent;
using TradingApp.WinUI.Models;

namespace TradingApp.WinUI.Docking
{
    public class OrdersDock : FluentDockBase<OrderViewModel>
    {
        public event Action<OrderViewModel>? OrderDoubleClicked;

        private bool _innerListHooked;

        public OrdersDock()
        {
            Text = "Orders";
            TabText = "Orders";
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

            View.Properties.Name = nameof(OrderViewModel.Symbol);
            View.Properties.Icon = nameof(OrderViewModel.SideIcon);
            View.Properties.Description = nameof(OrderViewModel.Comment);

            View.ShowColumns = true;
            View.Properties.Columns = new List<string>
            {
                nameof(OrderViewModel.Side),
                nameof(OrderViewModel.Type),
                nameof(OrderViewModel.Lots),
                nameof(OrderViewModel.Price),
                nameof(OrderViewModel.SL),
                nameof(OrderViewModel.TP),
                nameof(OrderViewModel.CreatedTime),
                nameof(OrderViewModel.ExpireTime)
            };

            View.Properties.ColumnNames = new List<string>
            {
                "Side", "Type", "Lots", "Price",
                "SL", "TP", "Created", "Expire"
            };
        }

        private void InnerList_DoubleClick(object? sender, EventArgs e)
        {
            var selected = View.SelectedItem as OrderViewModel;
            if (selected != null)
                OrderDoubleClicked?.Invoke(selected);
        }

        protected override string GetPersistString()
        {
            return nameof(OrdersDock);
        }
    }
}
