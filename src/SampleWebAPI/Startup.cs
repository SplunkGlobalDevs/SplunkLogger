using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Splunk.Configurations;

namespace VTEX.SampleWebAPI
{
    public class Startup
    {
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

            app.UseMvc();
        }
    }
}