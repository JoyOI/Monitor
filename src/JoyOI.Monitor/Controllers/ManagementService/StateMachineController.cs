using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

using JoyOI.Monitor.Models;

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
                "ORDER BY t DESC",
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
                      }
                  };
              }
            ));
        }
    }
}
