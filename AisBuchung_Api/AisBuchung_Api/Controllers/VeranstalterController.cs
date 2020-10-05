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
        private readonly AuthModel auth;

        public VeranstalterController()
        {
            model = new VeranstalterModel();
            auth = new AuthModel();
        }

        [HttpGet]
        public ActionResult<IEnumerable<string>> GetAllOrganizers(LoginPost loginPost)
        {
            if (!auth.CheckIfOrganizerPermissions(loginPost))
            {
                return Unauthorized();
            }

            var query = Request.QueryString.ToUriComponent();
            query = System.Web.HttpUtility.UrlDecode(query);
            var result = model.GetOrganizers(query);
            return Content(result, "application/json");
        }

        [HttpGet("{id}")]
        public ActionResult<IEnumerable<string>> GetOrganizer(LoginPost loginPost, long id)
        {
            if (!auth.CheckIfOrganizerPermissions(loginPost))
            {
                return Unauthorized();
            }

            var result = model.GetOrganizer(id);
            if (result == null)
            {
                return NotFound();
            }
            return Content(result, "application/json");
        }

        [HttpGet("{id}/kalender")]
        public ActionResult<IEnumerable<string>> GetOrganizerCalendars(LoginPost loginPost, long id)
        {
            if (!auth.CheckIfOrganizerPermissions(loginPost))
            {
                return Unauthorized();
            }

            var result = model.GetOrganizerCalendars(id);
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
                return Ok();
            }
            else
            {
                return Conflict();
            }
        }

        [HttpPut("{id}")]
        public ActionResult<IEnumerable<string>> PutOrganizer(long id, OrganizerPost organizerPost)
        {
            //TODO OrganizerPost überprüfen
            if (!auth.CheckIfOrganizerPermissions(organizerPost, id))
            {
                return Unauthorized();
            }

            var result = model.PutOrganizer(id, organizerPost);
            if (result)
            {
                //var path = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}/veranstalter/{result}";
                return NoContent();
            }
            else
            {
                return NotFound();
            }
        }

        [HttpDelete("{id}")]
        public ActionResult<IEnumerable<string>> DeleteOrganizer(LoginPost loginPost, long id)
        {
            if (!auth.CheckIfOrganizerPermissions(loginPost, id))
            {
                return Unauthorized();
            }

            var result = model.DeleteOrganizer(id);
            if (result)
            {
                return NoContent();
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPost("{id}/autorisieren")]
        public ActionResult<IEnumerable<string>> AuthorizeOrganizer(LoginPost loginPost, long id)
        {
            if (!auth.CheckIfOrganizerPermissions(loginPost))
            {
                return Unauthorized();
            }

            var result = model.AuthorizeOrganizer(id);

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
