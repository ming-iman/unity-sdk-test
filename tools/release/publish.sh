#!/usr/bin/env bash
# Publish a Unity SDK version: bump VERSION, sync package.json, commit, tag, push to public.
#
# Usage:
#   ./tools/release/publish.sh <version> [options]
#
# Examples:
#   ./tools/release/publish.sh 0.0.1-rc.3
#   ./tools/release/publish.sh 0.0.2 --pack
#   ./tools/release/publish.sh 0.0.1-rc.3 --dry-run
#
# Options:
#   --pack         Also run build-packages.sh (rebuild AAR + npm pack into build/)
#   --dry-run      Print actions without writing files / committing / pushing
#   --skip-push    Commit + tag locally, do not push remotes
#   --no-origin    Push only the `public` remote (default also pushes `origin`)
#   --allow-dirty  Allow a non-clean working tree (unrelated files stay unstaged)
#
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
VERSION_FILE="$ROOT/tools/release/VERSION"
PUBLIC_REMOTE="${MSP_UNITY_PUBLIC_REMOTE:-public}"
ORIGIN_REMOTE="${MSP_UNITY_ORIGIN_REMOTE:-origin}"

PACK=0
DRY_RUN=0
SKIP_PUSH=0
PUSH_ORIGIN=1
ALLOW_DIRTY=0
VERSION=""

usage() {
  awk '
    /^# Publish a Unity SDK version/ {printing=1}
    printing && /^set -euo pipefail/ {exit}
    printing && /^#/ {
      sub(/^# ?/, "")
      print
    }
  ' "$0"
  exit "${1:-0}"
}

while [[ $# -gt 0 ]]; do
  case "$1" in
    -h|--help) usage 0 ;;
    --pack) PACK=1; shift ;;
    --dry-run) DRY_RUN=1; shift ;;
    --skip-push) SKIP_PUSH=1; shift ;;
    --no-origin) PUSH_ORIGIN=0; shift ;;
    --allow-dirty) ALLOW_DIRTY=1; shift ;;
    -*)
      echo "Unknown option: $1" >&2
      usage 1
      ;;
    *)
      if [[ -n "$VERSION" ]]; then
        echo "Unexpected argument: $1" >&2
        usage 1
      fi
      VERSION="$1"
      shift
      ;;
  esac
done

if [[ -z "$VERSION" ]]; then
  echo "Missing version. Example: $0 0.0.1-rc.3" >&2
  usage 1
fi

if [[ ! "$VERSION" =~ ^[0-9]+\.[0-9]+\.[0-9]+([.-][A-Za-z0-9._-]+)?$ ]]; then
  echo "Invalid version: $VERSION" >&2
  exit 1
fi

TAG="v${VERSION}"

cd "$ROOT"

PREVIOUS_VERSION=""
if [[ -f "$VERSION_FILE" ]]; then
  PREVIOUS_VERSION="$(tr -d '[:space:]' < "$VERSION_FILE")"
fi

if [[ "$PREVIOUS_VERSION" == "$VERSION" ]]; then
  echo "VERSION is already $VERSION. Refusing to republish the same version." >&2
  exit 1
fi

if [[ "$ALLOW_DIRTY" -ne 1 && "$DRY_RUN" -ne 1 ]]; then
  if [[ -n "$(git status --porcelain)" ]]; then
    echo "Working tree is not clean. Commit/stash first, or pass --allow-dirty." >&2
    git status --short >&2
    exit 1
  fi
fi

if git rev-parse "$TAG" >/dev/null 2>&1; then
  echo "Tag already exists locally: $TAG" >&2
  exit 1
fi

if ! git remote get-url "$PUBLIC_REMOTE" >/dev/null 2>&1; then
  echo "Missing git remote '$PUBLIC_REMOTE'. Add it, e.g.:" >&2
  echo "  git remote add public git@github.com:<org>/<repo>.git" >&2
  exit 1
fi

if [[ "$PUSH_ORIGIN" -eq 1 ]] && ! git remote get-url "$ORIGIN_REMOTE" >/dev/null 2>&1; then
  echo "Missing git remote '$ORIGIN_REMOTE' (use --no-origin to skip)." >&2
  exit 1
fi

run() {
  if [[ "$DRY_RUN" -eq 1 ]]; then
    echo "+ $*"
  else
    "$@"
  fi
}

echo "[MSP Unity] Publishing $VERSION (tag $TAG)"
if [[ -n "$PREVIOUS_VERSION" ]]; then
  echo "[MSP Unity] Previous VERSION: $PREVIOUS_VERSION"
fi

if [[ "$DRY_RUN" -eq 1 ]]; then
  echo "+ write $VERSION_FILE"
else
  printf '%s\n' "$VERSION" > "$VERSION_FILE"
fi

run "$ROOT/tools/release/sync-package-versions.sh"

if [[ "$PACK" -eq 1 ]]; then
  run "$ROOT/tools/release/build-packages.sh"
else
  run "$ROOT/tools/release/validate-packages.sh"
fi

echo "[MSP Unity] Updating docs/demo git-link examples to $TAG..."
if [[ "$DRY_RUN" -eq 1 ]]; then
  echo "+ rewrite README/docs/demo package refs"
