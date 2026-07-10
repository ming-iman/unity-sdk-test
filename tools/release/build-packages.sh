#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
BUILD_DIR="$ROOT/build"

ADAPTERS=(
  nova
  google
  facebook
  unity
  inmobi
  mobilefuse
  mintegral
  pubmatic
  moloco
  amazon
  liftoff
  applovin
)

echo "[MSP Unity] Building Android bridge AAR into upm/Plugins/Android..."
(
  cd "$ROOT/android-bridge"
  UNITY_PLUGINS_DIR="$ROOT/upm/Plugins/Android" make install
)

echo "[MSP Unity] Validating publishable package layout..."
"$ROOT/tools/release/validate-packages.sh"

mkdir -p "$BUILD_DIR"
rm -f "$BUILD_DIR"/ai.themsp.unity.core-*.tgz "$BUILD_DIR"/ai.themsp.unity.adapter.*.tgz

echo "[MSP Unity] Packing core package..."
(
  cd "$ROOT/upm"
  npm pack --pack-destination "$BUILD_DIR"
)

for adapter in "${ADAPTERS[@]}"; do
  echo "[MSP Unity] Packing adapter-$adapter package..."
  (
    cd "$ROOT/packages/adapter-$adapter"
    npm pack --pack-destination "$BUILD_DIR"
  )
done

echo "[MSP Unity] Release artifacts:"
ls -1 "$BUILD_DIR"/ai.themsp.unity.core-*.tgz "$BUILD_DIR"/ai.themsp.unity.adapter.*.tgz
