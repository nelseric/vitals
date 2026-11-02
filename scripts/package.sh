#!/usr/bin/env bash
# Usage: scripts/package.sh <version> — builds dist/BepInEx/plugins/Vitals and Vitals-<version>.zip.
set -euo pipefail

version="$1"

root="$(cd "$(dirname "$0")/.." && pwd)"
cd "$root"

dotnet build src/Vitals -c Release -p:Version="$version"

rm -rf dist "Vitals-$version.zip"
dest=dist/BepInEx/plugins/Vitals
mkdir -p "$dest"
cp src/Vitals/bin/Release/netstandard2.1/Vitals.dll README.md LICENSE "$dest/"
(cd dist && python3 -m zipfile -c "../Vitals-$version.zip" BepInEx)
echo "Packaged Vitals-$version.zip"
