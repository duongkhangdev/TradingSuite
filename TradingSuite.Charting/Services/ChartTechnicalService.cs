using Cuckoo.Shared;
using Microsoft.Extensions.Logging;
using ScottPlot;
using Skender.Stock.Indicators;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using TradingSuite.Charting.Extensions;
using TradingSuite.Charting.Indicators;
using TradingSuite.Charting.Extensions;

namespace TradingSuite.Charting.Services
{
    public interface IChartTechnicalService
    {
        void AddOrUpdate(string symbol, string time_frame, string unique_key, object? model);
        Task<object?> GetAsync(string symbol, string time_frame, string unique_key);
        bool Remove(string symbol);
        bool ContainsKey(string symbol);
        bool ContainsKey(string symbol, string time_frame);
        bool ContainsKey(string symbol, string time_frame, string unique_key);
        int Count();

        Task IndicatorsCompute(string symbol, string time_frame, List<AppQuote> quotes);
        Task<Dictionary<string, object>?> GetIndicatorsDictionary(string symbol, string time_frame);
    }

    public class ChartTechnicalService : IChartTechnicalService
    {
        private static readonly ConcurrentDictionary<string,
                                    ConcurrentDictionary<string,
                                        ConcurrentDictionary<string, object>>> _data = new();
        private readonly ILogger<ChartTechnicalService> _logger;

        public ChartTechnicalService(ILogger<ChartTechnicalService> logger)
        {
            _logger = logger;
        }

        public void AddOrUpdate(string symbol, string time_frame, string unique_key, object? model)
        {
            if (string.IsNullOrEmpty(symbol) || string.IsNullOrEmpty(time_frame) || string.IsNullOrEmpty(unique_key) || model is null)
                return;

            if (!_data.ContainsKey(symbol))
            {
                if (_data.TryAdd(symbol, new ConcurrentDictionary<string, ConcurrentDictionary<string, object>>()))
                {
                    _logger.LogInformation("Added new symbol {Symbol}.", symbol);
                }
            }
            if (!_data[symbol].ContainsKey(time_frame))
            {
                if (_data[symbol].TryAdd(time_frame, new ConcurrentDictionary<string, object>()))
                {
                    _logger.LogInformation("Added new time frame {TimeFrame} for symbol {Symbol}.", time_frame, symbol);
                }
            }
            if (!_data[symbol][time_frame].ContainsKey(unique_key))
            {
                if (_data[symbol][time_frame].TryAdd(unique_key, model))
                {
                    _logger.LogInformation("Added new key {Key} for {Symbol}/{TimeFrame}.", unique_key, symbol, time_frame);
                }
                return;
            }
            if (_data[symbol][time_frame].TryUpdate(unique_key, model, _data[symbol][time_frame][unique_key]))
            {
                _logger.LogInformation("Updated {Symbol}/{TimeFrame} key {Key}.", symbol, time_frame, unique_key);
            }
        }

        public async Task<object?> GetAsync(string symbol, string time_frame, string unique_key)
        {
            if (!ContainsKey(symbol))
                return null;
            if (!ContainsKey(symbol, time_frame))
                return null;
            if (!ContainsKey(symbol, time_frame, unique_key))
                return null;

            var result = _data[symbol][time_frame][unique_key];
            return await Task.FromResult(result);
        }

        public bool Remove(string symbol)
        {
            return _data.TryRemove(symbol, out _);
        }

        public bool ContainsKey(string symbol)
        {
            return _data.ContainsKey(symbol);
        }

        public bool ContainsKey(string symbol, string time_frame)
        {
            if (!ContainsKey(symbol))
                return false;

            return _data[symbol].ContainsKey(time_frame);
        }

        public bool ContainsKey(string symbol, string time_frame, string unique_key)
        {
            if (!ContainsKey(symbol))
                return false;
            if (!ContainsKey(symbol, time_frame))
                return false;

            return _data[symbol][time_frame].ContainsKey(unique_key);
        }

        public int Count()
        {
            return _data.Count;
        }

