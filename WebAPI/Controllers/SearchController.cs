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
        public async Task<IActionResult> Search([FromBody] SearchRequest request)
        {
            var response = await _managerService.SearchAsync(request);
            return HandleResponse(response);
        }
    }
}
