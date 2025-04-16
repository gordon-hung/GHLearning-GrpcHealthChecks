using GHLearning.GrpcHealthChecks.GrpcService.Services;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();

//AddOpenTelemetry
builder.Services.AddOpenTelemetry()
	.ConfigureResource(resource => resource
	.AddService(builder.Configuration["ServiceName"]!))
	.UseOtlpExporter(OtlpExportProtocol.Grpc, new Uri(builder.Configuration["OtlpEndpointUrl"]!))
	.WithMetrics(metrics => metrics
		.AddMeter("GHLearning.")
		.AddAspNetCoreInstrumentation()
		.AddRuntimeInstrumentation()
		.AddProcessInstrumentation()
		.AddPrometheusExporter())
	.WithTracing(tracing => tracing
		.AddEntityFrameworkCoreInstrumentation()
		.AddHttpClientInstrumentation()
		.AddGrpcClientInstrumentation()
		.AddAspNetCoreInstrumentation(options => options.Filter = (httpContext) => !httpContext.Request.Path.StartsWithSegments("/swagger", StringComparison.OrdinalIgnoreCase) &&
				!httpContext.Request.Path.StartsWithSegments("/live", StringComparison.OrdinalIgnoreCase) &&
				!httpContext.Request.Path.StartsWithSegments("/healthz", StringComparison.OrdinalIgnoreCase) &&
				!httpContext.Request.Path.StartsWithSegments("/metrics", StringComparison.OrdinalIgnoreCase) &&
				!httpContext.Request.Path.StartsWithSegments("/favicon.ico", StringComparison.OrdinalIgnoreCase) &&
				!httpContext.Request.Path.Value!.Equals("/api/events/raw", StringComparison.OrdinalIgnoreCase) &&
				!httpContext.Request.Path.Value!.EndsWith(".js", StringComparison.OrdinalIgnoreCase) &&
				!httpContext.Request.Path.StartsWithSegments("/_vs", StringComparison.OrdinalIgnoreCase)));

builder.Services
	.AddGrpcHealthChecks(options => options.Services.Map("greet.Greeter", r => r.Tags.Contains("greeter")))
	.AddCheck("self", () => HealthCheckResult.Healthy(), ["live", "greeter"]);

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<GreeterService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.MapGrpcHealthChecksService();

app.Run();
