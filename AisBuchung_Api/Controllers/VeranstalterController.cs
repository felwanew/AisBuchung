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
    [Route("veranstalter")]
    [ApiController]
    public class VeranstalterController : ControllerBase
    {
        private readonly VeranstalterModel model;

        public VeranstalterController()
        {

            model = new VeranstalterModel();
        }

        [HttpGet]
        public ActionResult<IEnumerable<string>> GetAllOrganizers()
        {
            var query = Request.QueryString.ToUriComponent();
            query = System.Web.HttpUtility.UrlDecode(query);
            var result = model.GetOrganizers(query);
            return Content(result, "application/json");
        }

        [HttpGet("{id}")]
        public ActionResult<IEnumerable<string>> GetAllOrganizers(long id)
        {
            var result = model.GetOrganizer(id);
            if (result == null)
            {
                return NotFound();
            }
            return Content(result, "application/json");
        }

        [HttpPost]
        public ActionResult<IEnumerable<string>> PostOrganizer(OrganizerPost organizerPost)
        {
            var result = model.PostOrganizer(organizerPost);
            if (result > 0)
            {
                var path = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}/veranstalter/{result}";
                return Created(path, null);
            }
            else
            {
                return Conflict();
            }
        }

        [HttpPut("{id}")]
        public ActionResult<IEnumerable<string>> PutOrganizer(long id, OrganizerPost organizerPost)
        {
            var result = model.PutOrganizer(id, organizerPost);
            if (result)
            {
                var path = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}/veranstalter/{result}";
                return Created(path, null);
            }
            else
            {
                return Conflict();
            }
        }
    }
    
    
}
