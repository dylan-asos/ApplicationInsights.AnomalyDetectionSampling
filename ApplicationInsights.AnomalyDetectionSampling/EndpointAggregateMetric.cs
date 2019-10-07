using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Metrics;

namespace ApplicationInsights.AnomalyDetectionSampling
{
   public class EndpointAggregateMetric
   {
      private const string Dimension1Name = "Endpoint";
      private const string Dimension2Name = "ResponseCode";
      private const string MetricId = "EndpointAggregates";

      public EndpointAggregateMetric(TelemetryClient telemetryClient)
      {
         Instance = telemetryClient.GetMetric(new MetricIdentifier("Custom", MetricId, Dimension1Name, Dimension2Name));
      }

      public Metric Instance
      {
         get;
      }
   }
}