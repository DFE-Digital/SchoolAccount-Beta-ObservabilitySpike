FROM ruby:3.3-slim

WORKDIR /app

RUN apt-get update \
    && apt-get install -y --no-install-recommends \
        build-essential \
        ca-certificates \
        openssl \
    && rm -rf /var/lib/apt/lists/*

COPY Ruby.Service/certificates/pki-root-ca.crt \
    /usr/local/share/ca-certificates/pki-root-ca.crt

RUN update-ca-certificates

ENV SSL_CERT_FILE=/etc/ssl/certs/ca-certificates.crt
ENV BUNDLE_SSL_CA_CERT=/etc/ssl/certs/ca-certificates.crt

COPY Ruby.Service/Gemfile ./Gemfile

RUN bundle config set path "/usr/local/bundle" \
    && bundle install

COPY Ruby.Service/app.rb \
     Ruby.Service/config.ru \
     Ruby.Service/telemetry.rb \
     ./

RUN mkdir -p /var/log/ruby

ENV RACK_ENV=production
ENV PORT=8080

EXPOSE 8080

CMD ["bundle", "exec", "puma", "--bind", "tcp://0.0.0.0:8080", "--workers", "1", "--threads", "1:5", "config.ru"]