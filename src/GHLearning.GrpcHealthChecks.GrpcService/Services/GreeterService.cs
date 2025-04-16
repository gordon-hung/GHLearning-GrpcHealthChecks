using Grpc.Core;

namespace GHLearning.GrpcHealthChecks.GrpcService.Services;

public class GreeterService(ILogger<GreeterService> logger) : Greeter.GreeterBase
{
	public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
	{
		logger.LogInformation("Received request: {Request}", request);

		return Task.FromResult(new HelloReply
		{
			Message = "Hello " + request.Name
		});
	}
}
