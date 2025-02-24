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
                NotificationTypeEnum.Success => Ok(response),
                NotificationTypeEnum.BadRequest => BadRequest(response),
                NotificationTypeEnum.NotFound => NotFound(response),
                NotificationTypeEnum.ServerError => StatusCode(500, response),
                _ => Ok(response),
            };
        }
    }
}
