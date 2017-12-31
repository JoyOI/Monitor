using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

using JoyOI.Monitor.Models;
using System.Drawing;

namespace JoyOI.Monitor.Controllers.ManagementService
{
    [Route("/ManagementService/StateMachine")]
    public class StateMachineController : ChartController
    {
        const string MGMTSVC = "mgmtsvc";

        [HttpGet("Created")]
        public async Task<IActionResult> Created(int start, int end, int interval)
        {
            if (start == 0 || end == 0 || interval == 0) {
                Response.StatusCode = 400;
                return Json(null);
            }
            var scaling = new ChartScaling(start, end, interval);
            return Json(await GetChartData(
                MGMTSVC,
                "SELECT " +
                "FLOOR(UNIX_TIMESTAMP(StartTime) / @interval) * @interval as t, " +
                "Count(Id) as c " +
                "FROM joyoi_mgmtsvc.statemachineinstances " +
                "GROUP BY t " +
                "HAVING t >= @start AND t <= @end " +
                "ORDER BY t DESC " +
                "LIMIT 0, @points",
                scaling,
              (rows) =>
              {
                  var rows_tuple =
                    rows.Select(d => Tuple.Create(
                                Convert.ToInt64(d["t"].ToString()),
                                Convert.ToDouble(d["c"].ToString())))
                            .Where(t => t.Item1 >= start && t.Item1 <= end)
                            .ToList();
                  rows_tuple = this.FillMissingAndSort(rows_tuple, scaling);
                  var labels = rows_tuple.Select(d => d.Item1).ToList();
                  var values = rows_tuple.Select(d => d.Item2).ToList();
                  var datasets = new List<ChartDataSet>() {
                          new ChartDataSet {
                              Label = "新建的状态机",
                              Data = values
                        }
                  };
                  return new Chart
                  {
                      Type = "bar",
                      Data = new ChartData
                      {
                          Labels = labels.Select(t => ConvertTime(t)).ToList(),
                          Datasets = datasets
                      },
                      Options = new {
                          Scales = TimeScaleOption()
                      }
                  };
              }
            ));
        }

        [HttpGet("Lifetime")]
        public async Task<IActionResult> Lifetime(int start, int end, int interval)
        {
            if (start == 0 || end == 0 || interval == 0)
            {
                Response.StatusCode = 400;
                return Json(null);
            }
            var scaling = new ChartScaling(start, end, interval);
            return Json(await GetChartData(
                MGMTSVC,
                "SELECT " +
                "Name as n, " +
                "FLOOR(UNIX_TIMESTAMP(EndTime) - UNIX_TIMESTAMP(StartTime)) as d," +
                "Count(Id) as c " +
                "FROM joyoi_mgmtsvc.statemachineinstances " +
                "WHERE UNIX_TIMESTAMP(StartTime) >= @start AND UNIX_TIMESTAMP(StartTime) <= @end " +
                "GROUP BY d, n ",
                scaling,
              (rows) =>
              {
                  var rows_tuple =
                    rows.Select(d => {
                        Int64 duration = 0;
                        Int64.TryParse(d["d"].ToString(), out duration);
                        return Tuple.Create(
                                d["n"].ToString(), duration,
                                Convert.ToDouble(d["c"].ToString()));
                    })
                            .ToList();
                  var labels = rows_tuple.Select(d => d.Item2).Distinct().ToList();
                  var groups = rows_tuple.GroupBy(d => d.Item1).Select(g => g.ToList()).ToList();
                  var datasets = groups.Select(g => {
                      var val_dir = g.ToDictionary(x => x.Item2, x => x.Item3);
                      var data_val = labels.Select(l => (double)val_dir.GetValueOrDefault(l, 0)).ToList();
                      var rnd = new Random();
                      var color = Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));
                      return new ChartDataSet
                      {
                          Label = g.First().Item1,
                          Data = data_val,
                          BackgroundColor = HexColor(color)
                      };
                  }).ToList();
                  return new Chart
                  {
                      Title = "状态机运行时长",
                      Type = "bar",
                      Data = new ChartData
                      {
                          Labels = labels.Select(t => t.ToString() + "s").ToList(),
                          Datasets = datasets
                      }
                  };
              }
            ));
        }
    }
}
