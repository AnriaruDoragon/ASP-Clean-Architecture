using Microsoft.AspNetCore.Mvc;
using Web.API.Authorization;

namespace Web.API.Controllers.V1;

[ApiController]
[Route("/")]
[Public]
public class DefaultController : Controller
{
    [HttpGet]
    public IActionResult Index() => Ok(new { message = "Hello World!" });
}
