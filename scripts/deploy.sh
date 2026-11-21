#!/usr/bin/env bash
#
# Deploy one app to CapRover.
#
# Default flow (image):  docker build local -> tag :<sha> + :latest ->
#   push to GHCR -> caprover deploy --imageName <tag-sha> (app token)
# Fallback flow (tarball): old tar + caprover deploy --tarFile (master password)
#
# Usage:
#   ./scripts/deploy.sh <app> [--tarball] [--yes]
#   app: server | miner | site | docs
#
# Secrets come from the root .env (never commit it) and are only passed via
# environment variables or stdin. Never in argv, never printed.

set -euo pipefail

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

die() { echo "ERROR: $*" >&2; exit 1; }
info() { echo "==> $*"; }

# ---------------------------------------------------------------------------
# Load config from root .env (if present)
# ---------------------------------------------------------------------------
if [ -f "$REPO_ROOT/.env" ]; then
  # Only export valid KEY=VALUE lines; never print their values.
  set -a
  # shellcheck disable=SC1091
  source "$REPO_ROOT/.env"
  set +a
else
  die "Root .env not found. Copy .env.example to .env and fill it in."
fi

: "${CAPROVER_URL:?CAPROVER_URL not set in .env}"
: "${GHCR_USER:?GHCR_USER not set in .env}"
: "${GHCR_TOKEN:?GHCR_TOKEN not set in .env}"
: "${GHCR_IMAGE_PREFIX:?GHCR_IMAGE_PREFIX not set in .env}"

# ---------------------------------------------------------------------------
# Parse args
# ---------------------------------------------------------------------------
APP=""
TARBALL=0
ASSUME_YES=0
for arg in "$@"; do
  case "$arg" in
    server|miner|site|docs) APP="$arg" ;;
    --tarball) TARBALL=1 ;;
    --yes) ASSUME_YES=1 ;;
    *) die "Unknown argument: $arg" ;;
  esac
done
[ -n "$APP" ] || die "Usage: ./scripts/deploy.sh <server|miner|site|docs> [--tarball] [--yes]"

# ---------------------------------------------------------------------------
# Per-app config
# ---------------------------------------------------------------------------
case "$APP" in
  server)
    CAPROVER_APP="server-ef-blockchain"
    IMAGE_NAME="server-ef-blockchain"
    BUILD_CONTEXT="$REPO_ROOT/backend/EF.Blockchain"
    DOCKERFILE="Dockerfile.Server"
    TOKEN_VAR="CAPROVER_APP_TOKEN_SERVER"
    ;;
  miner)
    CAPROVER_APP="miner-ef-blockchain"
    IMAGE_NAME="miner-ef-blockchain"
    BUILD_CONTEXT="$REPO_ROOT/backend/EF.Blockchain"
    DOCKERFILE="Dockerfile.Miner"
    TOKEN_VAR="CAPROVER_APP_TOKEN_MINER"
    ;;
  site)
    CAPROVER_APP="ef-blockchain"
    IMAGE_NAME="ef-blockchain"
    BUILD_CONTEXT="$REPO_ROOT/frontend/site"
    DOCKERFILE="Dockerfile"
    TOKEN_VAR="CAPROVER_APP_TOKEN_SITE"
    ;;
  docs)
    CAPROVER_APP="docs-ef-blockchain"
    IMAGE_NAME="docs-ef-blockchain"
    BUILD_CONTEXT="$REPO_ROOT/frontend/docs"
    DOCKERFILE="Dockerfile"
    TOKEN_VAR="CAPROVER_APP_TOKEN_DOCS"
    ;;
esac

APP_TOKEN="${!TOKEN_VAR:-}"
[ -n "$APP_TOKEN" ] || die "$TOKEN_VAR not set in .env (CapRover app token)"

# ---------------------------------------------------------------------------
# Pre-build: quality gates + env-file swap
# ---------------------------------------------------------------------------
MINER_SETTINGS="$REPO_ROOT/backend/EF.Blockchain/src/EF.Blockchain.Client/appsettings.json"
MINER_SETTINGS_PROD="$REPO_ROOT/backend/EF.Blockchain/src/EF.Blockchain.Client/appsettings.Production.json"
MINER_SWAPPED=0

swap_miner_settings() {
  if git -C "$REPO_ROOT" diff --quiet -- "$MINER_SETTINGS"; then
    cp "$MINER_SETTINGS_PROD" "$MINER_SETTINGS"
    MINER_SWAPPED=1
    info "Swapped appsettings.Production.json over appsettings.json"
  else
    info "appsettings.json has local changes; not swapping (using file as-is)"
  fi
}

restore_miner_settings() {
  if [ "$MINER_SWAPPED" -eq 1 ]; then
    git -C "$REPO_ROOT" checkout -- "$MINER_SETTINGS"
    info "Restored appsettings.json"
  fi
}

# Quality gates go here, in order, before any build.
# Currently none (documented in DEPLOY.md). Examples:
#   dotnet build backend/EF.Blockchain/EF.Blockchain.sln
#   npm --prefix frontend/site run build:check

if [ "$APP" = "miner" ]; then
  swap_miner_settings
fi

# ---------------------------------------------------------------------------
# Docker Desktop / daemon check
# ---------------------------------------------------------------------------
if ! docker info >/dev/null 2>&1; then
  restore_miner_settings
  die "Docker is not running. Start Docker Desktop and try again."
