using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using JoyOI.Monitor.Models;

namespace JoyOI.Monitor.Controllers.ManagementService
{
    [Route("/ManagementService/Blob")]
    public class BlobController : ChartController
    {
        [HttpGet("Created")]
        public async Task<IActionResult> Created(int start, int end, int interval, int timezoneoffset, CancellationToken token)
        {
            if (start == 0 || end == 0 || interval == 0)
            {
                Response.StatusCode = 400;
                return Json(null);
            }
            var scaling = new ChartScaling(start, end, interval);
            return Json(await GetChartData(
                Mgmtsvc,
                @"SELECT 
                  FLOOR(UNIX_TIMESTAMP(CreateTime) / @interval) * @interval as t,  
                  Count(Id) as c  
                  FROM joyoi_mgmtsvc.blobs 
                  GROUP BY t 
                  HAVING t >= @start AND t <= @end 
                  ORDER BY t DESC 
                  LIMIT 0, @points",
                scaling,
                this.DefaultLineChartRowFn(scaling, timezoneoffset, "新建的二进制数据"),
                token
            ));
        }
        [HttpGet("Size")]
        public async Task<IActionResult> Size(int start, int end, int interval, CancellationToken token)
        {
            if (start == 0 || end == 0 || interval == 0)
            {
                Response.StatusCode = 400;
                return Json(null);
            }
            var scaling = new ChartScaling(start, end, interval);
            return Json(await GetChartData(
                Mgmtsvc,
                @"SELECT 
                  FLOOR(OCTET_LENGTH(body) / 1024) * 1024 as s,
                  Count(Id) as c 
                  FROM joyoi_mgmtsvc.blobs 
                  WHERE UNIX_TIMESTAMP(CreateTime) >= @start AND UNIX_TIMESTAMP(CreateTime) <= @end 
                  GROUP BY s",
                scaling,
              (rows) =>
              {
                  var tiers = new List<int>() { 1024, 1024 * 1024, 1024 * 1024 * 10 };
                  var buckets = Enumerable.Repeat((double)0, tiers.Count + 1).ToList();
                  var rows_tuple =
                    rows.Select(d => (Convert.ToInt64(d["s"]), Convert.ToDouble(d["c"])));
                  foreach ((Int64, double) t in rows_tuple) {
                      int bucket_idx = 0;
                      foreach (int tier in tiers) {
                          if (t.Item1 < tier) {
                              buckets[bucket_idx] += t.Item1;
                              goto NEXT;
                          }
                          bucket_idx++;
                      }
                      buckets[bucket_idx] += t.Item1;
                      NEXT:;
                  }
                  var labels = tiers.Select(x => x.ToString()).ToList();
                  labels.Add(">10M");
                  return new Chart
                  {
                      Title = "二进制数据大小",
                      Type = "bar",
                      Data = new ChartData
                      {
                          Labels = labels,
                          Datasets = new List<ChartDataSet>() {
                              new ChartDataSet {
                                  Data = buckets,
                                  BackgroundColor = RandomColorHex()
                              }
                          }
                      }
                  };
              },
              token
            ));
        }
    }
}
