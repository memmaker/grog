#!/bin/sh
# RVIP ASan substitute: random-key runs, each followed by save -> load in a new process.
# usage: sh web/native/run-seeds.sh [first-seed] [count] [keys]
command -v dotnet >/dev/null || { export PATH="$HOME/.dotnet:$PATH" DOTNET_ROOT="$HOME/.dotnet"; }
HERE=$(cd "$(dirname "$0")" && pwd)
DLL="$HERE/bin/Release/net10.0/GrogNative.dll"
dotnet build -c Release "$HERE" -nologo -v q >/dev/null || exit 1
first=${1:-1}; count=${2:-10}; keys=${3:-3000}
fail=0
s=$first
while [ $s -lt $((first+count)) ]; do
  d=$(mktemp -d)
  (cd "$d" && dotnet "$DLL" $s $keys save) || fail=1
  if ls "$d"/grog42_* >/dev/null 2>&1; then
    (cd "$d" && dotnet "$DLL" $((s+100000)) $keys save) || fail=1
  fi
  rm -rf "$d"
  s=$((s+1))
done
exit $fail
