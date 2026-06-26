using System.Diagnostics;
using Serilog.Core;
using Serilog.Events;

namespace BuildingBlocks.Logging;

public sealed class TraceIdEnricher : ILogEventEnricher
{
  public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
  {
    Activity? activity = Activity.Current;

    if (activity is null)
    {
      return;
    }

    logEvent.AddPropertyIfAbsent(
      propertyFactory.CreateProperty("TraceId", activity.TraceId.ToString()));

    logEvent.AddPropertyIfAbsent(
      propertyFactory.CreateProperty("SpanId", activity.SpanId.ToString()));
  }
}
