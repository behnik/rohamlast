using Microsoft.AspNetCore.Mvc;
using Roham.Services.Services;

namespace Roham.Services.Controllers;

[Route("api/[controller]")]
[ApiController]
public class LocksController(LocksService _locksService) : ControllerBase
{
    [HttpGet("check")]
    public IActionResult Check() => Ok(_locksService.CheckLock());
}
