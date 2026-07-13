# School Account Beta - Observability Spike

## Overview

This repository contains a proof of concept demonstrating how services across a mixed technology estate can participate in a shared observability platform using OpenTelemetry.

The purpose of the spike is to prove that services instrumented with OpenTelemetry can emit standard telemetry into a common observability stack, regardless of the implementation technology.

The current proof of concept includes:

- .NET 8
- Ruby / Sinatra
- OpenTelemetry
- OpenTelemetry Collector
- Grafana
- Tempo
- Prometheus

The spike demonstrates distributed tracing across multiple services, downstream dependency visibility, latency attribution and failure diagnosis.

Chaos and failure simulation are used only to generate interesting telemetry for the observability platform.

This is **not a chaos engineering proof of concept**.

---

## Objective

The objective is to demonstrate that a DfE service can participate in a common observability platform by adopting OpenTelemetry instrumentation.

The proof of concept demonstrates:

- Distributed tracing
- Cross-service trace propagation
- Cross-language trace propagation
- Service dependencies
- Slow downstream calls
- Failed requests
- Timeouts
- Random latency
- Random failures
- SQL dependency telemetry
- Service maps
- Request rates
- Application observability

The intended audience includes:

- Solutions Architects
- Technical Architects
- Senior Developers
- Platform Engineers

---

## Architecture

The current application flow is:

```text
Browser
   |
   v
school-account-api (.NET 8)
   |
   v
collect-api (.NET 8)
   |
   +----------------------+
   |                      |
   v                      v
student-records-api       ruby-service
(.NET 8)                  (Ruby / Sinatra)
   |                      |
   +----------+-----------+
              |
              v
      OpenTelemetry Collector
              |
       +------+------+
       |             |
       v             v
     Tempo       Prometheus
       |             |
       +------+------+
              |
              v
           Grafana
```

The services export telemetry using OpenTelemetry Protocol (OTLP).

The .NET services export traces using OTLP/gRPC.

The Ruby service exports traces using OTLP/HTTP.

Both protocols are received by the same OpenTelemetry Collector.

---

## Services

### school-account-api

Represents the School Account application.

Technology:

```text
.NET 8
ASP.NET Core
OpenTelemetry
```

Responsibilities:

- Hosts the Operations Console
- Generates simulated application traffic
- Calls COLLECT
- Starts the distributed trace
- Exports OpenTelemetry traces

Docker service:

```text
school-account-api
```

Local URL:

```text
http://localhost:5032
```

Operations Console:

```text
http://localhost:5032/demo
```

---

### collect-api

Represents the COLLECT dependency.

Technology:

```text
.NET 8
ASP.NET Core
OpenTelemetry
```

Responsibilities:

- Receives requests from School Account
- Calls downstream services
- Generates latency and failure scenarios
- Calls Student Records
- Calls the Ruby service
- Demonstrates SQL dependency instrumentation
- Continues the distributed trace

Docker service:

```text
collect-api
```

Local URL:

```text
http://localhost:5002
```

---

### student-records-api

Represents an additional downstream .NET service.

Technology:

```text
.NET 8
ASP.NET Core
OpenTelemetry
```

Responsibilities:

- Demonstrates a multi-service .NET dependency chain
- Generates normal responses
- Generates slow responses
- Generates failures
- Generates timeout scenarios

Docker service:

```text
student-records-api
```

Local URL:

```text
http://localhost:5040
```

---

### ruby-service

Demonstrates cross-platform OpenTelemetry instrumentation.

Technology:

```text
Ruby 3.3
Sinatra
Puma
OpenTelemetry Ruby
```

Responsibilities:

- Receives requests from COLLECT
- Accepts W3C trace context from .NET
- Continues the distributed trace
- Creates Ruby application spans
- Exports traces using OTLP/HTTP
- Generates latency and failure scenarios

Docker service:

```text
ruby-service
```

Local URL:

```text
http://localhost:5003
```

Health endpoint:

```text
http://localhost:5003/health
```

---

### OpenTelemetry Collector

The OpenTelemetry Collector provides a common telemetry ingestion point.

Docker service:

```text
otel-collector
```

The collector accepts:

```text
OTLP/gRPC  - port 4317
OTLP/HTTP  - port 4318
```

The .NET services use:

```text
http://otel-collector:4317
```

The Ruby service uses:

```text
http://otel-collector:4318
```

The collector forwards traces to Tempo.

Collector configuration:

```text
otel-collector-config.yaml
```

---

### Tempo

