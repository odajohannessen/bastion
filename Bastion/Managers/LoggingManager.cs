using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using System;
using System.Collections.Generic;
using System.Text;
using Bastion.Helpers;
using MediatR;

namespace Bastion.Managers;

public class LoggingManager
{
    private readonly TelemetryClient telemetryClient;

    public string runId;

    public LoggingManager(string connectionStringSecretName = "") // Input only used in the case of unit testing
    {
        // Initialize Telemetry Client
        string? connectionString;
        if (connectionStringSecretName == "")
        {
            connectionString = GetSecretFromKeyVaultHelper.GetSecret("APPLICATIONINSIGHTS-CONNECTION-STRING");
            if (connectionString == "Secret not found")
            {
                throw new Exception("Connection string not found");
            }
        }
        else 
        {
            // Gets environment variable stored in GitHub
            connectionString = Environment.GetEnvironmentVariable(connectionStringSecretName);
            if (connectionString == null) 
            {
                throw new Exception("Connection string not found for testing");
            }
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
