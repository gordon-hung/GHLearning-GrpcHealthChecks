using Grpc.Health.V1;
using Microsoft.AspNetCore.Mvc;
using static Grpc.Health.V1.HealthCheckResponse.Types;

namespace GHLearning.GrpcHealthChecks.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class HealthController(
	Health.HealthClient client) : ControllerBase
{
	[HttpGet("Check")]
	public async Task<ServingStatus> CheckAsync()
		=> (await client.CheckAsync(new HealthCheckRequest(), cancellationToken: HttpContext.RequestAborted).ConfigureAwait(false)).Status;

	[HttpGet("check/greet-greeter")]
	public async Task<ServingStatus> CheckByGreetGreeterAsync()
		=> (await client.CheckAsync(new HealthCheckRequest { Service = "greet.Greeter" }, cancellationToken: HttpContext.RequestAborted).ConfigureAwait(false)).Status;
}
