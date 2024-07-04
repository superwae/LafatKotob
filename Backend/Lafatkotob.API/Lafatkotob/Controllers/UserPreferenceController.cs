using Lafatkotob.Services.UserPreferenceService;
using Lafatkotob.ViewModels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lafatkotob.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class UserPreferenceController : Controller
    {
        private readonly IUserPreferenceService _userPreferenceService;
        public UserPreferenceController(IUserPreferenceService userPreferenceService)
        {
            _userPreferenceService = userPreferenceService;
        }

        [HttpGet("getall")]
        public async Task<IActionResult> GetAllUserPreferences()
        {
            var userPreferences = await _userPreferenceService.GetAll();
            if(userPreferences == null) return BadRequest();
            return Ok(userPreferences);
        }

        [HttpGet("getbyid")]
        public async Task<IActionResult> GetUserPreferenceById(int userPreferenceId)
        {
            var userPreference = await _userPreferenceService.GetById(userPreferenceId);
            if (userPreference == null) return BadRequest();
            return Ok(userPreference);
        }

        [HttpPost("post")]
        public async Task<IActionResult> PostUserPreferences([FromBody] List<UserPreferenceModel> models)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _userPreferenceService.PostBatch(models);
            if (response.Success)
            {
                return Ok();
            }
            else
            {
                return BadRequest(response.Message);
            }
        }




        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteUserPreference(int userPreferenceId)
        {
            var userPreference = await _userPreferenceService.Delete(userPreferenceId);
            if (userPreference == null) return BadRequest();
            return Ok(userPreference);
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateUserPreference(UserPreferenceModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            await _userPreferenceService.Update(model);
            return Ok();
        }
       
    }
}
