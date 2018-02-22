using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Splunk.Configurations;

namespace Splunk.SampleWebAPI
{
    public class Startup
    {
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
            services.Configure<SplunkLoggerConfiguration>(Configuration.GetSection("Splunk"));
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
            
            /******************************** Define Your Logger *********************************/
            /*                                                                                   */
            // Get Configuration to be used at Logger                                            //
            var splunkLoggerConfiguration = GetSplunkLoggerConfiguration(app);
            //                                                                                   //
            //                       Choose one or more of those loggers                         //
            //                                                                                   //                                                                                  
            loggerFactory.AddHECRawSplunkLogger(splunkLoggerConfiguration);                      //
            //                                                                                   //
            //                                                                                   //
            //loggerFactory.AddHECJsonSplunkLogger(splunkConfiguration);                         //
            //                                                                                   //
            //loggerFactory.AddTcpSplunkLogger(splunkConfiguration);                             //
            //                                                                                   //
            //loggerFactory.AddUdpSplunkLogger(splunkConfiguration);                             //
            /*                                                                                   */
            /******************************** Define Your Logger *********************************/

            app.UseMvc();
        }

        /// <summary>
        /// Demonstrate how can you provide configuration to your splunk logger addapter(s) 
        /// </summary>
        SplunkLoggerConfiguration GetSplunkLoggerConfiguration(IApplicationBuilder app)
        {
            SplunkLoggerConfiguration result = null;

            //Retrieving Splunk configuration from appsettings json configuration file
            var splunkLoggerConfigurationOption = app.ApplicationServices.GetService<IOptions<SplunkLoggerConfiguration>>();
            if(splunkLoggerConfigurationOption != null && splunkLoggerConfigurationOption.Value != null)
                result = app.ApplicationServices.GetService<IOptions<SplunkLoggerConfiguration>>().Value;

            //You can also provide a hard code configuration
            //result = new SplunkLoggerConfiguration()
            //{
            //    HecConfiguration = new HECConfiguration()
            //    {
            //        SplunkCollectorUrl = "https://localhost:8088/services/collector",
            //        BatchIntervalInMiliseconds = 5000,
            //        BatchSizeCount = 100,
            //        ChannelIdType = HECConfiguration.ChannelIdOption.None,
                    
            //        Token = "753c5a9c-fb59-4da0-9064-947f99dc20ba"
            //    },
            //    SocketConfiguration = new SocketConfiguration()
            //    {
            //        HostName = "localhost",
            //        Port = 8111
            //    }
            //};
            return result;
        }
    }
}