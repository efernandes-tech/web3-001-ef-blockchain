# Deploy to CapRover

Deploy script: `scripts/deploy.sh`. Deployable apps:

| Script arg | CapRover app name | Source | Dockerfile |
|---|---|---|---|
| `server` | `server-ef-blockchain` | `backend/EF.Blockchain` | `Dockerfile.Server` |
| `miner` | `miner-ef-blockchain` | `backend/EF.Blockchain` | `Dockerfile.Miner` |
| `site` | `ef-blockchain` | `frontend/site` | `Dockerfile` |
| `docs` | `docs-ef-blockchain` | `frontend/docs` | `Dockerfile` |

## Requirements

- **Docker Desktop running** (local build). The script fails fast if the daemon is down.
- Node.js (for `npx caprover`).
- Root `.env` with deploy config. Copy `.env.example` to `.env` and fill in:

  - `GHCR_USER`, `GHCR_TOKEN` (classic PAT with `read:packages` + `write:packages`)
  - `GHCR_IMAGE_PREFIX` (GitHub username, lowercase)
  - `CAPROVER_URL`
  - `CAPROVER_APP_TOKEN_SERVER` / `_MINER` / `_SITE` / `_DOCS` (app tokens, not master password)
  - `VITE_API_URL` (baked into the site build)
  - `CAPROVER_PASSWORD` only if you use the `--tarball` fallback

## Default flow (image)

```
./scripts/deploy.sh server            # prompts before deploying
./scripts/deploy.sh server --yes      # skips the confirmation
```

Steps, in order:

1. Quality gates (currently none; hooks exist in the script for adding tests/audit later).
2. Miner only: swaps `appsettings.Production.json` over `appsettings.json` before the build
   and restores the file afterwards (only if it had no local changes).
3. `docker build` with two tags: `ghcr.io/<GHCR_IMAGE_PREFIX>/<app>:<commit-sha>` and `:latest`.
4. `docker login ghcr.io` using the PAT via `--password-stdin`.
5. Push both tags.
6. Explicit confirmation prompt (skipped with `--yes`), then
   `caprover deploy -i <tag-sha>` using the **app token** via the
   `CAPROVER_APP_TOKEN` environment variable. Secrets never appear in argv or logs.

The image lives in GHCR, so it survives image pruning on the VPS. The "No such image"
problem no longer breaks deploys or rollbacks.

## Fallback flow (tarball)

```
./scripts/deploy.sh server --tarball
```

Recreates the old flow: creates `build-<app>.tar`, uses the master password
(`CAPROVER_PASSWORD`) to upload and build on the VPS. Use only if the image flow is broken.

## Rollback

Redeploy an old SHA tag (the image is still in GHCR):

```
CAPROVER_URL=... CAPROVER_APP=server-ef-blockchain CAPROVER_APP_TOKEN=<token> \
  npx caprover deploy -i ghcr.io/<GHCR_IMAGE_PREFIX>/server-ef-blockchain:<old-sha>
```

Or add a `rollback` flag to `scripts/deploy.sh` if this becomes frequent.

## One-time setup (outside the repo)

1. **GitHub Classic PAT**: `read:packages` + `write:packages`.
2. **CapRover app token per app**: dashboard -> App -> Deployment tab -> Enable App Token.
3. **Registry in CapRover**: Infrastructure -> Docker Registries -> Remote Registry:
   - Username: GitHub username (lowercase)
   - Password: the PAT
   - Domain: `ghcr.io` (host only, no `https://`)
   - Image Prefix: your GitHub username
   - Saving restarts Captain. That is normal.
4. **Verify credentials first**: `echo <PAT> | docker login ghcr.io -u <user> --password-stdin`
   before blaming CapRover.
