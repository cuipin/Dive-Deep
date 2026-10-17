using Dive_Deep.Services;
using Microsoft.AspNetCore.Mvc;

namespace Dive_Deep.Api.Controllers;

[ApiController]
[Route("api/v1/dive-conditions")]
[Produces("application/json")]
public sealed class DiveConditionsApiController(IDiveConditionsService diveConditions) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(string location, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(location))
        {
            return BadRequest(new { message = "Skriv en lokation i Danmark." });
        }

        var result = await diveConditions.GetConditionsAsync(location, cancellationToken);
        return result.Report is not null
            ? Ok(result.Report)
            : BadRequest(new { message = result.ErrorMessage });
    }
}
