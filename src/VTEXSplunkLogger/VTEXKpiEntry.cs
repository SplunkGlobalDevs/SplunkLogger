using System.Collections.Generic;

namespace Vtex.SplunkLogger
{
    /// <summary>
    /// This class represents a VTEX performance indicator.
    /// </summary>
    public class VTEXKpiEntry
    {
        /// <summary>
        /// Application name.
        /// </summary>
        public string Application { get; private set; }

        /// <summary>
        /// For which account this kpi happened.
        /// </summary>
        public string Account { get; internal set; }

        /// <summary>
        /// Extra parameters that will be represented as `{Key}="{Value}"` entries at Splunk text message.
        /// </summary>
        public Dictionary<string, string> ExtraParameters { get; internal set; }

        /// <summary>
        /// Number of times that performance indicator was called during current minute.
        /// </summary>
        public ulong Count { get; internal set; }

        /// <summary>
        /// Maximum value provided at this performance indicator during current minute.
        /// </summary>
        public float Max { get; internal set; }

        /// <summary>
        /// Minimum value provided at this performance indicator during current minute.
        /// </summary>
        public float Min { get; internal set; }

        /// <summary>
        /// Performance indicator name.
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// Sum of values provided to this performance indicator during current minute.
        /// </summary>
        public float Sum { get; internal set; }

        /// <summary>
        /// Performance indicator value.
        /// </summary>
        public float Value { get; internal set; }

        internal VTEXKpiEntry(string application, string name)
        {
            ExtraParameters = new Dictionary<string, string>();
            Application = application;
            Name = name;
        }

        Dictionary<string, string> GetExtraFields(VTEXKpiEntry entry)
        {
            Dictionary<string, string> extraFields = new Dictionary<string, string>();
            if (entry.ExtraParameters != null && entry.ExtraParameters.Count > 0)
            {
                foreach (var field in entry.ExtraParameters)
                {
                    extraFields.Add(field.Key, field.Value);
                }
            }
            return extraFields;
        }
    }
}