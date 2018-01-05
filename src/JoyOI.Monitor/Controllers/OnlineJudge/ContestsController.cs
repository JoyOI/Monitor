using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using JoyOI.Monitor.Models;
using System.Collections;

namespace JoyOI.Monitor.Controllers.OnlineJudge
{
    
    [Route("/OnlineJudge/Contest")]
    public class ContestsController : ChartController
    {
        [HttpGet("Ongoing")]
        public async Task<IActionResult> Ongoing(int start, int end, int interval, int timezoneoffset, CancellationToken token)
        {
            if (start == 0 || end == 0 || interval == 0)
            {
                Response.StatusCode = 400;
                return Json(null);
            }
            var scaling = new ChartScaling(start, end, interval);
            return Json(await GetChartData(
                Judge,
                @"select FLOOR(UNIX_TIMESTAMP(timepoint) / @interval) * @interval as t, 
                sum(1) AS c from((select FROM_UNIXTIME(@start + divided.x * @interval) as timepoint from(select * from(select(v * 10 + u + 1) x from
                (select 0 v union select 1 union select 2 union select 3 union select 4 union
                select 5 union select 6 union select 7 union select 8 union select 9) A,
                (select 0 u union select 1 union select 2 union select 3 union select 4 union
                select 5 union select 6 union select 7 union select 8 union select 9) B) AS seq) as divided)as times),
                (select begin, duration from contests) as r
                where r.begin >= FROM_UNIXTIME(@start - 86400 * 30)
                and times.timepoint <= FROM_UNIXTIME(@end)
                and (timepoint >= r.begin and timepoint<addtime(r.begin, r.duration)
                or timepoint < r.begin and addtime(timepoint, SEC_TO_TIME(@interval)) > timepoint<addtime(r.begin, r.duration)
                or addtime(timepoint, SEC_TO_TIME(@interval)) > r.begin and addtime(timepoint, SEC_TO_TIME(@interval)) < addtime(r.begin, r.duration))
                group by t
                order by t",
                scaling,
                DefaultLineChartRowFn(scaling, timezoneoffset, "正在进行的比赛"),
                token
            ));
        }

        [HttpGet("Attendee")]
        public async Task<IActionResult> Attende(int start, int end, int interval, int timezoneoffset, CancellationToken token)
        {
            if (start == 0 || end == 0 || interval == 0)
            {
                Response.StatusCode = 400;
                return Json(null);
            }
            var scaling = new ChartScaling(start, end, interval);
            return Json(await GetChartData(
                Judge,
                @"select FLOOR(UNIX_TIMESTAMP(timepoint) / @interval) * @interval as t, 
                sum(r.CachedAttendeeCount) AS c, 
                r.DisableVirtual as n
                from((select FROM_UNIXTIME(@start + divided.x * @interval) as timepoint from(select * from(select(v * 10 + u + 1) x from
                (select 0 v union select 1 union select 2 union select 3 union select 4 union
                select 5 union select 6 union select 7 union select 8 union select 9) A,
                (select 0 u union select 1 union select 2 union select 3 union select 4 union
                select 5 union select 6 union select 7 union select 8 union select 9) B) AS seq) as divided)as times),
                (select begin, duration, CachedAttendeeCount, DisableVirtual from contests) as r
                where r.begin >= FROM_UNIXTIME(@start - 86400 * 30)
                and times.timepoint <= FROM_UNIXTIME(@end)
                and (timepoint >= r.begin and timepoint<addtime(r.begin, r.duration)
                or timepoint < r.begin and addtime(timepoint, SEC_TO_TIME(@interval)) > timepoint<addtime(r.begin, r.duration)
                or addtime(timepoint, SEC_TO_TIME(@interval)) > r.begin and addtime(timepoint, SEC_TO_TIME(@interval)) < addtime(r.begin, r.duration))
                group by t
                order by t",
                scaling,
                (rows) => {
                    var graph = (GroupingLineChartRowFn(scaling, timezoneoffset, "正在比赛的选手"))(rows);
                    graph.Data.Datasets = graph.Data.Datasets.Select(ds => {
                        switch (ds.Label) {
                            case "0":
                                ds.Label = "模拟赛选手";
                                break;
                            case "1":
                                ds.Label = "正式参赛选手";
                                break;
                        }
                        return ds;
                    });
                    return graph;
                },
                token
            ));
        }
    }
}
