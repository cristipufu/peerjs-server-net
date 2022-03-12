using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PeerJs
{
    public class ZombieConnectionsBackgroundTask : TimedBackgroundTask
    {
        private readonly ILogger<ExpiredMessagesBackgroundTask> _logger;
        private const int DefaultCheckInterval = 300;

        public ZombieConnectionsBackgroundTask(
            IServiceProvider services,
            ILogger<ExpiredMessagesBackgroundTask> logger)
            : base(services, TimeSpan.FromSeconds(DefaultCheckInterval))
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var scope = Services.CreateScope();
            var server = scope.ServiceProvider.GetRequiredService<IPeerJsServer>();

            var realms = server.GetRealms();

            foreach (var realm in realms)
            {
                await PruneZombieConnectionsAsync(realm.Value);
            }
        }

        private async Task PruneZombieConnectionsAsync(IRealm realm)
        {
            var clientIds = realm.GetClientIds();

            var now = DateTime.UtcNow;
            var aliveTimeout = TimeSpan.FromSeconds(60);

            var count = 0;

            foreach (var clientId in clientIds)
            {
                var client = realm.GetClient(clientId);
                var timeSinceLastHeartbeat = now - client.GetLastHeartbeat();

                if (timeSinceLastHeartbeat < aliveTimeout)
                {
                    continue;
                }

                var socket = client.GetSocket();

                try
                {
                    await socket?.CloseAsync($"Zombie connection, time since last heartbeat: {timeSinceLastHeartbeat.TotalSeconds}s");
                }
                finally
                {
                    realm.ClearMessageQueue(clientId);
                    realm.RemoveClientById(clientId);

                    socket?.Dispose();
                }

                count++;
            }

            _logger.LogInformation("Pruned zombie connections for {} peers.", count);
        }
    }
}
