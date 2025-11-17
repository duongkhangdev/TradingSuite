using ChartPro;
using ChartPro.Services;
using Cuckoo.WinLifetime;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Windows.Forms;
using TradingApp.WinUI.Docking;
using TradingApp.WinUI.Logging;

namespace TradingApp.WinUI
{
    internal static class Program
    {
        [STAThread]
        static async Task Main()
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

                var builder = Host.CreateApplicationBuilder();

                builder.UseWindowsFormsLifetime<MainForm>(options =>
                {
                    options.HighDpiMode = HighDpiMode.SystemAware;
                    options.EnableVisualStyles = true;
                    options.CompatibleTextRenderingDefault = false;
                    options.SuppressStatusMessages = false;
                    options.EnableConsoleShutdown = false; // true: close app when console window is closed
                });

                builder.Services.AddSingleton<IQuoteService, QuoteService>();
                builder.Services.AddTransient<IChartService, ChartService>();
                builder.Services.AddTransient<IChartSubplotService, ChartSubplotService>();
                builder.Services.AddSingleton<IChartTechnicalService, ChartTechnicalService>();


                builder.Services.AddTransient<AccountsDock>();
                builder.Services.AddTransient<ChartDocument>();
                builder.Services.AddTransient<HistoryDock>();
                builder.Services.AddTransient<LogDock>();
                builder.Services.AddTransient<OrdersDock>();
                builder.Services.AddTransient<PositionsDock>();
                builder.Services.AddTransient<SignalsDock>();
                builder.Services.AddTransient<WatchlistDock>();

                var app = builder.Build();

                try
                {
                    //var discordBot = app.Services.GetRequiredService<IDiscordService>();
                    //await discordBot.StartBotAsync();
                    //Log.Information("starting server.");
                    await app.RunAsync();
                }
                catch (Exception ex)
                {
                    //Log.Fatal(ex, "An error occurred while closing Serilog.");
                }
                finally
                {
                    //Log.CloseAndFlush();
                }
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
