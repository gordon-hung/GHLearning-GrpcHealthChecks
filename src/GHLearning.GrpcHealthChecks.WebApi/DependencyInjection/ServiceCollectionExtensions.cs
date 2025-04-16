using GHLearning.GrpcHealthChecks.GrpcService;
using GHLearning.GrpcHealthChecks.WebApi;
using GHLearning.GrpcHealthChecks.WebApi.Interceptors;
using Grpc.Core;
using Grpc.Health.V1;
using Grpc.Net.Client.Balancer;
using Grpc.Net.Client.Configuration;
using Grpc.Net.ClientFactory;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddWebApi(
		this IServiceCollection services,
		Action<GrpcServiceOptions, IServiceProvider> grpcServiceOptions)
	{
		services.AddOptions<GrpcServiceOptions>().Configure(grpcServiceOptions);

		services.TryAddSingleton<TracingHandlerInterceptor>();

		services.TryAddSingleton<LoggingHandlerInterceptor>();

		services.TryAddSingleton<ErrorHandlerInterceptor>();

		services.AddGrpcServiceClient();

		services.TryAddSingleton<ResolverFactory>(sp => new DnsResolverFactory(refreshInterval: TimeSpan.FromSeconds(30)));

		return services;
	}

	private static IServiceCollection AddGrpcServiceClient(
		this IServiceCollection services)
	{
		var grpcServiceOptions = services.BuildServiceProvider().GetRequiredService<IOptions<GrpcServiceOptions>>().Value;

		var credentials = grpcServiceOptions.BaseUrl.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase)
		   ? ChannelCredentials.SecureSsl
		   : ChannelCredentials.Insecure;

		services.AddGrpcClient<Greeter.GreeterClient>(
			options =>
			{
				options.Address = grpcServiceOptions.BaseUrl;
				options.ChannelOptionsActions.Add(options =>
				{
					options.Credentials = credentials;
					options.ServiceConfig = new ServiceConfig
					{
						MethodConfigs =   {
						new MethodConfig
						{
							Names = { MethodName.Default },
							RetryPolicy = new RetryPolicy
							{
								MaxAttempts = 5,
								InitialBackoff = TimeSpan.FromSeconds(1),
								MaxBackoff = TimeSpan.FromSeconds(5),
								BackoffMultiplier = 1.5,
								RetryableStatusCodes = { StatusCode.Unavailable }
							}
						}
						}
					};
				});
			})
			.AddInterceptor<TracingHandlerInterceptor>(InterceptorScope.Client)
			.AddInterceptor<LoggingHandlerInterceptor>(InterceptorScope.Client)
			.AddInterceptor<ErrorHandlerInterceptor>(InterceptorScope.Client);

		services.AddGrpcClient<Health.HealthClient>(
			options =>
			{
				options.Address = grpcServiceOptions.BaseUrl;
				options.ChannelOptionsActions.Add(options =>
				{
					options.Credentials = credentials;
					options.ServiceConfig = new ServiceConfig
					{
						MethodConfigs =   {
					new MethodConfig
					{
						Names = { MethodName.Default },
						RetryPolicy = new RetryPolicy
						{
							MaxAttempts = 5,
							InitialBackoff = TimeSpan.FromSeconds(1),
							MaxBackoff = TimeSpan.FromSeconds(5),
							BackoffMultiplier = 1.5,
							RetryableStatusCodes = { StatusCode.Unavailable }
						}
					}
						}
					};
				});
			})
			.AddInterceptor<TracingHandlerInterceptor>(InterceptorScope.Client)
			.AddInterceptor<LoggingHandlerInterceptor>(InterceptorScope.Client)
			.AddInterceptor<ErrorHandlerInterceptor>(InterceptorScope.Client);

		return services;
	}
}
