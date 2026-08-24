using System.Diagnostics;
using Serilog.Core;
using Serilog.Events;

namespace OficinaMecanica.Infrastructure.Logging;

public class CorrelationIdEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var correlationId = Activity.Current?.TraceId.ToString();
        if (string.IsNullOrEmpty(correlationId))
            return;

        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("CorrelationId", correlationId));
    }
}
