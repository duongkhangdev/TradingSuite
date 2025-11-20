using TradingApp.WebApi.Hubs;
using TradingApp.WebApi.Services;
using TradingSuite.Charting.Services;

var builder = WebApplication.CreateBuilder(args);

var keepAlive = TimeSpan.FromSeconds(120);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddSingleton<IQuoteService, QuoteService>();
builder.Services.AddSingleton<IChartTechnicalService, ChartTechnicalService>();
builder.Services.AddSingleton<IWebSocketConnectionManager, WebSocketConnectionManager>();
builder.Services.AddSignalR();
builder.Services.AddSingleton<ITradingBroadcaster, TradingBroadcaster>();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseWebSockets(new WebSocketOptions
{
    KeepAliveInterval = keepAlive
});

app.UseAuthorization();

app.MapControllers();
app.MapHub<TradingHub>("/hubs/trading");

app.Run();