        public async Task IndicatorsCompute(string symbol, string time_frame, List<AppQuote> quotes)
        {
            var sw = Stopwatch.StartNew();
            await Task.Run(() =>
            {
                try
                {
                    List<OHLC> ohlcs = quotes.ToOhlcs(BrokerHelper.GetInterval(time_frame));
                    if (ohlcs != null && ohlcs.Any())
                    {
                        AddOrUpdate(symbol, time_frame, "OHLCS", ohlcs);
                    }

                    var times = quotes.Select(q => q.Date.ToOADate()).ToArray();
                    AddOrUpdate(symbol, time_frame, "Times", times);

                    var closes = quotes.Select(q => Convert.ToDouble(q.Close)).ToArray();
                    AddOrUpdate(symbol, time_frame, "Closes", closes);

                    var rsiArr = ComputeRsi(closes, 14);
                    AddOrUpdate(symbol, time_frame, "RsiArr", rsiArr);

                    var macdArr = ComputeMacd(closes);
                    AddOrUpdate(symbol, time_frame, "MacdArr", macdArr);

                    var cciArr = ComputeCci(closes, 20);
                    AddOrUpdate(symbol, time_frame, "CciArr", cciArr);

                    var stochRsiArr = ComputeStochRsi(closes, 14);
                    AddOrUpdate(symbol, time_frame, "StochRsiArr", stochRsiArr);

                    var bb12 = quotes.GetBollingerBandsResults(20, 1.2);
                    if (bb12 != null && bb12.Any())
                    {
                        AddOrUpdate(symbol, time_frame, "BB12", bb12);
                    }

                    var bb17 = quotes.GetBollingerBandsResults(20, 1.7);
                    if (bb17 != null && bb17.Any())
                    {
                        AddOrUpdate(symbol, time_frame, "BB17", bb17);
                    }

                    var bb25 = quotes.GetBollingerBandsResults(20, 2.5);
                    if (bb25 != null && bb25.Any())
                    {
                        AddOrUpdate(symbol, time_frame, "BB25", bb25);
                    }

                    var bb35 = quotes.GetBollingerBandsResults(20, 3.5);
                    if (bb35 != null && bb35.Any())
                    {
                        AddOrUpdate(symbol, time_frame, "BB35", bb35);
                    }

                    var bb45 = quotes.GetBollingerBandsResults(20, 4.5);
                    if (bb45 != null && bb45.Any())
                    {
                        AddOrUpdate(symbol, time_frame, "BB45", bb45);
                    }

                    var atrStop = quotes.GetAtrStopResults();
                    if (atrStop != null && atrStop.Any())
                    {
                        AddOrUpdate(symbol, time_frame, "AtrStop", atrStop);
                    }

                    var superTrend = quotes.GetSuperTrendResults(10, 3.0);
                    if (superTrend != null && superTrend.Any())
                    {
                        AddOrUpdate(symbol, time_frame, "SuperTrend", superTrend);
                    }

                    var ichimoku = quotes.GetIchimokuResults();
                    if (ichimoku != null && ichimoku.Any())
                    {
                        AddOrUpdate(symbol, time_frame, "Ichimoku", ichimoku);
                    }

                    var alligator = quotes.GetAlligatorResults();
                    if (alligator != null && alligator.Any())
                    {
                        AddOrUpdate(symbol, time_frame, "Alligator", alligator);
                    }

                    var sar = quotes.GetParabolicSarResults(0.02, 0.2);
                    if (sar != null && sar.Any())
                    {
                        AddOrUpdate(symbol, time_frame, "Sar", sar);
                    }

                    var adx = quotes.GetAdxResults();
                    if (adx != null && adx.Any())
                    {
                        AddOrUpdate(symbol, time_frame, "Adx", adx);
                    }

                    var macd = quotes.GetMacdResults();
                    if (macd != null && macd.Any())
                    {
                        AddOrUpdate(symbol, time_frame, "Macd", macd);
                    }

                    var stochRsi = quotes.GetStochRsiResults(14, 14, 3, 3);
                    if (stochRsi != null && stochRsi.Any())
                    {
                        AddOrUpdate(symbol, time_frame, "StochRsi", stochRsi);
                    }

                    var atr = quotes.GetAtrResults();
                    if (atr != null && atr.Any())
                    {
                        AddOrUpdate(symbol, time_frame, "Atr", atr);
                    }

                    var rsi = quotes.GetRsiResults();
                    if (rsi != null && rsi.Any())
                    {
                        AddOrUpdate(symbol, time_frame, "Rsi", rsi);
                    }

                    var cci = quotes.GetCciResults();
                    if (cci != null && cci.Any())
                    {
                        AddOrUpdate(symbol, time_frame, "Cci", cci);
                    }

                    var ema5 = quotes.GetEmaResults(5);
                    if (ema5 != null && ema5.Any())
                    {
                        AddOrUpdate(symbol, time_frame, "Ema5", ema5);
                    }

                    var ema9 = quotes.GetEmaResults(9);
                    if (ema9 != null && ema9.Any())
                    {
                        AddOrUpdate(symbol, time_frame, "Ema9", ema9);
                    }

                    var ema21 = quotes.GetEmaResults(21);
                    if (ema21 != null && ema21.Any())
                    {
                        AddOrUpdate(symbol, time_frame, "Ema21", ema21);
                    }

                    var ema34 = quotes.GetEmaResults(34);
                    if (ema34 != null && ema34.Any())
                    {
                        AddOrUpdate(symbol, time_frame, "Ema34", ema34);
                    }

                    var ema55 = quotes.GetEmaResults(55);
                    if (ema55 != null && ema55.Any())
                    {
                        AddOrUpdate(symbol, time_frame, "Ema55", ema55);
                    }

                    var ema89 = quotes.GetEmaResults(89);
                    if (ema89 != null && ema89.Any())
                    {
                        AddOrUpdate(symbol, time_frame, "Ema89", ema89);
                    }

                    var ema144 = quotes.GetEmaResults(144);
                    if (ema144 != null && ema144.Any())
                    {
                        AddOrUpdate(symbol, time_frame, "Ema144", ema144);
                    }

                    var ema200 = quotes.GetEmaResults(200);
                    if (ema200 != null && ema200.Any())
                    {
                        AddOrUpdate(symbol, time_frame, "Ema200", ema200);
                    }

                    var sma7 = quotes.GetSmaResults(7);
                    if (sma7 != null && sma7.Any())
                    {
                        AddOrUpdate(symbol, time_frame, "Sma7", sma7);
                    }

                    var sma25 = quotes.GetSmaResults(25);
                    if (sma25 != null && sma25.Any())
                    {
                        AddOrUpdate(symbol, time_frame, "Sma25", sma25);
                    }

                    var sma99 = quotes.GetSmaResults(99);
                    if (sma99 != null && sma99.Any())
                    {
                        AddOrUpdate(symbol, time_frame, "Sma99", sma99);
                    }

                    var sma200 = quotes.GetSmaResults(200);
                    if (sma200 != null && sma200.Any())
                    {
                        AddOrUpdate(symbol, time_frame, "Sma200", sma200);
                    }

                    var vwma5 = quotes.GetVwmaResults(5);
                    if (vwma5 != null && vwma5.Any())
                    {
                        AddOrUpdate(symbol, time_frame, "Vwma5", vwma5);
                    }

                    var vwma9 = quotes.GetVwmaResults(9);
                    if (vwma9 != null && vwma9.Any())
                    {
                        AddOrUpdate(symbol, time_frame, "Vwma9", vwma9);
                    }

                    var vwma21 = quotes.GetVwmaResults(21);
                    if (vwma21 != null && vwma21.Any())
                    {
                        AddOrUpdate(symbol, time_frame, "Vwma21", vwma21);
                    }

                    var vwma34 = quotes.GetVwmaResults(34);
                    if (vwma34 != null && vwma34.Any())
                    {
                        AddOrUpdate(symbol, time_frame, "Vwma34", vwma34);
                    }

                    var vwma55 = quotes.GetVwmaResults(55);
                    if (vwma55 != null && vwma55.Any())
                    {
                        AddOrUpdate(symbol, time_frame, "Vwma55", vwma55);
                    }

                    var vwma89 = quotes.GetVwmaResults(89);
                    if (vwma89 != null && vwma89.Any())
                    {
                        AddOrUpdate(symbol, time_frame, "Vwma89", vwma89);
                    }

                    var vwma144 = quotes.GetVwmaResults(144);
                    if (vwma144 != null && vwma144.Any())
                    {
                        AddOrUpdate(symbol, time_frame, "Vwma144", vwma144);
                    }

                    var vwma200 = quotes.GetVwmaResults(200);
                    if (vwma200 != null && vwma200.Any())
                    {
                        AddOrUpdate(symbol, time_frame, "Vwma200", vwma200);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "IndicatorsCompute failed for {Symbol} {TimeFrame}", symbol, time_frame);
                }
            });

            sw.Stop();
            _logger.LogInformation("Calculated indicators for {Symbol}/{TimeFrame} in {ElapsedSeconds:F2}s", symbol, time_frame, sw.Elapsed.TotalSeconds);

            await Task.CompletedTask;
        }

