#!/usr/bin/env bash
# RSD test runner.
# Requires Docker (for Testcontainers Postgres) and either the .NET 9 SDK
# or the .NET 10 SDK with DOTNET_ROLL_FORWARD=LatestMajor.
set -euo pipefail
cd "$(dirname "$0")"
DOTNET_ROLL_FORWARD="${DOTNET_ROLL_FORWARD:-LatestMajor}" dotnet test RSD.Web.Tests/RSD.Web.Tests.csproj "$@"
