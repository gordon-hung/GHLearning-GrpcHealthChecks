using Grpc.Core;
using Grpc.Core.Interceptors;

namespace GHLearning.GrpcHealthChecks.WebApi.Interceptors;

public class LoggingHandlerInterceptor(ILogger<LoggingHandlerInterceptor> logger) : Interceptor
{
	public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
		TRequest request,
		ClientInterceptorContext<TRequest, TResponse> context,
		AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
	{
		var type = context.Method.Type;
		var method = context.Method.Name;
		try
		{
			logger.LogInformation("LogOn:{logOn} Type/Method: {type} / {method} Request:{request}", DateTime.Now.ToString("o"), type, method, request);

			var response = continuation(request, context);

			logger.LogInformation("LogOn:{logOn} Type/Method: {type} / {method} Response:{response}", DateTime.Now.ToString("o"), type, method, response);
			return response;
		}
		catch
		{
			throw;
		}
	}
}
