using GHLearning.GrpcHealthChecks.GrpcService;
using Microsoft.AspNetCore.Mvc;

namespace GHLearning.GrpcHealthChecks.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class GreeterController(
	Greeter.GreeterClient client) : ControllerBase
{
	[HttpGet("SayHello")]
	public async Task<string> SayHelloAsync([FromQuery] string name)
		=> (await client.SayHelloAsync(new HelloRequest { Name = name }, cancellationToken: HttpContext.RequestAborted).ConfigureAwait(false)).Message;
}
