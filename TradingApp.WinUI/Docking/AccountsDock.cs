using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Fluent;
using TradingApp.WinUI.Models;

namespace TradingApp.WinUI.Docking
{
    public class AccountsDock : FluentDockBase<AccountViewModel>
    {
        public event Action<AccountViewModel>? AccountDoubleClicked;

        private bool _innerListHooked;

        public AccountsDock()
        {
            Text = "Accounts";
            TabText = "Accounts";
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

            View.Properties.Name = nameof(AccountViewModel.Name);
            View.Properties.Description = nameof(AccountViewModel.Description);

            View.ShowColumns = true;
            View.Properties.Columns = new List<string>
            {
                nameof(AccountViewModel.Broker),
                nameof(AccountViewModel.AccountId),
                nameof(AccountViewModel.Type),
                nameof(AccountViewModel.Currency),
                nameof(AccountViewModel.Leverage),
                nameof(AccountViewModel.Balance),
                nameof(AccountViewModel.Equity),
                nameof(AccountViewModel.FreeMargin),
                nameof(AccountViewModel.MarginLevel),
                nameof(AccountViewModel.IsCurrent)
            };

            View.Properties.ColumnNames = new List<string>
            {
                "Broker", "Account", "Type", "Cur", "Lev",
                "Balance", "Equity", "FreeMargin", "Margin %", "Current"
            };
        }

        private void InnerList_DoubleClick(object? sender, EventArgs e)
        {
            var acc = View.SelectedItem as AccountViewModel;
            if (acc != null)
                AccountDoubleClicked?.Invoke(acc);
        }

        protected override string GetPersistString()
        {
            return nameof(AccountsDock);
        }
    }
}
