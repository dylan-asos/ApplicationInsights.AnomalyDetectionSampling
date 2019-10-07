using ApplicationInsights.AnomalyDetectionSampling.Settings;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.WindowsServer.Channel.Implementation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace ApplicationInsights.AnomalyDetectionSampling
{
   public static class TelemetryProcessorChainBuilderExtensions
   {
      public static void UseAnomalyDetectionSampling(this IApplicationBuilder app, AnomalyDetectionSettings detectionSettings)
      {
         var configuration       = app.ApplicationServices.GetRequiredService<TelemetryConfiguration>();
         var httpContextAccessor = app.ApplicationServices.GetService<IHttpContextAccessor>();

         var settings = new SamplingPercentageEstimatorSettings { MaxTelemetryItemsPerSecond = detectionSettings.MaximumTelemetryItemsPerSecond };

         configuration.TelemetryProcessorChainBuilder.Use(next => new AnomalySamplingTelemetryProcessor(detectionSettings, settings, httpContextAccessor, next));
         configuration.TelemetryProcessorChainBuilder.Build();
      }
   }
}