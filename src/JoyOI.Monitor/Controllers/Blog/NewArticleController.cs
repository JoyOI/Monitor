using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using JoyOI.Monitor.Models;

namespace JoyOI.Monitor.Controllers.Blog
{
    
    [Route("/Blog/NewArticle")]
    public class NewArticleController : ChartController
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
            return Json(await GetData(
                Blog,
                @"SELECT 
                  FLOOR(UNIX_TIMESTAMP(Time) / @interval) * @interval as t,  
                  Count(Id) as c  
                  FROM joyoi_blog.posts 
                  GROUP BY t 
                  HAVING t >= @start AND t <= @end 
                  ORDER BY t DESC 
                  LIMIT 0, @points",
                scaling,
                this.DefaultLineChartRowFn(scaling, timezoneoffset, "新文章"),
                token
            ));
        }
    }
}
