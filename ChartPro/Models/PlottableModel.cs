using ScottPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChartPro
{
    public class PlottableModel
    {
        public string? Category { get; set; }
        public string? Key { get; set; }
        public string? Symbol { get; set; }
        public IPlottable? Plottable { get; set; }
        public object? Object { get; set; }
        public PlottType PlottType { get; set; }        

        public PlottableModel()
        {

        }

        public PlottableModel(string category, string key, string symbol, IPlottable? plottable, object? @object, PlottType plottType)
        {
            this.Category = category;
            this.Key = key;
            this.Symbol = symbol;
            this.Plottable = plottable;
            this.Object = @object;
            this.PlottType = plottType;
        }
    }
}
