using System;
using System.Net.Http;
using Microsoft.Extensions.Logging;
               
namespace Splunk.Loggers
{
    public abstract class HECBaseLogger : BaseLogger
    {
        protected readonly BatchManager batchManager;

        readonly HttpClient httpClient;

        public HECBaseLogger(string categoryName, LogLevel threshold, HttpClient httpClient, BatchManager batchManager, ILoggerFormatter loggerFormatter)
            : base(categoryName, threshold, loggerFormatter)
        {
            this.httpClient = httpClient;
            this.batchManager = batchManager;
        }
    }
}