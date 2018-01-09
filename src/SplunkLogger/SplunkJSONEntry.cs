using System;
namespace Splunk
{
    public class SplunkJSONEntry
    {
        public string @event { get; private set; }
        public ulong? time { get; private set; }
        public string host { get; private set; }
        public string source { get; private set; }
        public string sourcetype { get; private set; }
        public string index { get; private set; }

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

        public bool ShouldSerializetime() => time.HasValue && time.Value > 0;
        public bool ShouldSerializehost() => !string.IsNullOrWhiteSpace(host);
        public bool ShouldSerializesource() => !string.IsNullOrWhiteSpace(source);
        public bool ShouldSerializesourcetype() => !string.IsNullOrWhiteSpace(sourcetype);
        public bool ShouldSerializeindex() => !string.IsNullOrWhiteSpace(index);
    }
}