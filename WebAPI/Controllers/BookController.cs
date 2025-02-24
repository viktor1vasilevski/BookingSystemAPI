using Main.Interfaces;
using Main.Requests;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookController : BaseController
    {
        private readonly IManagerService _managerService;
        public BookController(IManagerService managerService)
        {
            _managerService = managerService;
        }

        [HttpPost("book")]
        public IActionResult Book([FromBody] BookRequest request)
        {
            var response = _managerService.Book(request);
            return HandleResponse(response);
        }
    }
}
