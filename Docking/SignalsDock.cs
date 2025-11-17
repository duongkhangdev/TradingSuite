using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Fluent;
using TradingApp.WinUI.Models;

namespace TradingApp.WinUI.Docking
{
    public class SignalsDock : FluentDockBase<SignalViewModel>
    {
        public event Action<SignalViewModel>? SignalDoubleClicked;

        private bool _innerListHooked;

        public SignalsDock()
        {
            Text = "Signals";
            TabText = "Signals";
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

            View.Properties.Name = nameof(SignalViewModel.Title);
            View.Properties.Icon = nameof(SignalViewModel.Icon);
            View.Properties.Description = nameof(SignalViewModel.Detail);

            View.ShowColumns = true;
            View.Properties.Columns = new List<string>
            {
                nameof(SignalViewModel.Time),
                nameof(SignalViewModel.Symbol),
                nameof(SignalViewModel.Timeframe),
                nameof(SignalViewModel.Price),
                nameof(SignalViewModel.Source),
                nameof(SignalViewModel.IsNew)
            };

            View.Properties.ColumnNames = new List<string>
            {
                "Time", "Symbol", "TF", "Price", "Source", "New"
            };
        }

        private void InnerList_DoubleClick(object? sender, EventArgs e)
        {
            var selected = View.SelectedItem as SignalViewModel;
            if (selected != null)
                SignalDoubleClicked?.Invoke(selected);
        }

        protected override string GetPersistString()
        {
            return nameof(SignalsDock);
        }
    }
}
