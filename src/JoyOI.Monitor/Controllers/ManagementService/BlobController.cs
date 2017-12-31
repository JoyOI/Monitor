using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace JoyOI.Monitor.Controllers.ManagementService
{
    public class BlobController : ChartController
    {
        // GET: /<controller>/
        public IActionResult Index()
        {
            return View();
        }
    }
}
