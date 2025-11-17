using System;
using System.Windows.Forms;
using Serilog;
using TradingApp.WinUI.Logging;

namespace TradingApp.WinUI
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            // Configure Serilog
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Debug()
                .WriteTo.Sink(new UiRichTextBoxSink())
                .CreateLogger();

            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                Application.ThreadException += (s, e) =>
                {
                    Log.Error(e.Exception, "Unhandled UI exception");
                };

                AppDomain.CurrentDomain.UnhandledException += (s, e) =>
                {
                    if (e.ExceptionObject is Exception ex)
                        Log.Fatal(ex, "Unhandled non-UI exception");
                };

                Log.Information("Application starting");
                Application.Run(new MainForm());
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application terminated unexpectedly");
                throw;
            }
            finally
            {
                Log.Information("Application shutting down");
                Log.CloseAndFlush();
            }
        }
    }
}
