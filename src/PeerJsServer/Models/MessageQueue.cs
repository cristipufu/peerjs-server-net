using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace PeerJsServer
{
    public interface IMessageQueue
    {
        DateTime GetReadTimestamp();

        void Enqueue(Message msg);

        Message Dequeue();

        IEnumerable<Message> GetAll();
    }

    public class MessageQueue : IMessageQueue
    {
        private readonly ConcurrentQueue<Message> _queue;
        private DateTime _readTimestamp;

        public MessageQueue()
        {
            _readTimestamp = DateTime.UtcNow;
            _queue = new ConcurrentQueue<Message>();
        }

        public void Enqueue(Message msg)
        {
            _queue.Enqueue(msg);
        }

        public Message Dequeue()
        {
            if (!_queue.TryDequeue(out var msg))
            {
                return null;
            }

            _readTimestamp = DateTime.UtcNow;

            return msg;
        }

        public IEnumerable<Message> GetAll()
        {
            return _queue.ToArray();
        }

        public DateTime GetReadTimestamp()
        {
            return _readTimestamp;
        }
    }
}