fi

SHORT_SHA="$(git -C "$REPO_ROOT" rev-parse --short HEAD)"
IMAGE_SHA="ghcr.io/${GHCR_IMAGE_PREFIX}/${IMAGE_NAME}:${SHORT_SHA}"
IMAGE_LATEST="ghcr.io/${GHCR_IMAGE_PREFIX}/${IMAGE_NAME}:latest"

# ---------------------------------------------------------------------------
# Fallback: tarball deploy (old flow)
# ---------------------------------------------------------------------------
deploy_tarball() {
  local tar_file="${REPO_ROOT}/build-${CAPROVER_APP}.tar"
  info "Building tarball (legacy flow): $tar_file"

  if [ "$APP" = "server" ] || [ "$APP" = "miner" ]; then
    if git -C "$REPO_ROOT" diff --quiet -- "$REPO_ROOT/backend/EF.Blockchain/Dockerfile"; then
      cp "${BUILD_CONTEXT}/Dockerfile.${APP^}" "${BUILD_CONTEXT}/Dockerfile"
      BACKEND_DOCKERFILE_SWAPPED=1
    else
      info "Dockerfile has local changes; using it as-is for the tarball"
    fi
  fi

  case "$APP" in
    server|miner)
      tar --exclude='*/bin*' --exclude='*/obj*' --exclude='*/.vs*' \
          --exclude='*/tests*' --exclude='*/docs*' \
          --exclude='*/coveragereport*' \
          -cvf "$tar_file" -C "$BUILD_CONTEXT" .
      ;;
    site)
      tar --exclude='*/node_modules*' --exclude='*/dist*' \
          -cvf "$tar_file" -C "$BUILD_CONTEXT" .
      ;;
    docs)
      tar --exclude='*/node_modules*' \
          -cvf "$tar_file" -C "$BUILD_CONTEXT" .
      ;;
  esac

  if [ "${BACKEND_DOCKERFILE_SWAPPED:-0}" -eq 1 ]; then
    git -C "$REPO_ROOT" checkout -- "$REPO_ROOT/backend/EF.Blockchain/Dockerfile"
    BACKEND_DOCKERFILE_SWAPPED=0
  fi

  info "Deploying tarball to CapRover app '$CAPROVER_APP'"
  CAPROVER_URL="$CAPROVER_URL" \
  CAPROVER_APP="$CAPROVER_APP" \
  CAPROVER_PASSWORD="${CAPROVER_PASSWORD:-}" \
    npx caprover deploy -t "$tar_file"
}

# ---------------------------------------------------------------------------
# Default: image build + push + deploy
# ---------------------------------------------------------------------------
deploy_image() {
  info "Building image $IMAGE_SHA (and :latest)"
  if [ "$APP" = "site" ]; then
    : "${VITE_API_URL:?VITE_API_URL not set in .env}"
    docker build \
      --build-arg "VITE_API_URL=$VITE_API_URL" \
      -f "$BUILD_CONTEXT/$DOCKERFILE" \
      -t "$IMAGE_SHA" -t "$IMAGE_LATEST" \
      "$BUILD_CONTEXT"
  else
    docker build \
      -f "$BUILD_CONTEXT/$DOCKERFILE" \
      -t "$IMAGE_SHA" -t "$IMAGE_LATEST" \
      "$BUILD_CONTEXT"
  fi

  info "Logging in to ghcr.io"
  printf '%s' "$GHCR_TOKEN" | docker login ghcr.io -u "$GHCR_USER" --password-stdin >/dev/null

  info "Pushing $IMAGE_SHA and :latest"
  docker push "$IMAGE_SHA"
  docker push "$IMAGE_LATEST"

  if [ "$ASSUME_YES" -ne 1 ]; then
    read -r -p "Deploy $CAPROVER_APP ($IMAGE_SHA) to production? [y/N] " confirm
    case "$confirm" in
      y|Y|yes|YES) ;;
      *) restore_miner_settings; die "Aborted by user. Nothing was deployed." ;;
    esac
  fi

  info "Deploying image to CapRover app '$CAPROVER_APP'"
  CAPROVER_URL="$CAPROVER_URL" \
  CAPROVER_APP="$CAPROVER_APP" \
  CAPROVER_APP_TOKEN="$APP_TOKEN" \
    npx caprover deploy -i "$IMAGE_SHA"

  info "Done. Deployed $IMAGE_SHA to $CAPROVER_APP"
  echo "    Rollback (see DEPLOY.md):"
  echo "    CAPROVER_URL=... CAPROVER_APP=$CAPROVER_APP CAPROVER_APP_TOKEN=<token> \\"
  echo "      npx caprover deploy -i ghcr.io/${GHCR_IMAGE_PREFIX}/${IMAGE_NAME}:<old-sha>"
}

# ---------------------------------------------------------------------------
if [ "$TARBALL" -eq 1 ]; then
  if [ -z "${CAPROVER_PASSWORD:-}" ]; then
    restore_miner_settings
    die "CAPROVER_PASSWORD not set in .env (required for --tarball fallback)"
  fi
  trap restore_miner_settings EXIT
  deploy_tarball
else
  trap restore_miner_settings EXIT
  deploy_image
fi
