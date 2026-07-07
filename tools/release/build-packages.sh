#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/../.." && pwd)"

echo "[MSP Unity] Building Android bridge AAR into upm/Plugins/Android..."
(
  cd "$ROOT/android-bridge"
  UNITY_PLUGINS_DIR="$ROOT/upm/Plugins/Android" make install
)

echo "[MSP Unity] Validating publishable package layout..."
"$ROOT/tools/release/validate-packages.sh"

echo "[MSP Unity] Packing core package..."
(
  cd "$ROOT/upm"
  rm -f ai.themsp.unity.core-*.tgz
  npm pack
)

echo "[MSP Unity] Packing Nova adapter package..."
(
  cd "$ROOT/packages/adapter-nova"
  rm -f ai.themsp.unity.adapter.nova-*.tgz
  npm pack
)

echo "[MSP Unity] Release artifacts:"
ls -1 \
  "$ROOT/upm"/ai.themsp.unity.core-*.tgz \
  "$ROOT/packages/adapter-nova"/ai.themsp.unity.adapter.nova-*.tgz
