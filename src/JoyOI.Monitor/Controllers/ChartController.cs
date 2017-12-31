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

namespace JoyOI.Monitor.Controllers
{
    public class ChartController : BaseController
    {

        [NonAction]
        public async Task<Chart> GetChartData(
            string datasource,
            string sql,
            ChartScaling scale,
            Func<List<Dictionary<string, object>>, Chart> proc_rows
        )
        {
            var query_data = new List<Dictionary<string, object>>();
            using (var conn = new MySqlConnection(Startup.Config["Datasource:" + datasource]))
            {
                await conn.OpenAsync();
                using (var cmd = new MySqlCommand(sql , conn))
                {
                    cmd.Parameters.Add(new MySqlParameter("points", scale.Points));
                    cmd.Parameters.Add(new MySqlParameter("interval", scale.Interval));
                    cmd.Parameters.Add(new MySqlParameter("start", scale.Start));
                    cmd.Parameters.Add(new MySqlParameter("end", scale.End));

                    using (var dr = await cmd.ExecuteReaderAsync())
                    {
                        while (await dr.ReadAsync()) {
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
        [NonAction]
        public string ConvertTime(long t, int timezoneoffset)
        {
            // First make a System.DateTime equivalent to the UNIX Epoch.
            System.DateTime dateTime = new System.DateTime(1970, 1, 1, 0, 0, 0, 0);

            var offset_sec = timezoneoffset * 60;

            // Add the number of seconds in UNIX timestamp to be converted.
            dateTime = dateTime.AddSeconds(t - offset_sec);

            return dateTime.ToString();
        }
        [NonAction]
        public List<Tuple<Int64, double>> FillMissingAndSort(List<Tuple<Int64, double>> rows, ChartScaling scaling) {
            var rows_dict = rows.ToDictionary(t => t.Item1, t => t.Item2);
            int end = scaling.End - (scaling.End % scaling.Interval);
            while (end > scaling.Start) {
                if (!rows_dict.ContainsKey(end)) {
                    rows.Add(Tuple.Create((long)end, (double)0));
                }
                end -= scaling.Interval;
            }
            return rows.OrderByDescending(t => t.Item1).ToList();
        }
        [NonAction]
        public static String HexColor(System.Drawing.Color c)
        {
            return "#" + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");
        }
        [NonAction]
        public static Object TimeScaleOption()
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
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
    }
}
