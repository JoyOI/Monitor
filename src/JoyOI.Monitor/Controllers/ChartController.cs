using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using MySql.Data.MySqlClient;

using JoyOI.Monitor.Models;
using JoyOI.Monitor.Lib;
using System.Drawing;
using System.Threading;

namespace JoyOI.Monitor.Controllers
{
    public class ChartController : BaseController
    {

        protected const string MGMTSVC = "mgmtsvc";
        protected const string USERCENTER = "uc";
        protected const string JUDGE = "oj";
        protected const string FORUM = "forum";

        protected async Task<Chart> GetChartData(
            string datasource,
            string sql,
            ChartScaling scale,
            Func<IEnumerable<IDictionary<string, object>>, Chart> proc_rows,
            CancellationToken token
        )
        {
            var query_data = new List<Dictionary<string, object>>();
            using (var conn = new MySqlConnection(Startup.Config["Datasource:" + datasource]))
            {
                await conn.OpenAsync(token);
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.Add(new MySqlParameter("points", scale.Points));
                    cmd.Parameters.Add(new MySqlParameter("interval", scale.Interval));
                    cmd.Parameters.Add(new MySqlParameter("start", scale.Start));
                    cmd.Parameters.Add(new MySqlParameter("end", scale.End));

                    using (var dr = await cmd.ExecuteReaderAsync(token))
                    {
                        while (await dr.ReadAsync(token))
                        {
                            var row = new Dictionary<string, object>();
                            for (var i = 0; i < dr.FieldCount; i++)
                            {
                                row.Add(dr.GetName(i), dr.GetValue(i));
                            }
                            query_data.Add(row);
                        }
                    }
                }
            }
            return proc_rows(query_data);
        }
        protected string ConvertTime(long t, int timezoneoffset)
        {
            // First make a System.DateTime equivalent to the UNIX Epoch.
            System.DateTime dateTime = new System.DateTime(1970, 1, 1, 0, 0, 0, 0);

            var offset_sec = timezoneoffset * 60;

            // Add the number of seconds in UNIX timestamp to be converted.
            dateTime = dateTime.AddSeconds(t - offset_sec);

            return dateTime.ToString();
        }
        protected IEnumerable<(long, double)> FillMissingAndSort(IEnumerable<(long, double)> rows, ChartScaling scaling)
        {
            var rows_dict = rows.ToDictionary(t => t.Item1, t => t.Item2);
            var rows_list = rows.ToList();
            int end = scaling.End - (scaling.End % scaling.Interval);
            while (end >= scaling.Start)
            {
                if (!rows_dict.ContainsKey(end))
                {
                    rows_list.Add(((long)end, (double)0));
                }
                end -= scaling.Interval;
            }
            return rows_list.OrderByDescending(t => t.Item1);
        }
        protected static String RandomColorHex()
        {
            var rnd = new Random();
            return "#" + rnd.Next(200).ToString("X2") + rnd.Next(200).ToString("X2") + rnd.Next(200).ToString("X2");
        }
        protected static Object TimeScaleOption()
        {
            return new
            {
                XAxes = new List<Object>()
                {
                    {
                        new {
                            Type = "time",
                            Distribution = "Series"
                        }
                     }}
            };
        }
        protected Func<IEnumerable<IDictionary<string, object>>, Chart> DefaultLineChartRowFn(
            ChartScaling scaling, int timezoneoffset, string title, string color = "#008b00"
        )
        {
            return (rows) =>
            {
                var rows_tuple =
                  rows
                  .Select(d => (Convert.ToInt64(d["t"]), Convert.ToDouble(d["c"])))
                  .Where(t => t.Item1 >= scaling.Start && t.Item1 <= scaling.End);
                rows_tuple = this.FillMissingAndSort(rows_tuple, scaling);
                var labels = rows_tuple.Select(d => d.Item1).ToList();
                var values = rows_tuple.Select(d => d.Item2).ToList();
                var datasets = new List<ChartDataSet>() {
                          new ChartDataSet {
                              Label = title,
                              Data = values,
                              Fill = false,
                              BorderColor = color,
                              BackgroundColor = color,
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
            };
        }
        protected Func<IEnumerable<IDictionary<string, object>>, Chart> GroupingLineChartRowFn(
            ChartScaling scaling, int timezoneoffset, string title
        )
        {
            return (rows) =>
            {
                var rows_tuple =
                  rows.Select(d => (d["n"].ToString(), Convert.ToInt64(d["t"]), Convert.ToDouble(d["c"])))
                          .Where(t => t.Item2 >= scaling.Start && t.Item2 <= scaling.End);
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
                    return new ChartDataSet
                    {
                        BackgroundColor = color,
                        BorderColor = color,
                        Label = set_rows_tuple.First().Item1,
                        Fill = false,
                        Data = data_tuple.Select(d => d.Item2).ToList()
                    };
                }).ToList();
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
            };
        }
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
    }
}
