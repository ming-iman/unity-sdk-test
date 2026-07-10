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

# adapter_id:PascalName pairs
ADAPTERS=(
  "nova:Nova"
  "google:Google"
  "facebook:Facebook"
  "unity:UnityAds"
  "inmobi:Inmobi"
  "mobilefuse:Mobilefuse"
  "mintegral:Mintegral"
  "pubmatic:Pubmatic"
  "moloco:Moloco"
  "amazon:Amazon"
  "liftoff:Liftoff"
  "applovin:Applovin"
)

echo "[MSP Unity] Validating publishable package layout..."

require_file "$ROOT/upm/package.json"
require_file "$ROOT/upm/Runtime/Adapter/MSPUnityAdapterRegistry.cs"
require_file "$ROOT/upm/Runtime/Adapter/MSPUnityNativeVersions.cs"
require_file "$ROOT/upm/Runtime/Adapter/MSPUnityOptionalAdapterLoader.cs"
require_file "$ROOT/upm/Editor/MSPUnityIosAdapterBootstrapEnsurer.cs"
require_file "$ROOT/upm/Editor/MSPUnityIosPodfileBuilder.cs"
require_file "$ROOT/upm/Editor/Dependencies.xml"
require_file "$ROOT/upm/Plugins/Android/msp-unity-bridge-release.aar"
require_file "$ROOT/docs/publishing-layout.md"

core_version="$(python3 -c "import json; print(json.load(open('$ROOT/upm/package.json'))['version'])")"

for entry in "${ADAPTERS[@]}"; do
  adapter="${entry%%:*}"
  pascal="${entry##*:}"
  adapter_root="$ROOT/packages/adapter-$adapter"
  require_file "$adapter_root/package.json"
  require_file "$adapter_root/Runtime/${pascal}AdapterContributor.cs"
  require_file "$adapter_root/Editor/Dependencies.xml"
  require_file "$adapter_root/Plugins/iOS/MSPUnity${pascal}Bootstrap.swift"
  require_file "$adapter_root/link.xml"

  if grep -q "artifactory.nb-sandbox.com" "$adapter_root/Editor/Dependencies.xml"; then
    echo "Dependencies.xml must not reference internal Artifactory for external release: $adapter" >&2
    exit 1
  fi

  adapter_version="$(python3 -c "import json; print(json.load(open('$adapter_root/package.json'))['version'])")"
  if [[ "$core_version" != "$adapter_version" ]]; then
    echo "Version mismatch: core=$core_version adapter-$adapter=$adapter_version" >&2
    exit 1
  fi
done

if grep -q "artifactory.nb-sandbox.com" "$ROOT/upm/Editor/Dependencies.xml"; then
  echo "Dependencies.xml must not reference internal Artifactory for external release." >&2
  exit 1
fi

echo "[MSP Unity] Package layout looks valid (version $core_version, ${#ADAPTERS[@]} adapters)."
