#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/../.." && pwd)"

require_file() {
  local file="$1"
  if [[ ! -f "$file" ]]; then
    echo "Missing required file: $file" >&2
    exit 1
  fi
}

echo "[MSP Unity] Validating publishable package layout..."

require_file "$ROOT/upm/package.json"
require_file "$ROOT/upm/Runtime/Adapter/MSPUnityAdapterRegistry.cs"
require_file "$ROOT/upm/Runtime/Adapter/MSPUnityNativeVersions.cs"
require_file "$ROOT/upm/Runtime/Adapter/MSPUnityOptionalAdapterLoader.cs"
require_file "$ROOT/upm/Editor/MSPUnityIosAdapterBootstrapEnsurer.cs"
require_file "$ROOT/upm/Editor/MSPUnityIosPodfileBuilder.cs"
require_file "$ROOT/upm/Editor/Dependencies.xml"
require_file "$ROOT/packages/adapter-nova/package.json"
require_file "$ROOT/packages/adapter-nova/Runtime/NovaAdapterContributor.cs"
require_file "$ROOT/packages/adapter-nova/Editor/Dependencies.xml"
require_file "$ROOT/packages/adapter-nova/Plugins/iOS/MSPUnityNovaBootstrap.swift"
require_file "$ROOT/packages/adapter-nova/link.xml"
require_file "$ROOT/docs/publishing-layout.md"

if grep -q "artifactory.nb-sandbox.com" "$ROOT/upm/Editor/Dependencies.xml" "$ROOT/packages/adapter-nova/Editor/Dependencies.xml"; then
  echo "Dependencies.xml must not reference internal Artifactory for external release." >&2
  exit 1
fi

core_version="$(python3 -c "import json; print(json.load(open('$ROOT/upm/package.json'))['version'])")"
nova_version="$(python3 -c "import json; print(json.load(open('$ROOT/packages/adapter-nova/package.json'))['version'])")"

if [[ "$core_version" != "$nova_version" ]]; then
  echo "Version mismatch: core=$core_version nova=$nova_version" >&2
  exit 1
fi

echo "[MSP Unity] Package layout looks valid (version $core_version)."
