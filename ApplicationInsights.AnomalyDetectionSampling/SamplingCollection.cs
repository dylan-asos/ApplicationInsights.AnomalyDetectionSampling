using System.Collections.Generic;
using System.Linq;
using ApplicationInsights.AnomalyDetectionSampling.Settings;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;

namespace ApplicationInsights.AnomalyDetectionSampling
{
   internal class SamplingCollection
   {
      public List<ITelemetry> Telemetries { get; } = new List<ITelemetry>();
      public bool ShouldSampleJourney { get; private set; } = true;
      public bool RequestTelemetryAlreadySent { get; private set; } = false;
      private AnomalyDetectionSettings _settings;

      internal void Add(ITelemetry telemetry, AnomalyDetectionSettings settings)
      {
         _settings = settings;
         
         CheckShouldSample(telemetry);

         Telemetries.Add(telemetry);
      }

      private void CheckShouldSample(ITelemetry telemetry)
      {
         switch (telemetry)
         {
            case DependencyTelemetry dependencyTelemetry:
               if (_settings.DependencyNonSamplingRules.Any(rule => rule.Rule.Invoke(dependencyTelemetry)))
               {
                  ShouldSampleJourney = false;
               }

               break;
            case EventTelemetry eventTelemetry:
               if (_settings.CustomEventNonSamplingRules.Any(rule => rule.Rule.Invoke(eventTelemetry)))
               {
                  ShouldSampleJourney = false;
               }

               break;
            case ExceptionTelemetry exceptionTelemetry:
               if (_settings.ExceptionSamplingMode == ExceptionSamplingMode.NeverSample ||
                   (_settings.ExceptionSamplingMode == ExceptionSamplingMode.UseExceptionRules &&
                    _settings.ExceptionNonSamplingRules.Any(rule => rule.Rule.Invoke(exceptionTelemetry))))
               {
                  ShouldSampleJourney = false;
               }

               break;
            case RequestTelemetry requestTelemetry:
               RequestTelemetryAlreadySent = true;
               if (_settings.RequestNonSamplingRules.Any(rule => rule.Rule.Invoke(requestTelemetry)))
               {
                  ShouldSampleJourney = false;
               }

               break;
            case TraceTelemetry traceTelemetry:
               if (_settings.TraceNonSamplingRules.Any(rule => rule.Rule.Invoke(traceTelemetry)))
               {
                  ShouldSampleJourney = false;
               }

               break;
         }
      }
   }
}