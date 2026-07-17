#!/usr/bin/env bash
# Pin all MSP Unity SDK git packages in Packages/manifest.json from MSP_VERSION.
#
# Unity only accepts git URLs in the root manifest.json (not in local package dependencies).
#
# Usage:
#   MSP_VERSION=v4.5.0-rc.0 ./scripts/sync-msp-version.sh
#   ./scripts/sync-msp-version.sh v4.5.0-rc.0
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_DIR="$(cd "${SCRIPT_DIR}/.." && pwd)"
MANIFEST_FILE="${PROJECT_DIR}/Packages/manifest.json"
VERSION_FILE="${PROJECT_DIR}/Packages/local/msp-version"
REPO="https://github.com/ming-iman/unity-sdk-test.git"

raw_version="${MSP_VERSION:-${1:-}}"
if [[ -z "${raw_version}" ]]; then
  echo "Usage: MSP_VERSION=v4.5.0-rc.0 ${0}" >&2
  echo "   or: ${0} v4.5.0-rc.0" >&2
  exit 1
fi

tag="${raw_version#v}"
tag="v${tag}"

python3 - "${MANIFEST_FILE}" "${VERSION_FILE}" "${REPO}" "${tag}" <<'PY'
import json
import sys

manifest_path, version_path, repo, tag = sys.argv[1:5]

msp_packages = [
    ("ai.themsp.unity.core", "/upm"),
    ("ai.themsp.unity.adapter.nova", "/packages/adapter-nova"),
    ("ai.themsp.unity.adapter.google", "/packages/adapter-google"),
    ("ai.themsp.unity.adapter.facebook", "/packages/adapter-facebook"),
    ("ai.themsp.unity.adapter.liftoff", "/packages/adapter-liftoff"),
    ("ai.themsp.unity.adapter.moloco", "/packages/adapter-moloco"),
]

def git_url(path: str) -> str:
    return f"{repo}?path={path}#{tag}"

with open(manifest_path, encoding="utf-8") as handle:
    manifest = json.load(handle)

deps = manifest["dependencies"]
for key in list(deps):
    if key.startswith("ai.themsp.unity.") or key == "com.gridlight.msp-dependencies":
        del deps[key]

msp_deps = {name: git_url(path) for name, path in msp_packages}
manifest["dependencies"] = {**msp_deps, **deps}

with open(manifest_path, "w", encoding="utf-8") as handle:
    json.dump(manifest, handle, indent=2)
    handle.write("\n")

with open(version_path, "w", encoding="utf-8") as handle:
    handle.write(f"{tag}\n")

print(f"Updated {manifest_path} (tag: {tag})")
print(f"Wrote {version_path}")
PY

echo "Reopen Unity Package Manager to refresh packages-lock.json."
