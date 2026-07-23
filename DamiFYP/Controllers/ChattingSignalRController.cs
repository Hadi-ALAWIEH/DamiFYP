using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.AspNetCore.Mvc;
namespace DamiFYP.Controllers;

[ApiController]
[Route("api/[controller]/")]
public class ChattingSignalRController : ControllerBase
{
    private readonly IHub _hub;

    public ChattingSignalRController(IHub hub)
    {
        _hub = hub;
    }

    [HttpGet]
    public async Task DoSomething()
    {
    }
}
