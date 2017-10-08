using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AspNetCoreApp.Services.LongTask;
using Microsoft.AspNetCore.Http;
using System.Diagnostics;
using AspNetCoreApp.Services;

namespace AspNetCoreApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILongTaskService _longTaskService;
        private readonly ILongRespondingService _longRespondingService;
        public HomeController(ILongTaskService longTaskService, ILongRespondingService longRespondingService)
        {
            _longTaskService = longTaskService;
            _longRespondingService = longRespondingService;
        }

        [HttpGet("[action]/{randomVal}")]
        public IActionResult Test(int randomVal)
        {
            // Получаем текущий URI здесь, так как внутри делегата, передаваемого в StartTask, будет уже поздно делать это
            var requetUri = HttpContext.Request.GetCurrentUri();
            var result = _longTaskService.StartTask(async () => await _longRespondingService.DoJob(requetUri));
            return Ok(result);
        }

        [HttpGet("[action]/{id}")]
        public IActionResult GetResult(Guid id)
        {
            try
            {
                return Ok(_longTaskService.CheckResult(id));
            }
            catch (AggregateException exc)
            {
                return StatusCode(500, exc.InnerException.ToString());
            }
            
        }


    }

    
}
