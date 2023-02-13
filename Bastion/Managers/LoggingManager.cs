﻿//using Microsoft.ApplicationInsights;
//using Microsoft.ApplicationInsights.Extensibility;
//using System;
//using System.Collections.Generic;
//using System.Text;
//using Bastion.Helpers;
//using MediatR;

//namespace Bastion.Managers;

//public class LoggingManager
//{
//    private readonly TelemetryClient telemetryClient;

//    public string runId;

//    public LoggingManager()
//    {
//        // Initialize Telemetry Client
//        var connectionString = Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING");
//        telemetryClient = new TelemetryClient(new TelemetryConfiguration() { ConnectionString = connectionString });
//        runId = Guid.NewGuid().ToString();
//    }

//    public void LogTrace(string message)
//    {
//        telemetryClient.TrackTrace($"Runid: {runId}. {message}");
//    }

//    public void LogEvent(string message)
//    {
//        telemetryClient.TrackEvent($"Runid: {runId}. {message}");
//    }

//    public void LogException(string message)
//    {
//        telemetryClient.TrackException(new Exception($"Runid: {runId}. {message}"));
//    }

//}