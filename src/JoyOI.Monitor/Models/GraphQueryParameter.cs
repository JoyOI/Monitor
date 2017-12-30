using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JoyOI.Monitor.Models

{
    public class GraphScaling
    {
        public int Points { get; set; }
        public int Interval { get; set; }
        public GraphScaling(int start, int end, int interval)
        {
            int range = end - start;
            Points = range / interval;
            Interval = interval;
        }
    }
}
