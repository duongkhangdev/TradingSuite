using System;
using Serilog.Core;
using Serilog.Events;
using TradingApp.WinUI.Docking;

namespace TradingApp.WinUI.Logging
{
    public class UiRichTextBoxSink : ILogEventSink
    {
        private readonly IFormatProvider? _formatProvider;

        public UiRichTextBoxSink(IFormatProvider? formatProvider = null)
        {
            _formatProvider = formatProvider;
        }

        public void Emit(LogEvent logEvent)
        {
            string message = logEvent.RenderMessage(_formatProvider);

            string line = $"{logEvent.Timestamp:HH:mm:ss} [{logEvent.Level}] {message}";
            if (logEvent.Exception != null)
                line += " " + logEvent.Exception;

            LogDock.Append(line);
        }
    }
}
