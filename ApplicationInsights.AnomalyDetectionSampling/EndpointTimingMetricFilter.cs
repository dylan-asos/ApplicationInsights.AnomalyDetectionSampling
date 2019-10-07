using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ApplicationInsights.AnomalyDetectionSampling
{
   public class EndpointTimingMetricFilter : IResultFilter
   {
      private readonly EndpointAggregateMetric _endpointMetric;
      
      public EndpointTimingMetricFilter(EndpointAggregateMetric endpointMetric)
      {
         _endpointMetric = endpointMetric;
      }
      
      public void OnResultExecuted(ResultExecutedContext context)
      {
         if (!(context.HttpContext.Items["timer"] is Stopwatch sw))
         {
            return;
         }

         sw.Stop();

         var endpointId = GetEndpointId(context);
         _endpointMetric.Instance.TrackValue(sw.ElapsedMilliseconds, endpointId, context.HttpContext.Response.StatusCode.ToString());
      }

      public void OnResultExecuting(ResultExecutingContext context)
      {
         context.HttpContext.Items["timer"] = Stopwatch.StartNew();
      }

      private static string GetEndpointId(ActionContext context)
      {
         var template = context.ActionDescriptor.AttributeRouteInfo.Template;
         var method   = context.HttpContext.Request.Method;
         return method + " - " + template;
      }
   }
}