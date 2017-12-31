using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JoyOI.Monitor.Models

{
    public class ChartScaling
    {
        public int Points { get; set; }
        public int Interval { get; set; }
        public int Start { get; set; }
        public int End { get; set; }

        public ChartScaling(int start, int end, int interval)
        {
            int range = end - start;
            Points = range / interval;
            Interval = interval;
            Start = start;
            End = end;
        }
    }
}
