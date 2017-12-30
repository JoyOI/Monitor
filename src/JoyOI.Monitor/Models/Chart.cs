using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JoyOI.Monitor.Models
{
    public class Chart
    {
        public string Title { get; set; }
        public List<ChartData> Data { get; set; }
    }

    public class ChartData {
        public string Title { get; set; }
        public List<DataPoint> Data { get; set; }
    }

    public class DataPoint
    {
        public long Timestamp { get; set; }
        public double Value { get; set; }
    }
}
