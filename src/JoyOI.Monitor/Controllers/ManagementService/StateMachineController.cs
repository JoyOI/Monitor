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
            return Json(await GetChartData(
                MGMTSVC,
                "SELECT " +
                "FLOOR(UNIX_TIMESTAMP(StartTime) / @interval) * @interval as t, " +
                "Count(Id) as c " +
                "FROM joyoi_mgmtsvc.statemachineinstances " +
                "GROUP BY t " +
                "HAVING t >= @start AND t <= @end " +
                "ORDER BY t DESC",
                new ChartScaling(start, end, interval),
              (rows) => new Chart
              {
                  Title = "新建的状态机",
                  Data = new List<ChartData>() {
                          new ChartData {
                              Data = rows.Select(d => Tuple.Create(
                                  Convert.ToInt64(d["t"].ToString()),
                                  Convert.ToInt32(d["c"].ToString())
                              ))
                              .Where(t => t.Item1 >= start && t.Item1 <= end)
                              .Select(t => new DataPoint
                              {
                                  Timestamp = t.Item1,
                                  Value = t.Item2
                              })
                              .ToList()}
                        }
              }
            ));
        }
    }
}
