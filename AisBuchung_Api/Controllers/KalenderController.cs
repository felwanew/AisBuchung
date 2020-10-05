using System;
using System.Web;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore;
using AisBuchung_Api.Models;

namespace AisBuchung_Api.Controllers
{
    [Route("kalender")]
    [ApiController]
    public class KalenderController : ControllerBase
    {
        private readonly KalenderModel model;

        [HttpPost]
        public ActionResult<IEnumerable<string>> PostCalendar(CalendarPost calendarPost)
        {
            var result = model.PostCalendar(calendarPost);
            if (result > 0)
            {
                var path = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}/calendar/{calendarPost.name}";
                return Created(path, null);
            }
            else
            {
                return Conflict();
            }
        }

    }
}
