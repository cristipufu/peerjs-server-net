using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PeerJs
{
    public class ExpiredMessagesBackgroundTask : TimedBackgroundTask
    {
        private readonly IRealm _realm;
        private readonly ILogger<ExpiredMessagesBackgroundTask> _logger;

        public ExpiredMessagesBackgroundTask(
            IRealm realm,
            ILogger<ExpiredMessagesBackgroundTask> logger)
            : base(TimeSpan.FromSeconds(2))
        {
            _logger = logger;
            _realm = realm;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var clientIds = _realm.GetClientIdsWithQueue();

            var now = DateTime.UtcNow;
            var maxDiff = TimeSpan.FromSeconds(10);

            var seenMap = new Dictionary<string, bool>();

            foreach(var clientId in clientIds)
            {
                var messageQueue = _realm.GetMessageQueueById(clientId);

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

                foreach(var message in messages)
                {
                    var seenKey = $"{message.Source}_{message.Destination};";

                    if (!seenMap.TryGetValue(seenKey, out var seen) || !seen)
                    {
                        var sourceClient = _realm.GetClient(message.Source);

                        await _realm.HandleMessageAsync(sourceClient, Message.Create(MessageType.Expire, string.Empty), stoppingToken);

                        seenMap[seenKey] = true;
                    }
                }

                _realm.ClearMessageQueue(clientId);
            }

            _logger.LogInformation($"Pruned expired messages for {seenMap.Keys.Count} peers.");
        }
    }
}