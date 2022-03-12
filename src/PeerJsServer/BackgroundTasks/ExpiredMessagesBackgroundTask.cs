using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PeerJs
{
    public class ExpiredMessagesBackgroundTask : TimedBackgroundTask
    {
        private readonly ILogger<ExpiredMessagesBackgroundTask> _logger;

        public ExpiredMessagesBackgroundTask(
            IServiceProvider services,
            ILogger<ExpiredMessagesBackgroundTask> logger)
            : base(services, TimeSpan.FromSeconds(2))
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
                await PruneExpiredMessagesAsync(realm.Value, stoppingToken);
            }
        }

        private async Task PruneExpiredMessagesAsync(IRealm realm, CancellationToken stoppingToken)
        {
            var clientIds = realm.GetClientIdsWithQueue();

            var now = DateTime.UtcNow;
            var maxDiff = TimeSpan.FromSeconds(10);

            var seenMap = new Dictionary<string, bool>();

            foreach (var clientId in clientIds)
            {
                var messageQueue = realm.GetMessageQueueById(clientId);

                if (messageQueue == null)
                {
                    continue;
                }

                var lastReadDiff = now - messageQueue.GetReadTimestamp();

                if (lastReadDiff < maxDiff)
                {
                    continue;
                }

                var messages = messageQueue.GetAll();

                foreach (var message in messages)
                {
                    var seenKey = $"{message.Source}_{message.Destination};";

                    if (!seenMap.TryGetValue(seenKey, out var seen) || !seen)
                    {
                        var sourceClient = realm.GetClient(message.Source);

                        await realm.HandleMessageAsync(sourceClient, Message.Create(MessageType.Expire, string.Empty), stoppingToken);

                        seenMap[seenKey] = true;
                    }
                }

                realm.ClearMessageQueue(clientId);
            }

            _logger.LogInformation("Pruned expired messages for {} peers.", seenMap.Keys.Count);
        }
    }
}