Tempo is the distributed tracing backend.

Docker service:

```text
tempo
```

Local URL:

```text
http://localhost:3200
```

Tempo receives traces from the OpenTelemetry Collector.

Grafana uses Tempo as its tracing data source.

Configuration:

```text
tempo.yaml
```

---

### Prometheus

Prometheus provides metrics storage and querying.

Docker service:

```text
prometheus
```

Local URL:

```text
http://localhost:9090
```

Configuration:

```text
prometheus.yaml
```

---

### Grafana

Grafana provides the observability user interface.

Docker service:

```text
grafana
```

Local URL:

```text
http://localhost:3000
```

Default local credentials:

```text
Username: admin
Password: admin
```

Tempo and Prometheus are provisioned as Grafana data sources.

Grafana provisioning configuration is located under:

```text
grafana/provisioning
```

---

## Prerequisites

The complete proof of concept is designed to run using Docker Compose.

Required:

- Docker Desktop
- Docker Compose
- Git

Optional for local development:

- .NET 8 SDK
- Ruby 3.3
- Bundler

Check Docker:

```bash
docker --version
docker compose version
```

Check the .NET SDK:

```bash
dotnet --version
```

The repository contains a `global.json` defining the expected .NET SDK version.

---

## Running the Complete Stack

Clone the repository:

```bash
git clone https://github.com/DFE-Digital/SchoolAccount-Beta-ObservabilitySpike.git
```

Change into the repository:

```bash
cd SchoolAccount-Beta-ObservabilitySpike
```

Build and start the complete stack:

```bash
docker compose up --build
```

Docker Compose starts:

```text
school-account-api
collect-api
student-records-api
ruby-service
otel-collector
tempo
prometheus
grafana
```

Wait for the services to complete their startup.

---

## Running in the Background

To run the stack in detached mode:

```bash
docker compose up -d --build
```

Check the running containers:

```bash
docker compose ps
```

View all logs:

```bash
docker compose logs -f
```

View logs for a specific service:

```bash
docker compose logs -f school-account-api
```

For example:

```bash
docker compose logs -f collect-api
```

```bash
docker compose logs -f ruby-service
```

```bash
docker compose logs -f otel-collector
```

---

## Operations Console

Open:

```text
http://localhost:5032/demo
```

The Operations Console generates continuous traffic through the application.

The console allows a scenario and request rate to be selected.

Available scenarios include normal traffic, latency, failures, timeouts and downstream dependency failures.

The console displays:

- Total requests
- Successful requests
- Failed requests
- Average latency
- Recent request activity

Select a scenario, configure the requests per second and select **Start Traffic**.

The generated telemetry is exported through OpenTelemetry and can be inspected in Grafana.

---

## Simulation Scenarios

### Normal

Generates successful application traffic.

Example:

```text
GET /api/simulation/normal
```

Expected trace:

```text
school-account-api
    |
    v
collect-api
    |
    v
student-records-api
```

---

### Slow

Introduces artificial downstream latency.

Example:

```text
GET /api/simulation/slow
```

This demonstrates how distributed tracing can identify the service and span responsible for increased request duration.

---

### Error

Generates a downstream service failure.

Example:

```text
GET /api/simulation/error
```

This demonstrates:

- Failed spans
- HTTP error propagation
- Downstream failure attribution

---

### Timeout

Generates a downstream call that exceeds the configured HTTP client timeout.

Example:

```text
GET /api/simulation/timeout
```

This demonstrates how timeout failures appear within a distributed trace.

---

### Random Latency

Generates variable downstream response times.

Example:

```text
GET /api/simulation/random-latency
```

This is useful for producing realistic latency distributions.

---

### Random Failure

Generates intermittent downstream failures.

Example:

```text
GET /api/simulation/random-failure
```

This is useful for demonstrating error rates and intermittent dependency failures.

---

## Ruby Cross-Platform Scenarios

The Ruby scenarios demonstrate a distributed trace crossing language and runtime boundaries.

### Ruby Normal

```text
GET /api/simulation/ruby
```

Expected trace:

```text
school-account-api (.NET)
    |
    v
collect-api (.NET)
    |
    v
ruby-service (Ruby)
```

---

### Ruby Slow

```text
GET /api/simulation/ruby-slow
```

Introduces latency inside the Ruby service.

---

### Ruby Error

```text
GET /api/simulation/ruby-error
```

Generates an exception inside the Ruby service.

---

### Ruby Timeout

```text
GET /api/simulation/ruby-timeout
```

