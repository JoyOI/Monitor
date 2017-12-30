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
    }

    public class ChartData {
        public List<string> Labels { get; set; }
        public List<ChartDataSet> Datasets { get; set; }
    }

    public class ChartDataSet {
        public string Label { get; set; }
        public List<double> Data { get; set; }
    }
}
