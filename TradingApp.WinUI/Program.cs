using System;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using TradingApp.WinUI.Docking;
using TradingApp.WinUI.Logging;
using ChartPro.Services;
using TradingApp.WinUI.Factories;
using ChartPro; // add for IQuoteService

namespace TradingApp.WinUI
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Configure Serilog sinks (still used by UI log dock)
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Debug()
                .WriteTo.Sink(new UiRichTextBoxSink())
                .CreateLogger();

            try
            {
                using var host = CreateHostBuilder().Build();

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
                var sp = host.Services;
                Application.Run(sp.GetRequiredService<MainForm>());
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

        private static IHostBuilder CreateHostBuilder()
            => Host.CreateDefaultBuilder()
                .ConfigureServices((ctx, services) =>
                {
                    // core chart services
                    services.AddSingleton<IQuoteService, QuoteService>(); // register QuoteService for DI
                    services.AddSingleton<IChartDataService>(sp =>
    new QuoteChartDataService(sp.GetRequiredService<IQuoteService>())); // default data source (can be swapped to QuoteChartDataService later)
                    services.AddSingleton<IChartService, ChartService>();
                    services.AddSingleton<ISubPlotService>(sp => new SubPlotService(sp.GetRequiredService<IChartService>()));
                    services.AddSingleton<IChartDocumentFactory, ChartDocumentFactory>();

                    // UI roots
                    services.AddSingleton<MainForm>();
                });
    }
}
