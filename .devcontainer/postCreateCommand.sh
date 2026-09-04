#!/usr/bin/env bash
set -Eeuo pipefail

workspace_dir="${1:-$PWD}"

cd "$workspace_dir"

if [ -d "$HOME/.1password" ]; then
    sudo chown vscode:vscode "$HOME/.1password" || true
    chmod 700 "$HOME/.1password" || true
fi

git config --global --unset-all include.path >/dev/null 2>&1 || true
if [ -f "$HOME/.gitconfig-host" ]; then
    git config --global --add include.path "$HOME/.gitconfig-host"
fi

corepack enable
corepack prepare yarn@1.22.19 --activate

git config --global --add safe.directory "$workspace_dir"
git lfs install --skip-repo

yarn install --frozen-lockfile --network-timeout 120000
dotnet restore src/Sonarr.sln
