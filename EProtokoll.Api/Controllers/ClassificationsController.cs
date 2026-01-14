using EProtokoll.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EProtokoll.Api.Controllers;

[ApiController]
[Route("api/v1/classifications")]
[Authorize]
public class ClassificationsController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        var values = Enum.GetValues<DocumentClassification>()
            .Select(x => new { Id = (int)x, Name = x.ToString() });
        return Ok(values);
    }
}
