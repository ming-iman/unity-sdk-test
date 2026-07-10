#!/usr/bin/env bash
# Sync Unity package versions from tools/release/VERSION into all package.json files.
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
VERSION_FILE="$ROOT/tools/release/VERSION"

if [[ ! -f "$VERSION_FILE" ]]; then
  echo "Missing version file: $VERSION_FILE" >&2
  exit 1
fi

VERSION="$(tr -d '[:space:]' < "$VERSION_FILE")"
if [[ -z "$VERSION" ]]; then
  echo "VERSION file is empty: $VERSION_FILE" >&2
  exit 1
fi

python3 - "$ROOT" "$VERSION" <<'PY'
import json
import sys
from pathlib import Path

root = Path(sys.argv[1])
version = sys.argv[2]

package_paths = [root / "upm" / "package.json"]
packages_dir = root / "packages"
if packages_dir.is_dir():
    package_paths.extend(sorted(packages_dir.glob("adapter-*/package.json")))

updated = 0
for path in package_paths:
    data = json.loads(path.read_text())
    changed = False
    if data.get("version") != version:
        data["version"] = version
        changed = True

    deps = data.get("dependencies")
    if isinstance(deps, dict) and "ai.themsp.unity.core" in deps:
        if deps["ai.themsp.unity.core"] != version:
            deps["ai.themsp.unity.core"] = version
            changed = True

    if changed:
        path.write_text(json.dumps(data, indent=2) + "\n")
        updated += 1
        print(f"[MSP Unity] Synced version {version} -> {path.relative_to(root)}")

print(f"[MSP Unity] Package version is {version} ({updated} file(s) updated).")
PY
