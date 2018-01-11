using System;
using System.ComponentModel;
using System.Timers;

namespace Vtex.SplunkLogger
{
    /// <summary>
    /// This class contains all methods and logics necessary to control a timer that should
    /// aways be triggered when minute reach 00 seconds.
    /// </summary>
    /// <remarks>
    /// OnMinuteClockTimer is used to always triggers at same time.
    /// </remarks>
    class OnMinuteClockTimer
    {
        Timer SummarizerTimer;
        int lastExecutedMinute = -1;

        [Category("Behavior")]
        [TimersDescription("TimerIntervalElapsed")]
        internal event ElapsedEventHandler Elapsed = delegate { };

        internal OnMinuteClockTimer()
        {
            SummarizerTimer = new Timer();
            SummarizerTimer.Elapsed += SummarizerTimer_Elapsed;
        }

        internal void Start()
        {
            SummarizerTimer.Interval = GetTimerInterval();
            SummarizerTimer.Start();
        }

        internal void Stop()
        {
            SummarizerTimer.Stop();
        }

        void SummarizerTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            var currentMinute = DateTime.Now.Minute;
            if (currentMinute != lastExecutedMinute)
            {
                Stop();
                Elapsed(sender, e);
                lastExecutedMinute = currentMinute;
                Start();
            }
        }

        double GetTimerInterval()
        {
            var dateTimeNow = DateTime.Now;
            var nextTickDateTime = dateTimeNow.AddMinutes(1).RemoveSecondMiliSecond();
            return (nextTickDateTime - dateTimeNow).TotalMilliseconds;
        }
    }
}