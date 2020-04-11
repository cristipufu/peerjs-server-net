using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace PeerJs
{
    public interface IRealm
    {
        IEnumerable<string> GetClientIds();

        IClient GetClient(string clientId);

        IEnumerable<string> GetClientIdsWithQueue();

        void SetClient(IClient client);

        bool RemoveClientById(string clientId);

        IMessageQueue GetMessageQueueById(string clientId);

        void AddMessageToQueue(string clientId, Message msg);

        void ClearMessageQueue(string clientId);

        string GenerateClientId();
    }

    public class Realm : IRealm
    {
        private readonly ConcurrentDictionary<string, IClient> _clients;
        private readonly ConcurrentDictionary<string, IMessageQueue> _messageQueues;

        public Realm()
        {
            _clients = new ConcurrentDictionary<string, IClient>();
            _messageQueues = new ConcurrentDictionary<string, IMessageQueue>();
        }

        public void AddMessageToQueue(string clientId, Message msg)
        {
            IMessageQueue messageQueue = null;

            if (!_messageQueues.TryGetValue(clientId, out var messageQeueue))
            {
                messageQueue = new MessageQueue();

                _messageQueues.TryAdd(clientId, messageQeueue);
            }

            messageQueue.Enqueue(msg);
        }

        public void ClearMessageQueue(string clientId)
        {
            _messageQueues.TryRemove(clientId, out var _);
        }

        public IMessageQueue GetMessageQueueById(string clientId)
        {
            return _messageQueues.TryGetValue(clientId, out var messageQueue) ? messageQueue : null;
        }

        public IClient GetClient(string clientId)
        {
            return _clients.TryGetValue(clientId, out var client) ? client : null;
        }

        public IEnumerable<string> GetClientIds()
        {
            return _clients.Keys;
        }

        public IEnumerable<string> GetClientIdsWithQueue()
        {
            return _messageQueues.Keys;
        }

        public void SetClient(IClient client)
        {
            _clients.TryAdd(client.GetId(), client);
        }

        public bool RemoveClientById(string clientId)
        {
            return _clients.TryRemove(clientId, out var _);
        }

        public string GenerateClientId()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
