using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Vtex.SplunkLogger
{
    /// <summary>
    /// This class contains all methods and logics necessary to control kpi process.
    /// </summary>
    /// <remarks>
    /// MetricManager is used to summarize performance indicators during one minute
    /// and them dispatch those entries to Splunk as a "custom" log.
    /// </remarks>
    class MetricManager
    {
        #region [ Private Constants ]

        internal const string METRIC_SPLIT = "-#-";
        internal const string OPTIONAL_SPLIT = ":#:";
        internal const string DIMENSION_SPLIT = "|#|";

        #endregion

        #region [ Private Kpi Ready Event ]

        event EventHandler<VTEXKpiEntry> kpiReady = delegate { };

        #endregion

        #region [ Private Static Fields ]

        static readonly ConcurrentDictionary<string, Tuple<ulong, float>> items = new ConcurrentDictionary<string, Tuple<ulong, float>>();
        static readonly ConcurrentDictionary<string, float> itemsMax = new ConcurrentDictionary<string, float>();
        static readonly ConcurrentDictionary<string, float> itemsMin = new ConcurrentDictionary<string, float>();

        #endregion

        #region [ Private Fields ]

        readonly OnMinuteClockTimer summarizerTimer;
        readonly ILogger logger;

        #endregion

        #region [ Internal Constructor ]

        internal MetricManager(ILogger logger, EventHandler<VTEXKpiEntry> kpiReady)
        {
            this.logger = logger;

            summarizerTimer = new OnMinuteClockTimer();
            summarizerTimer.Elapsed += SummarizerTimer_Elapsed;
            summarizerTimer.Start();

            this.kpiReady = kpiReady;
        }

        #endregion

        #region [ Internal Methods ]

        internal void RegisterKpi(string application, string kpiName, float kpiValue, string account = "", params Tuple<string, string>[] extraParameters)
        {
            if (string.IsNullOrWhiteSpace(application))
                throw new ArgumentNullException(nameof(application));

            if (string.IsNullOrWhiteSpace(kpiName))
                throw new ArgumentNullException(nameof(kpiName));
            
            var extraFields = new Dictionary<string, string>();

            if (extraParameters != null && extraParameters.Length > 0)
                extraParameters.ToList().ForEach(tuple => extraFields.Add(tuple.Item1, tuple.Item2));

            extraFields.Add("application", application);

            if (!string.IsNullOrWhiteSpace(account))
                extraFields.Add("account", account); 

            RegisterKpi(kpiName, kpiValue, extraFields);
        }

        #endregion

        #region [ Private Methods ]

        void RegisterKpi(string metricName, float metricValue, Dictionary<string, string> extraFields)
        {
            var clonedExtraFields = new Dictionary<string, string>();
            extraFields.All(a => { clonedExtraFields.Add(a.Key, a.Value); return true; });

            string itemKey = GetKey(metricName, clonedExtraFields);
            Tuple<ulong, float> metricItem = new Tuple<ulong, float>(1, metricValue);

            items.AddOrUpdate(itemKey, metricItem, (key, metric) =>
            {
                return new Tuple<ulong, float>(metric.Item1 + metricItem.Item1, metric.Item2 + metricItem.Item2);
            });

            itemsMax.AddOrUpdate(itemKey, metricValue, (key, metric) =>
            {
                if (itemsMax.ContainsKey(itemKey))
                {
                    float oldValue = itemsMax[itemKey];
                    return metricValue > oldValue ? metricValue : oldValue;
                }

                return metric;
            });

            itemsMin.AddOrUpdate(itemKey, metricValue, (key, metric) =>
            {
                if (itemsMin.ContainsKey(itemKey))
                {
                    float oldValue = itemsMin[itemKey];
                    return metricValue < oldValue ? metricValue : oldValue;
                }

                return metric;
            });
        }

        void SummarizerTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (items.Keys.Count > 0)
            {
                List<VTEXKpiEntry> kpiMetrics = new List<VTEXKpiEntry>();
                Parallel.ForEach(items.Keys, key =>
                {
                    if (!string.IsNullOrWhiteSpace(key))
                    {
                        Tuple<ulong, float> valueTuple = null;
                        items.TryRemove(key, out valueTuple);

                        float maxValue = 0;
                        itemsMax.TryRemove(key, out maxValue);

                        float minValue = 0;
                        itemsMin.TryRemove(key, out minValue);

                        var kpiEntry = GenerateEntry(key, valueTuple, maxValue, minValue);
                        kpiReady(logger, kpiEntry);
                    }
                });
            }
        }

        VTEXKpiEntry GenerateEntry(string key, Tuple<ulong, float> valueTuple, float maxValue, float minValue)
        {
            string metricName = string.Empty;
            Dictionary<string, string> extraFields = null;
            RetreiveKeyItems(key, out metricName, out extraFields);
            var account = "";
            var application = "";

            if (extraFields != null && extraFields.Count > 0)
            {
                if (extraFields.ContainsKey("application"))
                {
                    application = extraFields["application"];
                    extraFields.Remove("application");
                }

                if (extraFields.ContainsKey("account"))
                {
                    account = extraFields["account"];
                    extraFields.Remove("account");
                }
            }

            return new VTEXKpiEntry(application, metricName)
            {
                Count = valueTuple.Item1,
                Name = metricName,
                Sum = valueTuple.Item2,
                Max = maxValue,
                Min = minValue,
                Account = account,
                ExtraParameters = extraFields
            };
        }

        void RetreiveKeyItems(string key, out string metricName, out Dictionary<string, string> customFields)
        {
            metricName = string.Empty;
            customFields = null;
            string[] parts = key.Split(new string[] { METRIC_SPLIT }, StringSplitOptions.RemoveEmptyEntries);
            metricName = parts[0];
            if (parts.Length > 1)
            {
                customFields = new Dictionary<string, string>();
                string[] metricParts = parts[1].Split(new string[] { OPTIONAL_SPLIT }, StringSplitOptions.RemoveEmptyEntries);
                string[] dimensionParts = null;
                foreach (string metricPart in metricParts)
                {
                    dimensionParts = metricPart.Split(new string[] { DIMENSION_SPLIT }, StringSplitOptions.RemoveEmptyEntries);
                    if (dimensionParts.Length == 2)
                        customFields.Add(dimensionParts[0], dimensionParts[1]);
                    else
                        customFields.Add(dimensionParts[0], string.Empty);
                }
            }
        }

        string GetKey(string metricName, Dictionary<string, string> extraFields = null)
        {
            if (extraFields != null && extraFields.Count > 0)
            {
                StringBuilder stringBuilder = new StringBuilder();

                foreach (KeyValuePair<string, string> item in extraFields)
                {
                    stringBuilder.AppendFormat("{0}{1}{2}", item.Key, DIMENSION_SPLIT, item.Value);
                    stringBuilder.Append(OPTIONAL_SPLIT);
                }
                string itemString = stringBuilder.ToString();
                itemString = string.Format("{0}{1}{2}", metricName, METRIC_SPLIT, itemString.Remove(itemString.Length - OPTIONAL_SPLIT.Length, OPTIONAL_SPLIT.Length));
                return itemString;
            }

             return metricName;
        }

        #endregion
    }
}