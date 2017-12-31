using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using JoyOI.Monitor.Models;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace JoyOI.Monitor.Controllers.UserCenter
{
    
    [Route("/UserCenter/UserRegister")]
    public class UserRegisterController : ChartController
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
            return Json(await GetChartData(
                USERCENTER,
                @"SELECT 
                  FLOOR(UNIX_TIMESTAMP(RegisterTime) / @interval) * @interval as t,  
                  Count(Id) as c  
                  FROM joyoi_uc.aspnetusers 
                  GROUP BY t 
                  HAVING t >= @start AND t <= @end 
                  ORDER BY t DESC 
                  LIMIT 0, @points",
                scaling,
                this.DefaultLintChartRowFn(scaling, timezoneoffset, "新用户注册"),
                token
            ));
        }
    }
}
