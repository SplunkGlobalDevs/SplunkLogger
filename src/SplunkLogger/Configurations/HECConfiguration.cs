
namespace Splunk.Configurations
{
    public class HECConfiguration
    {
        public uint BatchIntervalInMiliseconds { get; set; } = 5000; // 5 seconds
        public uint BatchSizeCount { get; set; } = 10;
        public int DefaultTimeoutInMiliseconds { get; set; } = 10000; // 10 seconds
        public string SplunkCollectorUrl { get; set; }
        public string Token { get; set; }
    }
}