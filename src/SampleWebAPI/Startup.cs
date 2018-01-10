using System;
using System.Net.Http;
using System.Threading;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Splunk;
using Splunk.Configurations;
using VTEX.SampleWebAPI.Logging;

namespace VTEX.SampleWebAPI
{
    public class Startup
    {
        static readonly ILoggerFormatter formatter = new VTEXSplunkLoggerFormatter(Microsoft.Extensions.PlatformAbstractions.PlatformServices.Default.Application.ApplicationVersion, GetHost());

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddDebug();

            var splunkConfiguration = new SplunkLoggerConfiguration()
            {
                HecConfiguration = new HECConfiguration()
                {
                    SplunkCollectorUrl = "https://localhost:8088/services/collector",
                    Token = "753c5a9c-fb59-4da0-9064-947f99dc20ba"
                },
                SocketConfiguration = new SocketConfiguration()
                {
                    HostName = "localhost",
                    Port = 8111
                }
            };

            //loggerFactory.AddHECRawSplunkLogger(splunkConfiguration, null);
            loggerFactory.AddHECRawSplunkLogger(splunkConfiguration, formatter);

            //loggerFactory.AddHECJsonSplunkLogger(splunkConfiguration, null);
            //loggerFactory.AddHECJsonSplunkLogger(splunkConfiguration, formatter);

            //loggerFactory.AddTcpSplunkLogger(splunkConfiguration, null);
            //loggerFactory.AddTcpSplunkLogger(splunkConfiguration, formatter);

            //loggerFactory.AddUdpSplunkLogger(splunkConfiguration, null);
            //loggerFactory.AddUdpSplunkLogger(splunkConfiguration, formatter);

            app.UseMvc();
        }

        /// <summary>
        /// Method created to get AWS EC2 host Id, or set `dev` as host if AWS internal call fails.
        /// </summary>
        static string GetHost()
        {
            string host = string.Empty;
            try
            {
                using (HttpClient httpClient = new HttpClient())
                {
                    TimeSpan timeSpan = new TimeSpan(0, 0, 5);
                    var cancellationTokenSource = new CancellationTokenSource((int)timeSpan.TotalMilliseconds);
                    httpClient.Timeout = timeSpan;
                    httpClient.BaseAddress = new Uri("http://169.254.169.254/latest/meta-data/");
                    host = httpClient
                        .GetAsync("instance-id", cancellationTokenSource.Token)
                        .Result
                        .Content
                        .ReadAsStringAsync()
                        .Result;
                }
            }
            catch
            {
                host = "dev";
            }
            return host;
        }
    }
}