using Main.Interfaces;
using Main.Requests;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SearchController : BaseController
    {
        private readonly IManagerService _managerService;
        public SearchController(IManagerService managerService)
        {
            _managerService = managerService;
        }


        [HttpPost("search")]
        public async Task<IActionResult> Search([FromBody] SearchReq req)
        {
            var response = await _managerService.SearchAsync(req);
            return HandleResponse(response);
        }
    }
}
