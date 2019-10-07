using System;
using ApplicationInsights.AnomalyDetectionSampling;
using ApplicationInsights.AnomalyDetectionSampling.Settings;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Test.Web.Api
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
            services.AddApplicationInsightsTelemetry(new ApplicationInsightsServiceOptions()
            {
                InstrumentationKey = "your-app-insights-key",
                EnableAdaptiveSampling = false,
                DeveloperMode = true,
            });


            services.AddSingleton<EndpointAggregateMetric>();

            services.AddMvc(options => options.Filters.Add<EndpointTimingMetricFilter>()
               ).SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            
            app.UseHttpsRedirection();
            app.UseMvc();

            // Just a demo of the settings object, we'll default these to sensible values...
            var anomalyDetectionSettings = new AnomalyDetectionSettings
            {
               ExceptionSamplingMode       = ExceptionSamplingMode.NeverSample,
               CustomEventNonSamplingRules = { new NonSamplingRule<EventTelemetry>("Test Event", telemetry => telemetry.Name.Contains("test-event") || telemetry.Name.Contains("some-critical-event"))}
            };
           
            app.UseAnomalyDetectionSampling(anomalyDetectionSettings);
        }
    }
}
