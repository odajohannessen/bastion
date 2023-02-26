using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using System;
using System.Collections.Generic;
using System.Text;
using EndOfSecretLifetime.Helpers;

namespace EndOfSecretLifetime.Managers;

public class LoggingManager
{
    private readonly TelemetryClient telemetryClient;

    public string runId;

    public LoggingManager()
    {
        // Initialize Telemetry Client
        //var connectionString = Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING");
        string connectionString = GetSecretFromKeyVaultHelper.GetSecret("APPLICATIONINSIGHTS-CONNECTION-STRING");
        if (connectionString == "Secret not found") 
        {
            throw new Exception("Connection string not found");
        }

        telemetryClient = new TelemetryClient(new TelemetryConfiguration() { ConnectionString = connectionString });
        runId = Guid.NewGuid().ToString();
    }

    public void LogTrace(string message)
    {
        telemetryClient.TrackTrace($"Runid: {runId}. {message}");
    }

    public void LogEvent(string message)
    {
        telemetryClient.TrackEvent($"Runid: {runId}. {message}");
    }

    public void LogException(string message)
    {
        telemetryClient.TrackException(new Exception($"Runid: {runId}. {message}"));
    }

}
