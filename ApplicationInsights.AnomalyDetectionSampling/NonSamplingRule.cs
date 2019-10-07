using System;
using Microsoft.ApplicationInsights.Channel;

namespace ApplicationInsights.AnomalyDetectionSampling
{
   public class NonSamplingRule<TTelemetry> where TTelemetry : ITelemetry
   {
      public string Name { get; }
      public Func<TTelemetry, bool> Rule { get; }
      
      public NonSamplingRule(string name, Func<TTelemetry, bool> rule)
      {
         Name = name ?? throw new ArgumentNullException(nameof(name));
         Rule = rule ?? throw new ArgumentNullException(nameof(rule));
      }
   }
}