        public async Task<Dictionary<string, object>?> GetIndicatorsDictionary(string symbol, string time_frame)
        {
            var heikinAshi = await GetAsync(symbol, time_frame, "HeikinAshi") as List<AppQuote?>;

            var ichimoku = await GetAsync(symbol, time_frame, "Ichimoku") as List<IchimokuResult?>;
            var superTrend = await GetAsync(symbol, time_frame, "SuperTrend") as List<SuperTrendResult?>;
            var atrStop = await GetAsync(symbol, time_frame, "AtrStop") as List<AtrStopResult?>;
            var alligator = await GetAsync(symbol, time_frame, "Alligator") as List<AlligatorResult?>;

            var sar = await GetAsync(symbol, time_frame, "Sar") as List<ParabolicSarResult?>;

            var adx = await GetAsync(symbol, time_frame, "Adx") as List<AdxResult?>;
            var macd = await GetAsync(symbol, time_frame, "Macd") as List<Skender.Stock.Indicators.MacdResult?>;
            var stochRsi = await GetAsync(symbol, time_frame, "StochRsi") as List<StochRsiResult?>;
            var atr = await GetAsync(symbol, time_frame, "Atr") as List<AtrResult?>;
            var rsi = await GetAsync(symbol, time_frame, "Rsi") as List<RsiResult?>;
            var cci = await GetAsync(symbol, time_frame, "Cci") as List<CciResult?>;
            var ema5 = await GetAsync(symbol, time_frame, "Ema5") as List<EmaResult?>;
            var ema9 = await GetAsync(symbol, time_frame, "Ema9") as List<EmaResult?>;
            var ema21 = await GetAsync(symbol, time_frame, "Ema21") as List<EmaResult?>;
            var ema34 = await GetAsync(symbol, time_frame, "Ema34") as List<EmaResult?>;
            var ema55 = await GetAsync(symbol, time_frame, "Ema55") as List<EmaResult?>;
            var ema89 = await GetAsync(symbol, time_frame, "Ema89") as List<EmaResult?>;
            var ema144 = await GetAsync(symbol, time_frame, "Ema144") as List<EmaResult?>;
            var ema200 = await GetAsync(symbol, time_frame, "Ema200") as List<EmaResult?>;
            var bb12 = await GetAsync(symbol, time_frame, "BB12") as List<BollingerBandsResult?>;
            var bb17 = await GetAsync(symbol, time_frame, "BB17") as List<BollingerBandsResult?>;
            var bb25 = await GetAsync(symbol, time_frame, "BB25") as List<BollingerBandsResult?>;
            var bb35 = await GetAsync(symbol, time_frame, "BB35") as List<BollingerBandsResult?>;

            var sma7 = await GetAsync(symbol, time_frame, "Sma7") as List<SmaResult?>;
            var sma25 = await GetAsync(symbol, time_frame, "Sma25") as List<SmaResult?>;
            var sma99 = await GetAsync(symbol, time_frame, "Sma99") as List<SmaResult?>;
            var sma200 = await GetAsync(symbol, time_frame, "Sma200") as List<SmaResult?>;

            var vwma5 = await GetAsync(symbol, time_frame, "Vwma5") as List<VwmaResult?>;
            var vwma9 = await GetAsync(symbol, time_frame, "Vwma9") as List<VwmaResult?>;
            var vwma21 = await GetAsync(symbol, time_frame, "Vwma21") as List<VwmaResult?>;
            var vwma34 = await GetAsync(symbol, time_frame, "Vwma34") as List<VwmaResult?>;
            var vwma55 = await GetAsync(symbol, time_frame, "Vwma55") as List<VwmaResult?>;
            var vwma89 = await GetAsync(symbol, time_frame, "Vwma89") as List<VwmaResult?>;
            var vwma144 = await GetAsync(symbol, time_frame, "Vwma144") as List<VwmaResult?>;
            var vwma200 = await GetAsync(symbol, time_frame, "Vwma200") as List<VwmaResult?>;

            var indicators = new Dictionary<string, object>
            {
                ["BB12"] = bb12!,
                ["BB17"] = bb17!,
                ["BB25"] = bb25!,
                ["BB35"] = bb35!,

                ["Ichimoku"] = ichimoku!,
                ["SuperTrend"] = superTrend!,
                ["AtrStop"] = atrStop!,
                ["Alligator"] = alligator!,
                ["Sar"] = sar!,
                ["Adx"] = adx!,
                ["Macd"] = macd!,
                ["StochRsi"] = stochRsi!,

                ["Atr"] = atr!,
                ["Rsi"] = rsi!,
                ["Cci"] = cci!,

                ["Ema5"] = ema5!,
                ["Ema9"] = ema9!,
                ["Ema21"] = ema21!,
                ["Ema34"] = ema34!,
                ["Ema55"] = ema55!,
                ["Ema89"] = ema89!,
                ["Ema144"] = ema144!,
                ["Ema200"] = ema200!,

                ["Sma7"] = sma7!,
                ["Sma25"] = sma25!,
                ["Sma99"] = sma99!,
                ["Sma200"] = sma200!,

                ["Vwma5"] = vwma5!,
                ["Vwma9"] = vwma9!,
                ["Vwma21"] = vwma21!,
                ["Vwma55"] = vwma55!,
                ["Vwma89"] = vwma89!,
                ["Vwma144"] = vwma144!,
                ["Vwma200"] = vwma200!,
            };

            return indicators;
        }