Generates a Ruby response that exceeds the COLLECT HTTP client timeout.

---

### Ruby Random Latency

```text
GET /api/simulation/ruby-random-latency
```

Generates variable latency inside the Ruby service.

---

### Ruby Random Failure

```text
GET /api/simulation/ruby-random-failure
```

Generates intermittent failures inside the Ruby service.

---

## Full Dependency Chain

The chain scenario exercises multiple downstream dependencies within a single distributed trace.

```text
GET /api/simulation/chain
```

The trace demonstrates:

```text
school-account-api
    |
    v
collect-api
    |
    +----------------------+
    |                      |
    v                      v
student-records-api       ruby-service
```

This scenario is useful for demonstrating service dependencies and trace fan-out in Grafana.

---

## Testing the Services Directly

### School Account

```bash
curl http://localhost:5032/api/simulation/normal
```

### COLLECT

```bash
curl http://localhost:5002/api/collect/normal
```

### Student Records

```bash
curl http://localhost:5040/api/student-records/normal
```

### Ruby

```bash
curl http://localhost:5003/health
```

```bash
curl http://localhost:5003/api/ruby/normal
```

---

## Generating Ruby Traffic

To generate multiple cross-platform traces:

```bash
for i in {1..20}; do
  curl -s http://localhost:5032/api/simulation/ruby > /dev/null
  sleep 0.5
done
```

For slow Ruby traces:

```bash
for i in {1..10}; do
  curl -s http://localhost:5032/api/simulation/ruby-slow > /dev/null
done
```

---

## Viewing Traces in Grafana

Open Grafana:

```text
http://localhost:3000
```

Sign in using:

```text
Username: admin
Password: admin
```

Navigate to:

```text
Explore
    >
Tempo
```

Set an appropriate time range, for example:

```text
Last 15 minutes
```

Run the query.

---

## Finding Ruby Traces

In Tempo, use the following TraceQL query:

```traceql
{ resource.service.name = "ruby-service" }
```

A cross-platform trace should contain spans from:

```text
school-account-api
collect-api
ruby-service
```

All spans should share the same trace ID.

The parent-child relationship should continue across the .NET and Ruby service boundary.

---

## Service Graph

In Grafana Explore, select the Tempo data source and view the Node Graph.

After generating traffic, the service graph should show dependencies similar to:

```text
user
  |
  v
school-account-api
  |
  v
collect-api
  |
  +----------------------+
  |                      |
  v                      v
student-records-api       ruby-service
```

Services only appear in the graph after telemetry has been generated for the selected time range.

If `ruby-service` is not visible, generate Ruby traffic:

```bash
for i in {1..20}; do
  curl -s http://localhost:5032/api/simulation/ruby > /dev/null
done
```

Then refresh the Tempo query.

---

## Verifying OpenTelemetry Export

### Ruby Service Logs

```bash
docker compose logs -f ruby-service
```

Generate a Ruby request:

```bash
curl http://localhost:5032/api/simulation/ruby
```

The Ruby service should receive:

```text
GET /api/ruby/normal
```

---

### COLLECT Logs

```bash
docker compose logs -f collect-api
```

The COLLECT service should call:

```text
http://ruby-service:8080/api/ruby/normal
```

---

### OpenTelemetry Collector Logs

```bash
docker compose logs -f otel-collector
```

Generate traffic:

```bash
curl http://localhost:5032/api/simulation/ruby
```

The collector should receive spans containing:

```text
service.name: ruby-service
```

---

## OpenTelemetry Instrumentation

### .NET

The .NET services use OpenTelemetry instrumentation for:

- ASP.NET Core requests
- HttpClient dependencies
- SQL client dependencies
- Custom `ActivitySource` spans

Outgoing `HttpClient` requests automatically propagate W3C trace context.

This allows downstream services to continue the same distributed trace.

---

### Ruby

The Ruby service uses:

- `opentelemetry-api`
- `opentelemetry-sdk`
- `opentelemetry-exporter-otlp`
- `opentelemetry-instrumentation-rack`
- `opentelemetry-instrumentation-sinatra`

Rack and Sinatra instrumentation create HTTP server spans.

The Ruby service also creates application-level scenario spans.

Ruby receives the W3C trace context propagated by the .NET `HttpClient` instrumentation and continues the existing distributed trace.

Ruby exports traces using:

```text
OTLP/HTTP
```

The .NET services export traces using:

```text
OTLP/gRPC
```

