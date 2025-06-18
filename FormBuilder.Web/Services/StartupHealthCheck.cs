using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FormBuilder.Web.Services
{
    public class StartupHealthCheck : IHealthCheck
    {
        private volatile bool _isReady;

        public bool IsReady
        {
            get => _isReady;
            set => _isReady = value;
        }

        public Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            if (IsReady)
            {
                return Task.FromResult(HealthCheckResult.Healthy("Application is ready"));
            }

            return Task.FromResult(HealthCheckResult.Unhealthy("Application is starting"));
        }
    }
}
