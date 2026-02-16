using System;
using System.Threading;

namespace SafetySentinel.Services
{
    public class ScanScheduler : IDisposable
    {
        private Timer? _quickScanTimer;
        private Timer? _dailyBriefTimer;
        private bool _running;

        public event Action? OnDailyBriefDue;
        public event Action<string>? OnStatusUpdate;

        public void Start(TimeSpan quickScanInterval, TimeSpan dailyBriefTime)
        {
            Stop();
            _running = true;

            // Quick scan timer
            _quickScanTimer = new Timer(_ =>
            {
                OnStatusUpdate?.Invoke($"Quick scan interval elapsed at {DateTime.Now:HH:mm}");
            }, null, quickScanInterval, quickScanInterval);

            // Daily brief timer — calculate delay until next brief time
            var now = DateTime.Now;
            var nextBrief = now.Date.Add(dailyBriefTime);
            if (nextBrief <= now) nextBrief = nextBrief.AddDays(1);
            var delay = nextBrief - now;

            _dailyBriefTimer = new Timer(_ =>
            {
                OnStatusUpdate?.Invoke($"Daily brief triggered at {DateTime.Now:HH:mm}");
                OnDailyBriefDue?.Invoke();

                // Reset for next day
                _dailyBriefTimer?.Change(TimeSpan.FromHours(24), Timeout.InfiniteTimeSpan);
            }, null, delay, Timeout.InfiniteTimeSpan);

            OnStatusUpdate?.Invoke($"Scheduler started. Next brief at {nextBrief:HH:mm}. Scan every {quickScanInterval.TotalHours}h.");
        }

        public void Stop()
        {
            _running = false;
            _quickScanTimer?.Dispose();
            _dailyBriefTimer?.Dispose();
            _quickScanTimer = null;
            _dailyBriefTimer = null;
        }

        public bool IsRunning => _running;

        public void Dispose()
        {
            Stop();
        }
    }
}
