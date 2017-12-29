using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JoyOI.Monitor.Lib
{
    public class GraphScaling
    {
        public int Points { get; set; }
        public int Interval { get; set; }
        public GraphScaling(int range, int interval)
        {
            this.Points = range / interval; ;
            this.Interval = interval;
        }
    }
}
