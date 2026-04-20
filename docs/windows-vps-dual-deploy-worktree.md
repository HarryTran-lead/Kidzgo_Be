# Windows VPS dual deploy with git worktree

This setup runs two API instances on the same Windows VPS:

- main/stable instance
- dev/staging instance

Both instances are deployed from the same git repository by using `git worktree`.

## 1. Prepare worktrees

Run on the VPS:

```powershell
cd C:\Users\Administrator\Desktop\Projects\Kidzgo
.\scripts\setup-git-worktrees-win.ps1
```

Default result:

- main worktree: `C:\Users\Administrator\Desktop\Projects\Kidzgo.Worktrees\main`
- dev worktree: `C:\Users\Administrator\Desktop\Projects\Kidzgo.Worktrees\dev`
- local branch `vps-main` tracks `origin/main`
- local branch `vps-dev` tracks `origin/dev`

## 2. Create two published app folders

Recommended folders:

- `C:\apps\kidzgo-api-main`
- `C:\apps\kidzgo-api-dev`

Each folder keeps its own `appsettings.Production.local.json`.

The deploy script now preserves local config files and injects instance-specific overrides:

- Kestrel bind URL
- public API URL
- Windows event log source

## 3. Create two service instances

Reuse the same service mechanism you already use on the VPS (for example NSSM), but create two service names:

- `KidzgoAPI-Main`
- `KidzgoAPI-Dev`

Point each service to the matching published folder.

Important:

- do not use the same service for both instances
- do not use the same published folder for both instances
- do not share the same database between main and dev

## 4. Deploy

Main:

```powershell
cd C:\Users\Administrator\Desktop\Projects\Kidzgo
.\deploy-main-win.ps1
```

Dev:

```powershell
cd C:\Users\Administrator\Desktop\Projects\Kidzgo
.\deploy-dev-win.ps1
```

Default bindings:

- main -> `http://127.0.0.1:5000`
- dev -> `http://127.0.0.1:5001`

Default public URLs:

- main -> `https://api.kidzgo.vn`
- dev -> `https://dev-api.kidzgo.vn`

Adjust them in the wrapper scripts if your VPS uses different domains.

## 5. Configure reverse proxy

Install dual-site Caddy config:

```powershell
cd C:\Users\Administrator\Desktop\Projects\Kidzgo
.\scripts\install-caddy-dual-win.ps1
```

That script creates two Caddy routes:

- `api.kidzgo.vn` -> `127.0.0.1:5000`
- `dev-api.kidzgo.vn` -> `127.0.0.1:5001`

## 6. Database separation

Use different databases, for example:

- `kidzgo_prod`
- `kidzgo_dev`

Store the connection string for each instance inside its own:

- `C:\apps\kidzgo-api-main\appsettings.Production.local.json`
- `C:\apps\kidzgo-api-dev\appsettings.Production.local.json`

Never let dev migrations run against the production database.
