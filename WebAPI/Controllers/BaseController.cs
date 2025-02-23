using Main.Enums;
using Main.Responses;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BaseController : ControllerBase
    {
        protected IActionResult HandleResponse<T>(ApiResponse<T> response) where T : class
        {
            return response.NotificationType switch
            {
                NotificationType.Success => Ok(response),
                NotificationType.BadRequest => BadRequest(response),
                NotificationType.NotFound => NotFound(response),
                NotificationType.ServerError => StatusCode(500, response),
                _ => Ok(response),
            };
        }
    }
}
