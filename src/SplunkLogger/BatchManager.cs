using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;

namespace Splunk
{
    class BatchManager
    {
        ConcurrentBag<object> events;
        bool isDisposed;
        bool isDisposing;
        uint batchSizeCount;
        uint batchIntervalInMiliseconds;
        Timer timer;
        Action<List<object>> emitAction;

        public BatchManager(uint batchSizeCount, uint batchIntervalInMiliseconds, Action<List<object>> emitAction)
        {
            events = new ConcurrentBag<object>();
            this.batchSizeCount = batchSizeCount;
            this.batchIntervalInMiliseconds = batchIntervalInMiliseconds;

            if (batchIntervalInMiliseconds > 0)
            {
                timer = new Timer(batchIntervalInMiliseconds);
                timer.AutoReset = false;
                timer.Enabled = true;
                timer.Elapsed += TimerTick;
                timer.Start();
            }

            this.emitAction = emitAction;
        }

        void TimerTick(object sender, ElapsedEventArgs e)
        {
            Task
            .Factory
            .StartNew(() =>
            {
                if (events.Count > 0)
                    Emit();
            }).ContinueWith(task =>
            {
                timer?.Start();
            });
        }

        void Emit()
        {
            bool continueExtraction = true;
            List<object> emitEvents = new List<object>();
            while (continueExtraction)
            {
                if (events.Count == 0)
                    continueExtraction = false;
                else
                {
                    events.TryTake(out object item);
                    if (item != null)
                        emitEvents.Add(item);
                    if (events.Count == 0 || emitEvents.Count >= batchSizeCount)
                        continueExtraction = false;
                }
            }
            if (emitEvents.Count > 0)
                emitAction?.Invoke(emitEvents);
        }

        public void Add(object item)
        {
            if (!isDisposed && !isDisposing)
            {
                events.Add(item);
                if (events.Count >= batchSizeCount)
                    Emit();
            }
        }

        public void Dispose()
        {
            if (!isDisposed)
            {
                isDisposing = true;
                while (events.Count != 0)
                {
                    Emit();
                }
                isDisposing = false;
                isDisposed = true;
            }
        }
    }
}