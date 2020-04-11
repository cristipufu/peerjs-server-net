using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PeerJs
{
    public class ZombieConnectionsBackgroundTask : TimedBackgroundTask
    {
        private readonly IRealm _realm;
        private readonly ILogger<ExpiredMessagesBackgroundTask> _logger;
        private const int DefaultCheckInterval = 300;

        public ZombieConnectionsBackgroundTask(
            IRealm realm,
            ILogger<ExpiredMessagesBackgroundTask> logger)
            : base(TimeSpan.FromSeconds(DefaultCheckInterval))
        {
            _logger = logger;
            _realm = realm;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var clientIds = _realm.GetClientIds();

            var now = DateTime.UtcNow;
            var aliveTimeout = TimeSpan.FromSeconds(60);

            var count = 0;

            foreach (var clientId in clientIds)
            {
                var client = _realm.GetClient(clientId);
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
                    _realm.ClearMessageQueue(clientId);
                    _realm.RemoveClientById(clientId);

                    socket?.Dispose();
                }

                count++;
            }

            _logger.LogInformation($"Pruned zombie connections for {count} peers.");
        }
    }
}
