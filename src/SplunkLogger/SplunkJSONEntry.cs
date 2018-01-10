
namespace Splunk
{
    /// <summary>
    /// This class represents a HEC Json log entry.
    /// </summary>
    public class SplunkJSONEntry
    {
        /// <summary>
        /// Text to be used at event.
        /// </summary>
        public string @event { get; private set; }

        /// <summary>
        /// Event epoch time.
        /// </summary>
        public ulong? time { get; private set; }

        /// <summary>
        /// Host entry value.
        /// </summary>
        public string host { get; private set; }

        /// <summary>
        /// Source entry value.
        /// </summary>
        public string source { get; private set; }

        /// <summary>
        /// Sourcetype entry value.
        /// </summary>
        public string sourcetype { get; private set; }

        /// <summary>
        /// Index entry value.
        /// </summary>
        public string index { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Splunk.SplunkJSONEntry"/> class.
        /// </summary>
        /// <param name="eventText">Text to be used at event.</param>
        /// <param name="time">Event epoch time.</param>
        /// <param name="host">Host entry value.</param>
        /// <param name="source">Source entry value.</param>
        /// <param name="sourcetype">Sourcetype entry value.</param>
        /// <param name="index">Index entry value.</param>
        public SplunkJSONEntry(string eventText, ulong time = 0, string host = "", string source = "", string sourcetype = "", string index = "")
        {
            @event = eventText;

            if (time > 0)
                this.time = time;

            this.host = host;
            this.source = source;
            this.sourcetype = sourcetype;
            this.index = index;
        }

        /// <summary>
        /// Define if `time` property should be the serialized.
        /// </summary>
        /// <returns><c>true</c>, if should be serialized, <c>false</c> otherwise.</returns>
        public bool ShouldSerializetime() => time.HasValue && time.Value > 0;

        /// <summary>
        /// Define if `host` property should be the serialized.
        /// </summary>
        /// <returns><c>true</c>, if should be serialized, <c>false</c> otherwise.</returns>
        public bool ShouldSerializehost() => !string.IsNullOrWhiteSpace(host);

        /// <summary>
        /// Define if `source` property should be the serialized.
        /// </summary>
        /// <returns><c>true</c>, if should be serialized, <c>false</c> otherwise.</returns>
        public bool ShouldSerializesource() => !string.IsNullOrWhiteSpace(source);

        /// <summary>
        /// Define if `sourcetype` property should be the serialized.
        /// </summary>
        /// <returns><c>true</c>, if should be serialized, <c>false</c> otherwise.</returns>
        public bool ShouldSerializesourcetype() => !string.IsNullOrWhiteSpace(sourcetype);

        /// <summary>
        /// Define if `index` property should be the serialized.
        /// </summary>
        /// <returns><c>true</c>, if should be serialized, <c>false</c> otherwise.</returns>
        public bool ShouldSerializeindex() => !string.IsNullOrWhiteSpace(index);
    }
}