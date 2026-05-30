using Genius.Atom.Web.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace Genius.Atom.Web.Demo.Controllers;

public sealed class DiagnosticsController : BaseController
{
    [HttpGet("ping")]
    public ActionResult<PingResponse> Ping()
    {
        return Ok(new PingResponse("pong", DateTimeOffset.UtcNow));
    }

    public sealed record PingResponse(string Message, DateTimeOffset ServerTimeUtc);
}
