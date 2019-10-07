using System;
using System.Collections.Generic;
using Microsoft.ApplicationInsights.DataContracts;

// Need List<T> instead of IEnumerable<T> so that consumers can use the .Add() method
// ReSharper disable ReturnTypeCanBeEnumerable.Global

namespace ApplicationInsights.AnomalyDetectionSampling.Settings
{
   public class AnomalyDetectionSettings
   {
      public int MaximumTelemetryItemsPerSecond { get; set; } = 5;

      public ExceptionSamplingMode ExceptionSamplingMode { get; set; } = ExceptionSamplingMode.NeverSample;

      public List<NonSamplingRule<DependencyTelemetry>> DependencyNonSamplingRules { get; } = new List<NonSamplingRule<DependencyTelemetry>>();

      public List<NonSamplingRule<RequestTelemetry>> RequestNonSamplingRules { get; } = new List<NonSamplingRule<RequestTelemetry>>();
      
      public List<NonSamplingRule<EventTelemetry>> CustomEventNonSamplingRules { get; } = new List<NonSamplingRule<EventTelemetry>>();
      
      public List<NonSamplingRule<TraceTelemetry>> TraceNonSamplingRules { get; } = new List<NonSamplingRule<TraceTelemetry>>();
      
      public List<NonSamplingRule<ExceptionTelemetry>> ExceptionNonSamplingRules { get; } = new List<NonSamplingRule<ExceptionTelemetry>>();

      internal readonly List<NonSamplingRule<DependencyTelemetry>> DefaultDependencyNonSamplingRules = new List<NonSamplingRule<DependencyTelemetry>>
      {
         new NonSamplingRule<DependencyTelemetry>("Slow Dependencies", telemetry => telemetry.Duration > TimeSpan.FromSeconds(2)),
         new NonSamplingRule<DependencyTelemetry>("Failed Dependencies", telemetry => telemetry.Success == false)
      };

      internal readonly List<NonSamplingRule<RequestTelemetry>> DefaultRequestNonSamplingRules = new List<NonSamplingRule<RequestTelemetry>>
      {
         new NonSamplingRule<RequestTelemetry>("Slow Requests", telemetry => telemetry.Duration > TimeSpan.FromSeconds(5)),
         new NonSamplingRule<RequestTelemetry>("Failed Requests", telemetry => telemetry.Success == false)
      };
   }
}