# frozen_string_literal: true

# Load the framework before attempting to install its instrumentation.
require "sinatra/base"

require "opentelemetry/sdk"
require "opentelemetry/exporter/otlp"
require "opentelemetry/instrumentation/rack"
require "opentelemetry/instrumentation/sinatra"

OpenTelemetry::SDK.configure do |config|
  config.service_name = ENV.fetch("OTEL_SERVICE_NAME", "ruby-service")
  config.service_version = ENV.fetch("OTEL_SERVICE_VERSION", "spike-b")

  config.use "OpenTelemetry::Instrumentation::Rack"
  config.use "OpenTelemetry::Instrumentation::Sinatra"
end