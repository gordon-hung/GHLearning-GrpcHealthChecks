using System.Diagnostics;

using Grpc.Core;

using Grpc.Core.Interceptors;

namespace GHLearning.GrpcHealthChecks.WebApi.Interceptors;

internal class TracingHandlerInterceptor(ActivitySource activitySource) : Interceptor
{
	public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context, AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
	{
		using var activity = activitySource.StartActivity($"Grpc Handle {context.Method.ServiceName}/{context.Method.Name}");
		activity?.SetTag("rpc.service", context.Method.ServiceName);
		activity?.SetTag("rpc.method_type", context.Method.Type);
		activity?.SetTag("rpc.method", context.Method.Name);
		return continuation(request, context);
	}
}