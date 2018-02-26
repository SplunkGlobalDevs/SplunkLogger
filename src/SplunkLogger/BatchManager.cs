using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Splunk
{
    /// <summary>
    /// This class contains all methods and logics necessary to control a batch process.
    /// </summary>
    /// <remarks>
    /// Batch process is necessary to improve Splunk HEC performance. You need to dose
    /// your own batch size and/or interval speed but it's much better than send
    /// individual POST for each log entry.
    /// </remarks>
    public class BatchManager
    {
        readonly ConcurrentBag<object> events;
        readonly uint batchSizeCount;
        readonly Timer timer;
        readonly Action<List<object>> emitAction;

        bool isDisposed;
        bool isDisposing;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Splunk.BatchManager"/> class.
        /// </summary>
        /// <param name="batchSizeCount">Batch size count.</param>
        /// <param name="batchIntervalInMilliseconds">Batch interval in milliseconds.</param>
        /// <param name="emitAction">Emit action to be invoked at Emit process.</param>
        public BatchManager(uint batchSizeCount, int batchIntervalInMilliseconds, Action<List<object>> emitAction)
        {
            events = new ConcurrentBag<object>();
            this.batchSizeCount = batchSizeCount;

            if (batchIntervalInMilliseconds > 0)
                timer = new Timer(EmitTimeChek, null, 0, batchIntervalInMilliseconds);

            this.emitAction = emitAction;
        }

        void EmitTimeChek(object state)
        {
            if (events.Count > 0)
                Emit();
        }

        void Emit()
        {
            Task.Factory.StartNew(() => {
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
            });
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