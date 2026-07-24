# frozen_string_literal: true

require_relative "telemetry"
require "json"

class RubyService < Sinatra::Base
  configure do
    set :bind, "0.0.0.0"
    set :port, ENV.fetch("PORT", "8080").to_i
    set :show_exceptions, false
    set :raise_errors, false
  end

  before do
    content_type :json
    @request_started_at = Process.clock_gettime(Process::CLOCK_MONOTONIC)
  end

  after do
    duration_ms =
      ((Process.clock_gettime(Process::CLOCK_MONOTONIC) - @request_started_at) * 1000).round

    failed = response.status >= 500

    level = failed ? "ERROR" : "INFO"
    outcome = failed ? "failed" : "completed"

    puts(
      "#{level} " \
      "#{request.request_method} " \
      "#{request.path} " \
      "#{outcome} " \
      "#{response.status} " \
      "in #{duration_ms}ms"
    )
  end

  get "/health" do
    json(status: "healthy", service: "ruby-service")
  end

  get "/api/ruby/normal" do
    scenario_span("normal") do |span|
      span.add_event("ruby.work.completed")

      json(
        scenario: "normal",
        success: true,
        message: "Ruby service completed normally"
      )
    end
  end

  get "/api/ruby/slow" do
    delay_ms = bounded_integer("delayMs", 2_000, 100, 15_000)

    scenario_span("slow", "demo.delay_ms" => delay_ms) do |span|
      span.add_event(
        "ruby.delay.started",
        attributes: { "delay.ms" => delay_ms }
      )

      sleep(delay_ms / 1000.0)

      span.add_event(
        "ruby.delay.completed",
        attributes: { "delay.ms" => delay_ms }
      )

      json(
        scenario: "slow",
        success: true,
        delayMs: delay_ms,
        message: "Ruby service completed after an artificial delay"
      )
    end
  end

  get "/api/ruby/error" do
    scenario_span("error") do |span|
      exception = StandardError.new("Simulated Ruby service failure")
  
      span.record_exception(exception)
      span.status = OpenTelemetry::Trace::Status.error(exception.message)
      span.set_attribute("error.type", exception.class.name)
  
      status 500
  
      json(
        scenario: "error",
        success: false,
        error: exception.class.name,
        message: exception.message
      )
    end
  end

  get "/api/ruby/timeout" do
    delay_ms = bounded_integer("delayMs", 12_000, 1_000, 30_000)

    scenario_span("timeout", "demo.delay_ms" => delay_ms) do
      sleep(delay_ms / 1000.0)

      json(
        scenario: "timeout",
        success: true,
        delayMs: delay_ms,
        message: "Ruby timeout delay completed"
      )
    end
  end

  get "/api/ruby/random-latency" do
    delay_ms = rand(250..4_000)

    scenario_span(
      "random-latency",
      "demo.delay_ms" => delay_ms
    ) do
      sleep(delay_ms / 1000.0)

      json(
        scenario: "random-latency",
        success: true,
        delayMs: delay_ms,
        message: "Ruby service returned with random latency"
      )
    end
  end

  get "/api/ruby/random-failure" do
    failure_percentage = bounded_integer("failurePercentage", 40, 0, 100)
    failed = rand(1..100) <= failure_percentage
  
    scenario_span(
      "random-failure",
      "demo.failure_percentage" => failure_percentage,
      "demo.failure_triggered" => failed
    ) do |span|
  
      if failed
        exception = StandardError.new("Random Ruby failure was triggered")
  
        span.record_exception(exception)
        span.status = OpenTelemetry::Trace::Status.error(exception.message)
        span.set_attribute("error.type", exception.class.name)
  
        status 500
  
        json(
          scenario: "random-failure",
          success: false,
          failurePercentage: failure_percentage,
          error: exception.class.name,
          message: exception.message
        )
      else
        json(
          scenario: "random-failure",
          success: true,
          failurePercentage: failure_percentage,
          message: "Ruby service completed without triggering a failure"
        )
      end
    end
  end

  not_found do
    status 404

    json(
      success: false,
      error: "Route not found",
      path: request.path
    )
  end

  error do
    exception = env["sinatra.error"]
    span = OpenTelemetry::Trace.current_span

    if exception
      span.record_exception(exception)
      span.status = OpenTelemetry::Trace::Status.error(exception.message)
      span.set_attribute("error.type", exception.class.name)
    end

    status 500

    json(
      success: false,
      error: exception&.class&.name || "UnknownError",
      message: exception&.message || "Unexpected Ruby service failure"
    )
  end

  private

  def tracer
    OpenTelemetry.tracer_provider.tracer(
      "ruby-service.scenarios",
      ENV.fetch("OTEL_SERVICE_VERSION", "spike-b")
    )
  end

  def scenario_span(scenario, attributes = {})
    tracer.in_span(
      "ruby.scenario.#{scenario}",
      attributes: {
        "demo.scenario" => scenario,
        "service.component" => "ruby-scenario-engine"
      }.merge(attributes)
    ) do |span|
      yield span
    end
  end

  def bounded_integer(name, default, minimum, maximum)
    value = params.fetch(name, default).to_i
    [[value, minimum].max, maximum].min
  end

  def json(payload)
    JSON.generate(
      payload.merge(service: "ruby-service")
    )
  end
end