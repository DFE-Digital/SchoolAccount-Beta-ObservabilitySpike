#!/bin/sh
# One-time bootstrap: register the mounted dashboards/ directory as a
# provisioning repository (the same thing "Administration > Provisioning >
# Configure file provisioning" does in the UI). Safe to re-run: exits early
# if the repository connection already exists.
set -eu

GRAFANA_URL="${GRAFANA_URL:-http://grafana:3000}"
AUTH="${GRAFANA_USER:-admin}:${GRAFANA_PASSWORD:-admin}"
REPO_API="$GRAFANA_URL/apis/provisioning.grafana.app/v0alpha1/namespaces/default/repositories"

echo "waiting for Grafana..."
until curl -sf -u "$AUTH" "$GRAFANA_URL/api/health" >/dev/null; do
  sleep 2
done

if curl -sf -u "$AUTH" "$REPO_API/dashboards" >/dev/null 2>&1; then
  echo "repository connection already exists, nothing to do"
  exit 0
fi

echo "creating file provisioning repository connection..."
curl -sf -u "$AUTH" -H 'Content-Type: application/json' -X POST "$REPO_API" -d '{
  "apiVersion": "provisioning.grafana.app/v0alpha1",
  "kind": "Repository",
  "metadata": {"name": "dashboards"},
  "spec": {
    "title": "Dashboards",
    "type": "local",
    "local": {"path": "dashboards/"},
    "workflows": ["write"],
    "sync": {"enabled": true, "target": "folder", "intervalSeconds": 60}
  }
}' >/dev/null

echo "done — dashboards in dashboards/ are now two-way synced"
