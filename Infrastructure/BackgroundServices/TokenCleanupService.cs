using Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.BackgroundServices
{
    public class TokenCleanupService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<TokenCleanupService> _logger;

        public TokenCleanupService(IServiceScopeFactory scopeFactory, ILogger<TokenCleanupService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var _repository = scope.ServiceProvider.GetRequiredService<IRefreshTokensRepository>();
                    await _repository.DeleteExpiredAndRevokedAsync();
                    Console.WriteLine("Expired and revoked tokens cleaned up.");
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex, "Error during cleaning tokens");
                }
                await Task.Delay(TimeSpan.FromHours(12), stoppingToken);
            }
        }
    }
}
