using Lafatkotob.Services.HistoryService;
using Lafatkotob.ViewModels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lafatkotob.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class HistoryController : Controller
    {
        private readonly IHistoryService _historyService;
        public HistoryController(IHistoryService historyService)
        {
            _historyService = historyService;
        }


        [HttpGet("getall")]
        public async Task<IActionResult> GetAllHistories()
        {
            var histories = await _historyService.GetAll();
            if(histories == null) return BadRequest();
            return Ok(histories);
        }


        [HttpGet("getbyid")]
        public async Task<IActionResult> GetHistoryById(int historyId)
        {
            var history = await _historyService.GetById(historyId);
            if (history == null) return BadRequest();
            return Ok(history);
        }


        [HttpPost("post")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> PostHistory([FromBody]string userId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var result = await _historyService.Post(userId);

            if (result.Success)
            {
                return Ok(new { HistoryId = result.Data }); 
            }
            else
            {
                return BadRequest(result.Message);
            }
        }



        [HttpDelete("delete")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> DeleteHistory(int historyId)
        {
            var history = await _historyService.Delete(historyId);
            if (history == null) return BadRequest();
            return Ok(history);
        }


        [HttpPut("update")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> UpdateHistory(HistoryModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            await _historyService.Update(model);
            return Ok();
        }
       
    }
}