        private static double[] ComputeRsi(double[] closes, int period)
        {
            if (closes.Length == 0 || period <= 0) return Array.Empty<double>();
            double[] rsi = new double[closes.Length];
            double avgGain = 0, avgLoss = 0;
            for (int i = 1; i < closes.Length; i++)
            {
                double change = closes[i] - closes[i - 1];
                double gain = change > 0 ? change : 0;
                double loss = change < 0 ? -change : 0;

                if (i <= period)
                {
                    avgGain += gain;
                    avgLoss += loss;
                    if (i == period)
                    {
                        avgGain /= period;
                        avgLoss /= period;
                        double rs = avgLoss == 0 ? double.PositiveInfinity : avgGain / avgLoss;
                        rsi[i] = 100 - (100 / (1 + rs));
                    }
                }
                else
                {
                    avgGain = (avgGain * (period - 1) + gain) / period;
                    avgLoss = (avgLoss * (period - 1) + loss) / period;
                    double rs = avgLoss == 0 ? double.PositiveInfinity : avgGain / avgLoss;
                    rsi[i] = 100 - (100 / (1 + rs));
                }
            }
            for (int i = 0; i < Math.Min(period, rsi.Length); i++) rsi[i] = double.NaN;
            return rsi;
        }

        private static double[] ComputeMacd(double[] closes)
        {
            double[] ema12 = ComputeEma(closes, 12);
            double[] ema26 = ComputeEma(closes, 26);
            return closes.Select((_, i) => ema12[i] - ema26[i]).ToArray();
        }

