using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace JoyOI.Monitor.Controllers.UserCenter
{
    [Route("/UserCenter/UserActive")]
    public class UserActiveController : Controller
    {
        [HttpGet("UserActive")]
        public IActionResult UserActive()
        {
            // TODO: 
            return Json(null);
        }
    }
}
