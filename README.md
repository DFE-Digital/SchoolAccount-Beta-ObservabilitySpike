# School Account Beta -- Observability Spike

## Overview

This repository contains a proof of concept used to investigate
observability options for the School Account Beta platform.

The spike explores how telemetry can be collected from downstream
services using OpenTelemetry and Azure Application Insights, and
evaluates how this approach could be extended across a mixed technology
estate including:

-   .NET 8 services
-   Legacy .NET Framework services (COLLECT)
-   Ruby services
-   Dynamics 365
-   Future Node.js services (stretch goal)

The aim is to determine whether a common observability strategy can be
adopted for the Beta platform.

## Spike Objectives

-   Instrument downstream HTTP calls using OpenTelemetry.
-   Evaluate Azure Application Insights as the primary telemetry
    platform.
-   Investigate Grafana, Tempo and Prometheus as an alternative or
    complementary observability stack.
-   Evaluate support for legacy .NET Framework, Ruby and Dynamics 365.
-   Produce service maps and telemetry suitable for latency and error
    dashboards.
-   Recommend an approach for the School Account Beta programme.

## Current Proof of Concept

-   ASP.NET Core request instrumentation
-   Automatic HttpClient dependency tracking
-   Manual business-level tracing using ActivitySource
-   Distributed trace correlation
-   Simulated slow responses
-   Simulated downstream failures
-   Multiple downstream dependency calls

## Endpoints

  ---------------------------------------------------------------------------
  Endpoint                           Description
  ---------------------------------- ----------------------------------------
  `GET /api/observability/collect`   Simulates a downstream service health
                                     check

  `GET /api/observability/todo`      Demonstrates custom business tracing and
                                     dependency tracking

  `GET /api/observability/slow`      Simulates a slow downstream dependency
                                     for latency dashboards

  `GET /api/observability/error`     Simulates a failing downstream
                                     dependency

  `GET /api/observability/chain`     Simulates multiple downstream service
                                     calls within a single request
  ---------------------------------------------------------------------------

## Local Development

``` bash
dotnet restore
dotnet run
```

Swagger:

``` text
http://localhost:<port>/swagger
```

## Configuration

``` json
{
  "Downstream": {
    "CollectBaseUrl": "https://jsonplaceholder.typicode.com"
  }
}
```

``` json
{
  "ApplicationInsights": {
    "ConnectionString": "<connection-string>"
  }
}
```

If no Application Insights connection string is supplied, telemetry is
exported to the console only.

## Current Findings

### ASP.NET Core

-   Automatic request instrumentation
-   Automatic dependency instrumentation
-   Distributed trace correlation

### Legacy .NET Framework (COLLECT)

Investigation in progress into OpenTelemetry compatibility, Application
Insights SDK support, correlation using traceparent, and manual
instrumentation options.

### Ruby

Investigation planned to validate OpenTelemetry support and distributed
tracing.

### Dynamics 365

Investigation planned to assess telemetry, correlation support and Azure
Monitor integration.

## Next Steps

-   Investigate existing monitoring within COLLECT.
-   Evaluate instrumentation options for legacy .NET Framework.
-   Create a Ruby proof of concept.
-   Evaluate Grafana, Tempo and Prometheus locally.
-   Compare Azure Monitor and Grafana approaches.
-   Produce example dashboards for latency, request rates, errors and
    service maps.
-   Recommend an observability approach for the School Account Beta
    programme.

## Status

Work in progress. This repository contains spike code and is not
intended to represent production-ready implementation.
