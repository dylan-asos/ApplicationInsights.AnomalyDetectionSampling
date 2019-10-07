using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using ApplicationInsights.AnomalyDetectionSampling.Settings;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.WindowsServer.Channel.Implementation;
using Microsoft.ApplicationInsights.WindowsServer.TelemetryChannel;
using Microsoft.AspNetCore.Http;

namespace ApplicationInsights.AnomalyDetectionSampling
{
   public class AnomalySamplingTelemetryProcessor : AdaptiveSamplingTelemetryProcessor, ITelemetryProcessor
   {
      private readonly IHttpContextAccessor _httpContextAccessor;

      private static ConditionalWeakTable<HttpContext, SamplingCollection> HttpContextTelemetryDictionary { get; } =
         new ConditionalWeakTable<HttpContext, SamplingCollection>();

      public AnomalyDetectionSettings Settings { get; }

      public ITelemetryProcessor SkipSamplingTelemetryProcessor { get; }

      public AnomalySamplingTelemetryProcessor(AnomalyDetectionSettings anomalyDetectionSettings, SamplingPercentageEstimatorSettings percentageEstimatorSettings,
         IHttpContextAccessor httpContextAccessor,
         ITelemetryProcessor skipSamplingTelemetryProcessor
         ) : base(percentageEstimatorSettings, null, skipSamplingTelemetryProcessor)
      {
         AddSettingsDefaultsIfEmpty(anomalyDetectionSettings);
         _httpContextAccessor   = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
         Settings               = anomalyDetectionSettings ?? throw new ArgumentNullException(nameof(anomalyDetectionSettings));
         SkipSamplingTelemetryProcessor = skipSamplingTelemetryProcessor;
      }

      private void AddSettingsDefaultsIfEmpty(AnomalyDetectionSettings anomalyDetectionSettings)
      {
         if (!anomalyDetectionSettings.DependencyNonSamplingRules.Any())
         {
            anomalyDetectionSettings.DependencyNonSamplingRules.AddRange(anomalyDetectionSettings.DefaultDependencyNonSamplingRules);
         }

         if (!anomalyDetectionSettings.RequestNonSamplingRules.Any())
         {
            anomalyDetectionSettings.RequestNonSamplingRules.AddRange(anomalyDetectionSettings.DefaultRequestNonSamplingRules);
         }
      }

      public new void Process(ITelemetry telemetry)
      {
         if (telemetry is MetricTelemetry || _httpContextAccessor.HttpContext == null)
         {
            base.Process(telemetry);
            return;
         }

         var journeyTelemetries = HttpContextTelemetryDictionary.GetOrCreateValue(_httpContextAccessor.HttpContext);
         journeyTelemetries.Add(telemetry, Settings);

         if (telemetry is RequestTelemetry)
         {
            // Request Telemetry will be the end of the journey when we've returned our response
            // So we can check all telemetries that have happened so far for this request (dependencies, exceptions, etc.)
            // And if any are bad, we can make sure we don't sample this entire journey
            if (!journeyTelemetries.ShouldSampleJourney)
            {
               Parallel.ForEach(journeyTelemetries.Telemetries, journeyTelemetry => SkipSamplingTelemetryProcessor.Process(journeyTelemetry));
            }
            else
            {
               Parallel.ForEach(journeyTelemetries.Telemetries, journeyTelemetry => base.Process(journeyTelemetry));
            }
         }
         else if (_httpContextAccessor.HttpContext.Response.HasStarted || journeyTelemetries.RequestTelemetryAlreadySent)
         {
            // This isn't a RequestTelemetry and the response has already started to be written.
            // This means this is a background task that was running after the response, so we should write this telemetry immediately.
            // As we won't have a future RequestTelemetry to write this (like above)
            if (!journeyTelemetries.ShouldSampleJourney)
            {
               SkipSamplingTelemetryProcessor.Process(telemetry);
            }
            else
            {
               base.Process(telemetry);
            }
         }
      }
   }
}