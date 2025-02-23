using Main.Hubs;
using Main.Interfaces;
using Main.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CheckStatusController : BaseController
    {
        private readonly IManagerService _managerService;
        public CheckStatusController(IManagerService managerService)
        {
            _managerService = managerService;
        }


        [HttpGet("checkstatus")]
        public async Task<IActionResult> CheckStatus([FromQuery] CheckStatusReq request, [FromServices] IHubContext<BookingHub> hubContext)
        {
            var response = await _managerService.CheckStatusAsync(request);
            return HandleResponse(response);
        }

    }
}
