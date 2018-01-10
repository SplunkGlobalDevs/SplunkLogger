using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;

namespace Splunk
{
    /// <summary>
    /// Class used at HEC loggers to control batch process.
    /// </summary>
    public class BatchManager
    {
        readonly ConcurrentBag<object> events;
        readonly uint batchSizeCount;
        readonly uint batchIntervalInMiliseconds;
        readonly Timer timer;
        readonly Action<List<object>> emitAction;

        bool isDisposed;
        bool isDisposing;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Splunk.BatchManager"/> class.
        /// </summary>
        /// <param name="batchSizeCount">Batch size count.</param>
        /// <param name="batchIntervalInMiliseconds">Batch interval in miliseconds.</param>
        /// <param name="emitAction">Emit action to be invoked at Emit process.</param>
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

        /// <summary>
        /// Add the specified item for futher batch emit process.
        /// </summary>
        /// <param name="item">Item to be added for next batch emit process.</param>
        public void Add(object item)
        {
            if (!isDisposed && !isDisposing)
            {
                events.Add(item);
                if (events.Count >= batchSizeCount)
                    Emit();
            }
        }

        /// <summary>
        /// Releases all resource used by the <see cref="T:Splunk.BatchManager"/> object.
        /// </summary>
        /// <remarks>Call <see cref="Dispose"/> when you are finished using the <see cref="T:Splunk.BatchManager"/>. The
        /// <see cref="Dispose"/> method leaves the <see cref="T:Splunk.BatchManager"/> in an unusable state. After
        /// calling <see cref="Dispose"/>, you must release all references to the <see cref="T:Splunk.BatchManager"/> so
        /// the garbage collector can reclaim the memory that the <see cref="T:Splunk.BatchManager"/> was occupying.
        /// </remarks>
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