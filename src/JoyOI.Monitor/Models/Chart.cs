using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JoyOI.Monitor.Models
{
    public class Chart
    {
        public string Title { get; set; }
        public string Type { get; set; }
        public ChartData Data { get; set; }
        public Object Options { get; set; }
    }

    public class ChartData {
        public IEnumerable<string> Labels { get; set; }
        public IEnumerable<ChartDataSet> Datasets { get; set; }
    }

    public class ChartDataSet {
        public string BackgroundColor { get; set; }
        public string BorderColor { get; set; }
        public bool Fill { get; set; } = true;
        public string Label { get; set; }
        public IEnumerable<double> Data { get; set; }
    }
}