        private static double[] ComputeCci(double[] closes, int period)
        {
            if (closes.Length == 0) return Array.Empty<double>();
            double[] cci = new double[closes.Length];
            for (int i = 0; i < closes.Length; i++)
            {
                if (i < period) { cci[i] = double.NaN; continue; }
                double sma = closes.Skip(i - period + 1).Take(period).Average();
                double meanDev = closes.Skip(i - period + 1).Take(period).Average(v => Math.Abs(v - sma));
                if (meanDev == 0) { cci[i] = 0; continue; }
                cci[i] = (closes[i] - sma) / (0.015 * meanDev);
            }
            return cci;
        }

        private static double[] ComputeStochRsi(double[] closes, int period)
        {
            if (closes.Length < period) return closes.Select(_ => double.NaN).ToArray();
            double[] rsi = ComputeRsi(closes, period);
            double[] stoch = new double[closes.Length];
            for (int i = 0; i < closes.Length; i++)
            {
                if (i < period) { stoch[i] = double.NaN; continue; }
                var window = rsi.Skip(i - period + 1).Take(period).ToArray();
                double min = window.Where(v => !double.IsNaN(v)).DefaultIfEmpty(double.NaN).Min();
                double max = window.Where(v => !double.IsNaN(v)).DefaultIfEmpty(double.NaN).Max();
                if (double.IsNaN(min) || double.IsNaN(max) || max - min == 0) { stoch[i] = double.NaN; continue; }
                stoch[i] = (rsi[i] - min) / (max - min) * 100.0;
            }
            return stoch;
        }

        private static double[] ComputeEma(double[] values, int period)
        {
            double[] ema = new double[values.Length];
            if (values.Length == 0) return ema;
            double k = 2.0 / (period + 1);
            ema[0] = values[0];
            for (int i = 1; i < values.Length; i++)
                ema[i] = values[i] * k + ema[i - 1] * (1 - k);
            return ema;
        }
    }
}
