using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Parking.Application.Common;

public static class ApplicationDiagnostics
{
    public const string SourceName = "Parking.Application";

    public static readonly ActivitySource ActivitySource = new(SourceName);
}

/// <summary>Business metrics exported through OpenTelemetry.</summary>
public sealed class ParkingMetrics
{
    public const string MeterName = "Parking.Business";

    private readonly Counter<long> _ticketsOpened;
    private readonly Counter<long> _ticketsClosed;
    private readonly Counter<double> _revenue;
    private readonly Counter<long> _loginFailures;

    public ParkingMetrics(IMeterFactory meterFactory)
    {
        ArgumentNullException.ThrowIfNull(meterFactory);
        var meter = meterFactory.Create(MeterName);
        _ticketsOpened = meter.CreateCounter<long>("parking.tickets.opened", description: "Check-ins performed");
        _ticketsClosed = meter.CreateCounter<long>("parking.tickets.closed", description: "Check-outs performed");
        _revenue = meter.CreateCounter<double>("parking.revenue", unit: "AOA", description: "Revenue from closed tickets");
        _loginFailures = meter.CreateCounter<long>("parking.auth.login_failures", description: "Failed login attempts");
    }

    public void TicketOpened() => _ticketsOpened.Add(1);

    public void TicketClosed(decimal amount)
    {
        _ticketsClosed.Add(1);
        _revenue.Add((double)amount);
    }

    public void LoginFailed() => _loginFailures.Add(1);
}
