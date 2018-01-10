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
        static readonly ILoggerFormatter formatter = new VTEXSplunkLoggerFormatter();

        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <returns>The configure.</returns>
        /// <param name="app">App.</param>
        /// <param name="env">Env.</param>
        /// <param name="loggerFactory">Logger factory.</param>
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

            /**************************** Define Your Logger ****************************/
            /*                                                                          */
            //loggerFactory.AddHECRawSplunkLogger(splunkConfiguration, null);           //
            loggerFactory.AddHECRawSplunkLogger(splunkConfiguration, formatter);        //

            //loggerFactory.AddHECJsonSplunkLogger(splunkConfiguration, null);          //
            //loggerFactory.AddHECJsonSplunkLogger(splunkConfiguration, formatter);     //

            //loggerFactory.AddTcpSplunkLogger(splunkConfiguration, null);              //
            //loggerFactory.AddTcpSplunkLogger(splunkConfiguration, formatter);         //

            //loggerFactory.AddUdpSplunkLogger(splunkConfiguration, null);
            //loggerFactory.AddUdpSplunkLogger(splunkConfiguration, formatter);
            /*                                                                          */
            /**************************** Define Your Logger ****************************/

            app.UseMvc();
        }


    }
}