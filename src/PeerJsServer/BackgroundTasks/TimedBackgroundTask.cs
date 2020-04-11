using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PeerJsServer
{
    public abstract class TimedBackgroundTask : IHostedService, IDisposable
    {
        private Timer _timer;
        private TimeSpan _interval;
        private Task _executingTask;
        private readonly CancellationTokenSource _stoppingCts = new CancellationTokenSource();

        public TimedBackgroundTask(TimeSpan interval)
        {
            _interval = interval;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(ExecuteTask, null, _interval, TimeSpan.FromMilliseconds(-1));

            return Task.CompletedTask;
        }

        private void ExecuteTask(object state)
        {
            _timer?.Change(Timeout.Infinite, 0);

            _executingTask = ExecuteTaskAsync(_stoppingCts.Token);
        }

        private async Task ExecuteTaskAsync(CancellationToken stoppingToken)
        {
            await ExecuteAsync(stoppingToken);

            _timer.Change(TimeSpan.FromSeconds(30), TimeSpan.FromMilliseconds(-1));
        }

        protected abstract Task ExecuteAsync(CancellationToken stoppingToken);

        public virtual async Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);

            // Stop called without start
            if (_executingTask == null)
            {
                return;
            }

            try
            {
                // Signal cancellation to the executing method
                _stoppingCts.Cancel();
            }
            finally
            {
                // Wait until the task completes or the stop token triggers
                await Task.WhenAny(_executingTask, Task.Delay(Timeout.Infinite, cancellationToken));
            }

        }

        public void Dispose()
        {
            _stoppingCts.Cancel();
            _timer?.Dispose();
        }
    }
}