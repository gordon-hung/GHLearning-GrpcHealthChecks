namespace GHLearning.GrpcHealthChecks.WebApi;

public record GrpcServiceOptions
{
	public Uri BaseUrl { get; set; } = default!;
}
