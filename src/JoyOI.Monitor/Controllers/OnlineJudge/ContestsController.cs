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
            return Json(await GetData(
                Judge,
                @"SELECT 
                  UNIX_TIMESTAMP(Begin) AS s,
                  UNIX_TIMESTAMP(ADDTIME(Begin, Duration)) AS e
                  FROM joyoi_oj.contests 
                  WHERE 
                  Begin >= DATE_ADD(FROM_UNIXTIME(@start), interval - 30 day) 
                  AND
                  Begin <  FROM_UNIXTIME(@end)
                  AND
                  ADDTIME(Begin, Duration) > FROM_UNIXTIME(@start)",
                scaling,
                (rows) => {
                    string color = "#008b00";
                    string title = "正在进行的比赛";
                    var rows_tuple = rows
                        .Select(d => (Convert.ToInt64(d["s"]), Convert.ToInt64(d["e"]))).ToList();
                    var labels = FillMissingAndSort(new List<(long, double)>(), scaling).Select(x => x.Item1);
                    var values = labels.Select(l =>
                    {
                        var r_start = l;
                        var r_end = l + scaling.Interval;
                        var count = 0;
                        foreach ((long, long) row in rows_tuple) {
                            if (row.Item2 >= r_start && row.Item1 <= r_end) {
                                count++;
                            }
                        }
                        return count;
                    })
                    .Select(i => (double)i);
                    var datasets = new List<ChartDataSet>()
                    {
                        new ChartDataSet {
                            Label = title,
                            Data = values,
                            Fill = false,
                            BackgroundColor = color,
                            BorderColor = color
                        }
                    };
                    return new Chart {
                        Title = title,
                        Type = "line",
                        Data = new ChartData
                        {
                            Labels = labels.Select(t => ConvertTime(t, timezoneoffset)).ToList(),
                            Datasets = datasets
                        },
                        Options = new
                        {
                            Scales = TimeScaleOption()
                        }
                    };
                },
                token
            ));
        }

        [HttpGet("Attendee")]
        public async Task<IActionResult> Attendee(int start, int end, int interval, int timezoneoffset, CancellationToken token)
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
                  UNIX_TIMESTAMP(Begin) AS s,
                  UNIX_TIMESTAMP(ADDTIME(Begin, Duration)) AS e,
                  CachedAttendeeCount as c,
                  DisableVirtual as nv
                  FROM joyoi_oj.contests 
                  WHERE 
                  Begin >= DATE_ADD(FROM_UNIXTIME(@start), interval - 30 day) 
                  AND
                  Begin <  FROM_UNIXTIME(@end)
                  AND
                  ADDTIME(Begin, Duration) > FROM_UNIXTIME(@start)",
                scaling,
                (rows) => {
                    string title = "正在比赛的选手";
                    var rows_tuple = rows
                        .Select(d => (
                            Convert.ToInt64(d["s"]), Convert.ToInt64(d["e"]),
                            Convert.ToInt64(d["c"]), Convert.ToBoolean(d["nv"])
                        )).ToList();
                    var labels = FillMissingAndSort(new List<(long, double)>(), scaling).Select(x => x.Item1);
                    var virtual_comp = new List<double>();
                    var nvirtual_comp = new List<double>();
                    foreach (var l in labels) {
                        var r_start = l;
                        var r_end = l + scaling.Interval;
                        double v = 0, nv = 0;
                        foreach ((long, long, long, bool) row in rows_tuple)
                        {
                            if (row.Item2 >= r_start && row.Item1 <= r_end)
                            {
                                if (row.Item4)
                                {
                                    // not virtual
                                    nv += row.Item3;
                                }
                                else
                                {
                                    v += row.Item3;
                                }
                            }
                        }
                        nvirtual_comp.Add(nv);
                        virtual_comp.Add(v);
                    }
                    var vcolor = RandomColorHex();
                    var nvcolor = RandomColorHex();
                    var datasets = new List<ChartDataSet>()
                    {
                        new ChartDataSet {
                            Label = "正式参赛选手",
                            Data = nvirtual_comp,
                            Fill = false,
                            BackgroundColor = vcolor,
                            BorderColor = vcolor
                        },
                        new ChartDataSet {
                            Label = "模拟赛选手",
                            Data = virtual_comp,
                            Fill = false,
                            BackgroundColor = nvcolor,
                            BorderColor = nvcolor
                        }
                    };
                    return new Chart
                    {
                        Title = title,
                        Type = "line",
                        Data = new ChartData
                        {
                            Labels = labels.Select(t => ConvertTime(t, timezoneoffset)).ToList(),
                            Datasets = datasets
                        },
                        Options = new
                        {
                            Scales = TimeScaleOption()
                        }
                    };
                },
                token
            ));
        }
    }
}
