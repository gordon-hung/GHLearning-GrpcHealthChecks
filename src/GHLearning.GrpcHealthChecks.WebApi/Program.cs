using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services
	.AddRouting(options => options.LowercaseUrls = true)
	.AddControllers(options =>
	{
		options.Filters.Add(new ProducesAttribute(MediaTypeNames.Application.Json));
		options.Filters.Add(new ConsumesAttribute(MediaTypeNames.Application.Json));
	})
	.AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddWebApi((options, sp) => options.BaseUrl = builder.Configuration.GetValue<Uri>("GrpcService")!);

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

builder.Services.AddHealthChecks()
		.AddCheck("self", () => HealthCheckResult.Healthy(), tags: ["live"]);

//Learn more about configuring HealthChecks at https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks
builder.Services
	.AddHealthChecksUI(setup => setup.AddHealthCheckEndpoint("Basic Health Check", "/healthz"))
	.AddInMemoryStorage();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
	app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "OpenAPI V1"));// swagger/
	app.UseReDoc(options => options.SpecUrl("/openapi/v1.json"));//api-docs/
	app.MapScalarApiReference();//scalar/v1
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UseHealthChecks("/live", new HealthCheckOptions
{
	Predicate = check => check.Tags.Contains("live"),
	ResultStatusCodes =
	{
		[HealthStatus.Healthy] = StatusCodes.Status200OK,
		[HealthStatus.Degraded] = StatusCodes.Status200OK,
		[HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
	}
});
app.UseHealthChecks("/healthz", new HealthCheckOptions
{
	Predicate = _ => true,
	ResponseWriter = (context, report) =>
	{
		context.Response.ContentType = "application/json; charset=utf-8";

		var options = new JsonWriterOptions { Indented = true };

		using var memoryStream = new MemoryStream();
		using (var jsonWriter = new Utf8JsonWriter(memoryStream, options))
		{
			jsonWriter.WriteStartObject();
			jsonWriter.WriteString("Status", report.Status.ToString());
			jsonWriter.WriteString("TotalDuration", report.TotalDuration.ToString());
			jsonWriter.WriteStartObject("Entries");

			foreach (var healthReportEntry in report.Entries)
			{
				jsonWriter.WriteStartObject(healthReportEntry.Key);
				jsonWriter.WriteString("Status", healthReportEntry.Value.Status.ToString());
				jsonWriter.WriteString("Duration", healthReportEntry.Value.Duration.ToString());
				jsonWriter.WriteString("Description", healthReportEntry.Value.Description ?? null);
				jsonWriter.WriteEndObject();
			}

			jsonWriter.WriteEndObject();
			jsonWriter.WriteEndObject();
		}

		return context.Response.WriteAsync(
			Encoding.UTF8.GetString(memoryStream.ToArray()));
	},
	ResultStatusCodes =
	{
		[HealthStatus.Healthy] = StatusCodes.Status200OK,
		[HealthStatus.Degraded] = StatusCodes.Status200OK,
		[HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
	}
});

app.UseHealthChecksUI(config => config.UIPath = "/healthz-ui");

app.Run();
