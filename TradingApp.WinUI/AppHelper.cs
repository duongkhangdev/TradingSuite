using Cuckoo.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingApp.WinUI
{
    public class AppHelper
    {
        private sealed record SampleDescriptor(string Symbol, string Timeframe);

        public static IReadOnlyList<(string Symbol, string Timeframe)> DiscoverSamples()
        {
            var root = SamplesRoot;
            if (!Directory.Exists(root))
                return Array.Empty<(string, string)>();

            var result = new List<(string, string)>();
            var files = Directory.GetFiles(root, "*.*", SearchOption.TopDirectoryOnly)
                .Where(f => f.EndsWith(".txt", StringComparison.OrdinalIgnoreCase) || f.EndsWith(".json", StringComparison.OrdinalIgnoreCase));

            foreach (var file in files)
            {
                var name = Path.GetFileNameWithoutExtension(file);
                var parts = name.Split('_');
                if (parts.Length < 2)
                    continue;

                var timeframe = parts[^1];
                var symbol = string.Join('_', parts.Take(parts.Length - 1));
                if (string.IsNullOrWhiteSpace(symbol) || string.IsNullOrWhiteSpace(timeframe))
                    continue;

                var tuple = (symbol.ToUpperInvariant(), timeframe.ToUpperInvariant());
                if (!result.Contains(tuple))
                    result.Add(tuple);
            }

            return result;
        }

        public static string SamplesRoot
        {
            get
            {
                var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                var candidate = Path.Combine(baseDir, "Samples");
                return Directory.Exists(candidate)
                    ? candidate
                    : Path.Combine(AppContext.BaseDirectory, "Samples");
            }
        }

        public static async Task<List<AppQuote>?> ReadFile(string symbol, string time_frame)
        {
            var content = await ReadSampleContentAsync(symbol, time_frame);

            if (string.IsNullOrEmpty(content))
            {
                MessageBox.Show($"Không tìm thấy file txt, json {symbol}_{time_frame}");
                return null;
            }

            var quotes = JsonHelper.DeserializeToObj<List<AppQuote>?>(content.Trim(), out var exception);
            if (exception != null)
            {
                MessageBox.Show($"Error parsing JSON: {exception}");
                return null;
            }

            return quotes;
        }

        private static async Task<string?> ReadSampleContentAsync(string symbol, string timeframe)
        {
            string[] extensions = new[] { ".txt", ".json" };

            foreach (var ext in extensions)
            {
                var fileName = $"{symbol}_{timeframe}{ext}";
                var path = Path.Combine(SamplesRoot, fileName);
                if (File.Exists(path))
                    return await File.ReadAllTextAsync(path);
            }

            return null;
        }

        
    }
}
