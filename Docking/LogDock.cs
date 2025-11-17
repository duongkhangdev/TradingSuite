using System;
using System.Drawing;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace TradingApp.WinUI.Docking
{
    public class LogDock : DockContent
    {
        private readonly RichTextBox _rtb;

        public static LogDock? Instance { get; private set; }

        public LogDock()
        {
            Text = "Log";
            TabText = "Log";

            _rtb = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                WordWrap = false,
                HideSelection = false,
                BackColor = Color.Black,
                ForeColor = Color.White,
                Font = new Font("Consolas", 9f)
            };

            Controls.Add(_rtb);

            Instance = this;
        }

        public void AppendLog(string text)
        {
            if (IsDisposed) return;

            if (InvokeRequired)
            {
                BeginInvoke(new Action<string>(AppendLog), text);
                return;
            }

            _rtb.AppendText(text + Environment.NewLine);
            _rtb.SelectionStart = _rtb.TextLength;
            _rtb.ScrollToCaret();
        }

        public static void Append(string text)
        {
            Instance?.AppendLog(text);
        }

        protected override string GetPersistString()
        {
            return nameof(LogDock);
        }
    }
}
