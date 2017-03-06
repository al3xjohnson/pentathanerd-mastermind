using System;
using System.Timers;

namespace Pentathanerd.Mastermind
{
    internal class TimerExtension : Timer
    {
        private DateTime _endTime;
        private DateTime _stopTime;

        public double SecondsLeft
        {
            get { return (_endTime - DateTime.Now).TotalSeconds; }
        }

        public double IntervalInSeconds
        {
            get { return Interval / 1000; }
        }

        public double TimeRemainingWhenStopped
        {
            get { return (_endTime - _stopTime).TotalSeconds; }
        }

        public DateTime EndTime
        {
            get { return _endTime; }
        }

        public TimerExtension()
        {
            Elapsed += OnElapsed;
        }

        public TimerExtension(double interval) : this()
        {
            Interval = interval;
        }

        public TimerExtension(DateTime endTime) : this()
        {
            _endTime = endTime;
        }

        private void OnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            if (AutoReset)
            {
                _endTime = DateTime.Now.AddMilliseconds(Interval);
            }
        }
        public new void Dispose()
        {
            Elapsed -= OnElapsed;
            base.Dispose();
        }

        public new void Start()
        {
            if (_endTime == default(DateTime))
            {
                _endTime = DateTime.Now.AddMilliseconds(Interval);
            }
            {
                Interval = (_endTime - DateTime.Now).TotalMilliseconds;
            }
            _stopTime = DateTime.Now;
            base.Start();
        }

        public new void Stop()
        {
            _stopTime = DateTime.Now;
            _endTime = default(DateTime);
            base.Stop();
        }
    }
}