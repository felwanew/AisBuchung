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
    [Route("veranstaltungen")]
    [ApiController]
    public class VeranstaltungenController : ControllerBase
    {
        private readonly VeranstaltungenModel model;

        public VeranstaltungenController()
        {
            
            model = new VeranstaltungenModel();
        }

        [HttpGet]
        public ActionResult<IEnumerable<string>> GetAllEvents()
        {

            return Ok();

            /*
            var query = Request.QueryString.ToUriComponent();
            query = System.Web.HttpUtility.UrlDecode(query);
            var result = model.GetEvents(0, query);
            return Content(result, "application/json");
            */
        }

        [HttpGet("{id}")]
        public ActionResult<IEnumerable<string>> GetAllOrganizers(long id)
        {
            var query = Request.QueryString.ToUriComponent();
            query = System.Web.HttpUtility.UrlDecode(query);
            var result = model.GetEvents(id, query);
            return Content(result, "application/json");
        }

        [HttpPost("{calendar}")]
        public ActionResult<IEnumerable<string>> PostEvent(EventPost eventPost, string calendar)
        {
            var organizerId = 1;
            var result = model.PostEvent(organizerId, calendar, eventPost);
            //authorization
            if (result != null)
            {
                var path = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}/veranstaltungen/{calendar}/{result}";
                return Created(path, null);
            }
            else
            {
                return BadRequest();
            }
        }

        [HttpPut("{calendar}/{uid}")]
        public ActionResult<IEnumerable<string>> PostEvent(EventPost eventPost, string calendar, string uid)
        {
            var organizerId = 1;
            var result = model.PutEvent(organizerId, uid, eventPost);
            //authorization
            if (result)
            {
                return NoContent();
            }
            else
            {
                return NotFound();
            }
        }
    }
    
    
}
