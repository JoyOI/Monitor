using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using JoyOI.Monitor.Models;
using System.Threading;

namespace JoyOI.Monitor.Controllers.OnlineJudge
{
    [Route("/OnlineJudge/Hack")]
    public class HackController : ChartController
    {
        [HttpGet("Chart")]
        public async Task<IActionResult> Chart(int start, int end, int interval, int timezoneoffset, CancellationToken token)
        {
            if (start == 0 || end == 0 || interval == 0)
            {
                Response.StatusCode = 400;
                return Json(null);
            }
            var scaling = new ChartScaling(start, end, interval);
            return Json(await GetData(
                Judge,
                @"SELECT 
                  Result as n, 
                  FLOOR(UNIX_TIMESTAMP(Time) / @interval) * @interval as t,  
                  Count(Id) as c
                  FROM joyoi_oj.hackstatuses 
                  WHERE UNIX_TIMESTAMP(Time) >= @start AND UNIX_TIMESTAMP(Time) <= @end 
                  GROUP BY n, t 
                  HAVING t >= @start AND t <= @end 
                  ORDER BY t DESC",
                scaling,
                (rows) => {
                    var chart_data = (GroupingLineChartRowFn(scaling, timezoneoffset, "Hack 结果"))(rows);
                    chart_data.Data.Datasets = chart_data.Data.Datasets.Select(d => {
                        var val = Convert.ToInt32(d.Label);
                        d.Label = ((HackResult)val).ToString();
                        return d;
                    })
                    .ToList();
                    return chart_data;
                },
                token
            ));
        }
    }
}
