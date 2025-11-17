using System.Collections.Generic;
using System.Windows.Forms;
using Fluent;
using WeifenLuo.WinFormsUI.Docking;

namespace TradingApp.WinUI.Docking
{
    public abstract class FluentDockBase<T> : DockContent
    {
        protected readonly FluentListView View;

        protected FluentDockBase()
        {
            View = new FluentListView
            {
                Dock = DockStyle.Fill
            };

            Controls.Add(View);

            ConfigureView();
        }

        protected abstract void ConfigureView();

        public virtual void SetItems(IEnumerable<T> items)
        {
            View.Items = new List<T>(items);
            View.Redraw();
        }
    }
}
