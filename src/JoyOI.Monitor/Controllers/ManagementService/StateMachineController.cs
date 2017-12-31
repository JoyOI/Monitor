using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

using JoyOI.Monitor.Models;
using System.Threading;

namespace JoyOI.Monitor.Controllers.ManagementService
{
    [Route("/ManagementService/StateMachine")]
    public class StateMachineController : ChartController
    {
        [HttpGet("Created")]
        public async Task<IActionResult> Created(int start, int end, int interval, int timezoneoffset, CancellationToken token)
        {
            if (start == 0 || end == 0 || interval == 0) {
                Response.StatusCode = 400;
                return Json(null);
            }
            var scaling = new ChartScaling(start, end, interval);
            return Json(await GetChartData(
                MGMTSVC,
                @"SELECT 
                  Name as n, 
                  FLOOR(UNIX_TIMESTAMP(StartTime) / @interval) * @interval as t,  
                  Count(Id) as c
                  FROM joyoi_mgmtsvc.statemachineinstances 
                  WHERE UNIX_TIMESTAMP(StartTime) >= @start AND UNIX_TIMESTAMP(StartTime) <= @end 
                  GROUP BY n, t 
                  HAVING t >= @start AND t <= @end 
                  ORDER BY t DESC",
                scaling,
              (rows) =>
              {
                  var rows_tuple =
                    rows.Select(d => (d["n"].ToString(), Convert.ToInt64(d["t"]), Convert.ToDouble(d["c"])))
                            .Where(t => t.Item2 >= start && t.Item2 <= end);
                  var labels = FillMissingAndSort(
                      rows_tuple
                      .Select(d => d.Item2)
                      .Distinct()
                      .Select(t => (t, 0.0)), scaling
                      )
                      .Select(d => d.Item1);
                  var row_types = rows_tuple.GroupBy(d => d.Item1).Select(g => g.ToList());
                  var datasets = row_types.Select(set_rows_tuple => {
                      var color = RandomColorHex();
                      var data_tuple = set_rows_tuple.Select(t => (t.Item2, t.Item3));
                      data_tuple = FillMissingAndSort(data_tuple, scaling);
                      return new ChartDataSet {
                          BackgroundColor = color,
                          BorderColor = color,
                          Label = set_rows_tuple.First().Item1,
                          Fill = false,
                          Data = data_tuple.Select(d => d.Item2).ToList()
                      };
                  }).ToList();
                  return new Chart
                  {
                      Title = "新建的状态机",
                      Type = "line",
                      Data = new ChartData
                      {
                          Labels = labels.Select(t => ConvertTime(t, timezoneoffset)).ToList(),
                          Datasets = datasets
                      },
                      Options = new {
                          Scales = TimeScaleOption()
                      }
                  };
              },
              token
            ));
        }

        [HttpGet("Lifetime")]
        public async Task<IActionResult> Lifetime(int start, int end, int interval, CancellationToken token)
        {
            if (start == 0 || end == 0 || interval == 0)
            {
                Response.StatusCode = 400;
                return Json(null);
            }
            var scaling = new ChartScaling(start, end, interval);
            return Json(await GetChartData(
                MGMTSVC,
                @"SELECT 
                  Name as n, 
                  FLOOR(UNIX_TIMESTAMP(EndTime) - UNIX_TIMESTAMP(StartTime)) as d,
                  Count(Id) as c 
                  FROM joyoi_mgmtsvc.statemachineinstances 
                  WHERE UNIX_TIMESTAMP(StartTime) >= @start AND UNIX_TIMESTAMP(StartTime) <= @end 
                  GROUP BY d, n ",
                scaling,
              (rows) =>
              {
                  var max_scale = 60;
                  var rows_tuple =
                    rows.Select(d => {
                        Int64 duration = 0;
                        Int64.TryParse(d["d"].ToString(), out duration);
                        return (d["n"].ToString(), 
                                duration,
                                Convert.ToDouble(d["c"]));
                    }).ToList();
                  var overflow = rows_tuple.Any(d => d.Item2 >= 60);
                  var labels = rows_tuple.Select(d => d.Item2).Distinct().Where(d => d < max_scale).ToList();
                  var groups = rows_tuple.GroupBy(d => d.Item1).Select(g => g.ToList()).ToList();
                  var datasets = groups.Select(g => {
                      var val_dir = g.ToDictionary(x => x.Item2, x => x.Item3);
                      var data_val = labels.Select(l => (double)val_dir.GetValueOrDefault(l, 0)).ToList();
                      var color = RandomColorHex();
                      if (overflow) {
                          data_val.Add(g.Where(d => d.Item2 >= max_scale).Select(x => x.Item3).Sum());
                      }
                      return new ChartDataSet
                      {
                          Label = g.First().Item1,
                          Data = data_val,
                          BackgroundColor = color
                      };
                  }).ToList();
                  var str_labels = labels.Select(t => t.ToString() + "s").ToList();
                  if (overflow) {
                      str_labels.Add("≥" + max_scale.ToString() + "s");
                  }
                  return new Chart
                  {
                      Title = "状态机运行时长",
                      Type = "bar",
                      Data = new ChartData
                      {
                          Labels = str_labels,
                          Datasets = datasets
                      }
                  };
              },
              token
            ));
        }
    }
}
