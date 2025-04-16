# GHLearning-GrpcHealthChecks

&#12288;&#12288;Health checks are exposed by an app as a gRPC service. They're typically used with an external monitoring service to check the status of an app. The service can be configured for various real-time monitoring scenarios:

- Health probes can be used by container orchestrators and load balancers to check an app's status. For example, Kubernetes supports gRPC liveness, readiness and startup probes. Kubernetes can be configured to reroute traffic or restart unhealthy - -containers based on gRPC health check results.
- Use of memory, disk, and other physical server resources can be monitored for healthy status.
- Health checks can test an app's dependencies, such as databases and external service endpoints, to confirm availability and normal functioning.

## Additional resources
- [Health checks in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks?view=aspnetcore-9.0)
- [gRPC health checking protocol](https://github.com/grpc/grpc/blob/master/doc/health-checking.md)
- [Grpc.AspNetCore.HealthChecks](https://www.nuget.org/packages/Grpc.AspNetCore.HealthChecks)
- [Grpc.HealthCheck](https://www.nuget.org/packages/Grpc.HealthCheck)
