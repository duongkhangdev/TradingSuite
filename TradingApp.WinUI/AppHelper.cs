using Cuckoo.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingApp.WinUI
{
    public class AppHelper
    {
        public static async Task<List<AppQuote>?> ReadFile(string symbol, string time_frame)
        {
            // Đọc file text
            var content = await FileHelper.ReadAllTextAsync($"Samples\\{symbol}_{time_frame}.txt");
            if (string.IsNullOrEmpty(content))
                content = await FileHelper.ReadAllTextAsync($"Samples\\{symbol}_{time_frame}.json");

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

        
    }
}