else
  python3 - "$ROOT" "$VERSION" "$PREVIOUS_VERSION" <<'PY'
import json
import re
import sys
from pathlib import Path

root = Path(sys.argv[1])
version = sys.argv[2]
previous = sys.argv[3]
tag = f"v{version}"
prev_tag = f"v{previous}" if previous else ""

text_files = [
    root / "README.md",
    root / "docs" / "publishing-layout.md",
    root / "demo" / "Packages" / "manifest.json",
    root / "demo" / "Packages" / "packages-lock.json",
]

for path in text_files:
    if not path.is_file():
        continue
    text = path.read_text()
    original = text
    # Prefer precise previous→new replacement when available.
    if previous:
        text = text.replace(prev_tag, tag)
        text = text.replace(previous, version)
    # Normalize any remaining v0.0.x git fragments / version mentions in examples.
    text = re.sub(r"#v\d+\.\d+\.\d+(?:[.-][A-Za-z0-9._-]+)?", f"#{tag}", text)
    text = re.sub(
        r"(ai\.themsp\.unity\.(?:core|adapter\.[a-z0-9-]+)-)\d+\.\d+\.\d+(?:[.-][A-Za-z0-9._-]+)?(\.tgz)",
        rf"\g<1>{version}\2",
        text,
    )
    text = re.sub(
        r"(\(now `)[^`]+(`\))",
        rf"\g<1>{version}\2",
        text,
        count=1,
    )
    if text != original:
        path.write_text(text)
        print(f"[MSP Unity] Updated refs in {path.relative_to(root)}")

lock_path = root / "demo" / "Packages" / "packages-lock.json"
if lock_path.is_file():
    data = json.loads(lock_path.read_text())
    deps = data.get("dependencies", {})
    for name, entry in deps.items():
        if not name.startswith("ai.themsp.unity."):
            continue
        if isinstance(entry, dict) and "dependencies" in entry:
            core = entry["dependencies"].get("ai.themsp.unity.core")
            if core is not None:
                entry["dependencies"]["ai.themsp.unity.core"] = version
    lock_path.write_text(json.dumps(data, indent=2) + "\n")
PY
fi

COMMIT_MSG="chore(release): bump Unity package version to ${VERSION}"
TAG_MSG="MSP Unity SDK ${VERSION}"

run git add \
  "$VERSION_FILE" \
  "$ROOT/upm/package.json" \
  "$ROOT"/packages/adapter-*/package.json \
  "$ROOT/README.md" \
  "$ROOT/docs/publishing-layout.md" \
  "$ROOT/demo/Packages/manifest.json" \
  "$ROOT/demo/Packages/packages-lock.json"

if [[ "$PACK" -eq 1 ]]; then
  # build-packages may refresh the bridge AAR inside upm/
  if [[ -f "$ROOT/upm/Plugins/Android/msp-unity-bridge-release.aar" ]]; then
    run git add "$ROOT/upm/Plugins/Android/msp-unity-bridge-release.aar"
  fi
fi

if [[ "$DRY_RUN" -eq 1 ]]; then
  echo "+ git commit -m \"$COMMIT_MSG\""
  echo "+ git tag -a $TAG -m \"$TAG_MSG\""
else
  if git diff --cached --quiet; then
    echo "Nothing staged for release commit (already up to date?)." >&2
    exit 1
  fi
  git commit -m "$COMMIT_MSG"
  git tag -a "$TAG" -m "$TAG_MSG"
fi

if [[ "$SKIP_PUSH" -eq 1 ]]; then
  echo "[MSP Unity] Skipped push (--skip-push). Local tag: $TAG"
  exit 0
fi

BRANCH="$(git rev-parse --abbrev-ref HEAD)"
if [[ "$BRANCH" == "HEAD" ]]; then
  echo "Detached HEAD; refusing to push." >&2
  exit 1
fi

if [[ "$PUSH_ORIGIN" -eq 1 ]]; then
  run git push "$ORIGIN_REMOTE" "$BRANCH"
  run git push "$ORIGIN_REMOTE" "$TAG"
fi
run git push "$PUBLIC_REMOTE" "$BRANCH"
run git push "$PUBLIC_REMOTE" "$TAG"

echo "[MSP Unity] Published $TAG"
echo "[MSP Unity] Public install example:"
PUBLIC_URL="$(git remote get-url "$PUBLIC_REMOTE")"
# normalize git@github.com:org/repo.git -> https://github.com/org/repo.git
PUBLIC_HTTPS="$PUBLIC_URL"
if [[ "$PUBLIC_URL" =~ ^git@github\.com:(.+)$ ]]; then
  PUBLIC_HTTPS="https://github.com/${BASH_REMATCH[1]}"
fi
PUBLIC_HTTPS="${PUBLIC_HTTPS%.git}.git"
echo "  \"ai.themsp.unity.core\": \"${PUBLIC_HTTPS}?path=/upm#${TAG}\""
echo "  \"ai.themsp.unity.adapter.nova\": \"${PUBLIC_HTTPS}?path=/packages/adapter-nova#${TAG}\""
