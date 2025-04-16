using Grpc.Core;
using Grpc.Core.Interceptors;

namespace GHLearning.GrpcHealthChecks.WebApi.Interceptors;

public class ErrorHandlerInterceptor() : Interceptor
{
	public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
		TRequest request,
		ClientInterceptorContext<TRequest, TResponse> context,
		AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
	{
		var call = continuation(request, context);

		return new AsyncUnaryCall<TResponse>(
			HandleResponse(call.ResponseAsync),
			call.ResponseHeadersAsync,
			call.GetStatus,
			call.GetTrailers,
			call.Dispose);
	}

	private static async Task<TResponse> HandleResponse<TResponse>(Task<TResponse> inner)
	{
		try
		{
			return await inner.ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			throw new InvalidOperationException("Custom error", ex);
		}
	}
}