Both are accepted by the same OpenTelemetry Collector.

---

## Corporate Certificate Environments

Some DfE development environments use HTTPS inspection with an internal certificate authority.

A Docker build may fail when Bundler accesses RubyGems with an error similar to:

```text
SSL verification error
self-signed certificate in certificate chain
Root certificate is not trusted
```

Do not disable SSL verification.

The local corporate root certificate must be added to the container trust store for development on the affected network.

Corporate certificates must not be committed to this repository.

The following paths and file types should remain excluded from Git:

```text
Ruby.Service/certificates/
*.cer
*.crt
```

Certificate handling is environment-specific and should use an approved local or CI certificate injection mechanism.

---

## Rebuilding a Single Service

Rebuild the Ruby service:

```bash
docker compose build --no-cache ruby-service
```

Start the Ruby service:

```bash
docker compose up -d ruby-service
```

Rebuild and restart the School Account API:

```bash
docker compose up -d --build school-account-api
```

Rebuild the complete stack:

```bash
docker compose up -d --build
```

---

## Stopping the Stack

Stop the containers:

```bash
docker compose down
```

Stop the containers and remove volumes:

```bash
docker compose down -v
```

Remove generated images if required:

```bash
docker compose down --rmi local
```

---

## Troubleshooting

### Ruby does not appear in Grafana

Confirm Ruby is running:

```bash
docker compose ps ruby-service
```

Test Ruby directly:

```bash
curl http://localhost:5003/health
```

Test the complete distributed trace:

```bash
curl http://localhost:5032/api/simulation/ruby
```

Check Ruby logs:

```bash
docker compose logs -f ruby-service
```

Check collector logs:

```bash
docker compose logs -f otel-collector
```

In Tempo, query:

```traceql
{ resource.service.name = "ruby-service" }
```

Ensure the selected Grafana time range includes the generated traffic.

---

### A service cannot call another Docker service

Docker services must use the Compose service name rather than `localhost`.

Correct:

```text
http://collect-api:8080
http://student-records-api:8080
http://ruby-service:8080
http://otel-collector:4317
http://otel-collector:4318
```

Incorrect from inside a container:

```text
http://localhost:5002
http://localhost:5003
```

Inside a container, `localhost` refers to that container.

---

### Ruby Bundler SSL certificate failure

If the build reports:

```text
Bundler::Fetcher::CertificateFailureError
```

or:

```text
self-signed certificate in certificate chain
```

the development network may be using HTTPS inspection.

Do not use:

```text
BUNDLE_SSL_VERIFY_MODE=0
```

Do not change RubyGems to HTTP.

Install the approved corporate CA certificate into the container trust store using an environment-appropriate mechanism.

---

## Project Structure

```text
.
├── Collect.Api/
│   ├── Chaos/
│   ├── Clients/
│   ├── Data/
│   ├── Dockerfile
│   └── Program.cs
│
├── Ruby.Service/
│   ├── app.rb
│   ├── config.ru
│   ├── telemetry.rb
│   ├── Gemfile
│   └── Dockerfile
│
├── SpikeB.Observability.Api/
│   ├── Clients/
│   ├── Controllers/
│   ├── Models/
│   ├── Pages/
│   ├── wwwroot/
│   ├── Dockerfile
│   └── Program.cs
│
├── StudentRecords.Api/
│   ├── Dockerfile
│   └── Program.cs
│
├── grafana/
│   └── provisioning/
│
├── compose.yaml
├── otel-collector-config.yaml
├── prometheus.yaml
├── tempo.yaml
├── global.json
└── README.md
```

---

## What the Spike Proves

The proof of concept demonstrates that OpenTelemetry can provide a common instrumentation standard across a mixed service estate.

Specifically, the spike proves that:

- .NET services can export standard OpenTelemetry telemetry
- Ruby services can export standard OpenTelemetry telemetry
- Trace context can propagate between .NET and Ruby
- Multiple OTLP transports can use the same collector
- Service dependencies can be reconstructed from distributed traces
- Downstream latency can be attributed to the responsible service
- Downstream failures can be identified within a trace
- Grafana and Tempo can present a cross-platform service dependency graph

The key architectural principle demonstrated by the spike is:

> Services do not need to use the same programming language or framework to participate in the same observability platform. They need to emit compatible telemetry and propagate standard trace context.

---

## Status

This repository contains proof-of-concept code produced as part of an observability technical spike.

The implementation is intended to validate architecture and instrumentation approaches.

It is not intended to represent a production deployment configuration.