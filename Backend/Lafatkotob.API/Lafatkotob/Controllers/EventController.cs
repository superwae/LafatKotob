using Lafatkotob.Entities;
using Lafatkotob.Hubs;
using Lafatkotob.Services.BadgeService;
using Lafatkotob.Services.EventService;
using Lafatkotob.ViewModels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.ComponentModel.Design;
using System.Net;

namespace Lafatkotob.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventController : Controller
    {
        private readonly IEventService _EventService;
        private readonly UserManager<AppUser> _userManager;
        private readonly IHubContext<ChatHub> _hubContext;

        public EventController(IEventService EventService, UserManager<AppUser> userManager, IHubContext<ChatHub> hubContext)
        {
            _EventService = EventService;
            _userManager = userManager;
            _hubContext = hubContext;
        }

        [HttpGet("getall")]
        public async Task<IActionResult> GetAllEvent(int pageNumber, int pageSize)
        {
            var badges = await _EventService.GetAll( pageNumber,  pageSize);
            if (badges == null) return BadRequest();
            return Ok(badges);
        }

        [HttpGet("getbyid")]
        public async Task<IActionResult> GetEventById(int EventId)
        {
            var Event = await _EventService.GetById(EventId);
            if (Event == null) return BadRequest();
            return Ok(Event);
        }

        [HttpGet("geteventsbyhostid")]
        public async Task<IActionResult> GetEventsByHostId(string HostId)
        {
            var Event = await _EventService.GetEventsByHostId(HostId);
            if (Event == null) return BadRequest();
            return Ok(Event);
        }

        [HttpGet("GetUserByEvent")]
        public async Task<IActionResult> GetUserByEvent(int eventId)
        {
            var user = await _EventService.GetUserByEvent(eventId);
            if (user == null) return BadRequest();
            return Ok(user);
        }

        [HttpGet("GetEventsByUserId")]
        public async Task<ActionResult<List<EventModel>>> GetEventsByUserId(string userId)
        {
          
           
            var events = await _EventService.GetEventsByUserId(userId);

            if(!events.Success )
            {
                return Ok(events.Message);
            }
            return Ok(events.Data);
        }



        [HttpPost("post")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Premium,Admin")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> PostEvent([FromForm] EventModel model, IFormFile imageFile)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (imageFile != null && !imageFile.ContentType.StartsWith("image/"))
            {
                return BadRequest("Only image files are allowed.");
            }
            var result = await _EventService.Post(model, imageFile);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }
            await _hubContext.Clients.All.SendAsync("EventAdded", new
            {

                id=model.Id,
                eventName = model.EventName,
                description = model.Description,
                dateScheduled = model.DateScheduled,
                location = model.Location,
                hostUserId = model.HostUserId,
                attendances = model.attendances,
                imagePath = model.ImagePath,

            });

            return Ok(result.Data);
        }
        [HttpGet("ByEventCity")]
        public async Task<IActionResult> GetEventsByCity([FromQuery] string city, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (string.IsNullOrWhiteSpace(city))
            {
                return BadRequest("City parameter is required.");
            }

            var result = await _EventService.GetEventsByCity(city, pageNumber, pageSize);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return Ok(result.Data);
        }


        [HttpDelete("DeleteAllRelationAndEvent")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Premium,Admin")]

        public async Task<IActionResult> DeleteAllRelationAndEvent(int id)
        {
            var Event = await _EventService.DeleteAllRelationAndEvent(id);
            if (Event == null) return BadRequest();
            return Ok(Event);

        }

        [HttpGet("GetEventsByUserName")]
        public async Task<IActionResult> GetEventsByUserName([FromQuery] string username, int pageNumber = 1, int pageSize = 20,bool GetRegisterd=true)
        {
            var books = await _EventService.GetBooksByUserName(username, pageNumber, pageSize, GetRegisterd);

            return Ok(books.Data);
        }


        [HttpDelete("delete")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Premium,Admin")]
        public async Task<IActionResult> DeleteEvent(int EventId)
        {
            var Event = await _EventService.Delete(EventId);
            if (Event == null) return BadRequest();
            await _hubContext.Clients.All.SendAsync("EventDeleted", EventId);
            return Ok(Event);
        }

        [HttpPut("update/{eventId}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Premium,Admin")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateEvent(int eventId, [FromForm] EventModel model, IFormFile imageFile)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            if (imageFile != null && !imageFile.ContentType.StartsWith("image/"))
            {
                return BadRequest("Only image files are allowed.");
            }
            var result = await _EventService.Update(eventId, model, imageFile);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }
            await _hubContext.Clients.All.SendAsync("EventUpdated", result.Data);

            return Ok(result.Data);
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchEvents([FromQuery] string query, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest("Search query is required.");
            }

            var result = await _EventService.SearchEvents(query, pageNumber, pageSize);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return Ok(result);
        }


    }
}
