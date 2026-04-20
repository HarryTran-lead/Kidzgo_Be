# Windows VPS dual deploy with git worktree

Tai lieu nay mo ta cach chay dong thoi 2 ban backend tren cung mot Windows VPS:

- `main`: ban on dinh cho production
- `dev`: ban de tiep tuc code va deploy test

Muc tieu la:

- chi dung 1 repo git goc
- tach ra 2 worktree rieng
- moi worktree deploy ra 1 thu muc publish rieng
- moi ban chay bang 1 Windows Service rieng
- moi ban di qua 1 domain rieng bang Caddy

## 1. Mo hinh sau khi setup

Sau khi lam xong, VPS se co cau truc nhu sau:

- repo goc: `C:\Users\Administrator\Desktop\Projects\Kidzgo`
- worktree main: `C:\Users\Administrator\Desktop\Projects\Kidzgo.Worktrees\main`
- worktree dev: `C:\Users\Administrator\Desktop\Projects\Kidzgo.Worktrees\dev`
- publish main: `C:\apps\kidzgo-api-main`
- publish dev: `C:\apps\kidzgo-api-dev`
- service main: `KidzgoAPI-Main`
- service dev: `KidzgoAPI-Dev`
- local bind main: `127.0.0.1:5000`
- local bind dev: `127.0.0.1:5001`
- public main: `https://api.kidzgo.vn`
- public dev: `https://dev-api.kidzgo.vn`

Khong duoc dung chung:

- thu muc publish
- service name
- database
- domain public

## 2. Dieu kien truoc khi chay

Can dam bao VPS da co:

- Git
- .NET SDK/runtime dung voi project
- NSSM neu dang dung NSSM de wrap app thanh Windows Service
- Caddy neu muon public ra domain

Neu chua co repo goc tren VPS:

```powershell
cd C:\Users\Administrator\Desktop\Projects
git clone <repo-url> Kidzgo
cd Kidzgo
git fetch origin --prune
```

Neu repo da ton tai, chi can:

```powershell
cd C:\Users\Administrator\Desktop\Projects\Kidzgo
git fetch origin --prune
```

## 3. Tao worktree tren VPS

### 3.1. Truong hop remote da co ca `main` va `dev`

```powershell
cd C:\Users\Administrator\Desktop\Projects\Kidzgo
.\scripts\setup-git-worktrees-win.ps1
```

### 3.2. Truong hop remote chua co branch `dev`

Dung lenh nay de script tu tao `origin/dev` tu `origin/main`:

```powershell
cd C:\Users\Administrator\Desktop\Projects\Kidzgo
.\scripts\setup-git-worktrees-win.ps1 -CreateMissingRemoteBranches
```

Script se tao:

- local branch `vps-main` track `origin/main`
- local branch `vps-dev` track `origin/dev`
- worktree `main`
- worktree `dev`

### 3.3. Neu lan chay truoc bi loi giua chung

Vi du `main` da tao xong, `dev` fail giua duong, thi chay:

```powershell
cd C:\Users\Administrator\Desktop\Projects\Kidzgo
git worktree prune
Remove-Item "C:\Users\Administrator\Desktop\Projects\Kidzgo.Worktrees\dev" -Recurse -Force -ErrorAction SilentlyContinue
.\scripts\setup-git-worktrees-win.ps1 -CreateMissingRemoteBranches
```

### 3.4. Kiem tra ket qua

```powershell
cd C:\Users\Administrator\Desktop\Projects\Kidzgo
git worktree list
```

Ky vong thay:

```text
C:\Users\Administrator\Desktop\Projects\Kidzgo
C:\Users\Administrator\Desktop\Projects\Kidzgo.Worktrees\main [vps-main]
C:\Users\Administrator\Desktop\Projects\Kidzgo.Worktrees\dev  [vps-dev]
```

## 4. Tao thu muc publish

Tao san 2 thu muc publish:

```powershell
New-Item -ItemType Directory -Force -Path C:\apps\kidzgo-api-main
New-Item -ItemType Directory -Force -Path C:\apps\kidzgo-api-dev
```

Moi thu muc publish se duoc deploy script cap nhat file:

- `appsettings.Local.json` neu da ton tai
- `appsettings.Production.local.json`

Deploy script se tu inject:

- bind URL cua tung instance
- public API URL cua tung instance
- event log source cho tung service

## 5. Cau hinh database rieng

Bat buoc tach database:

- production: `kidzgo_prod`
- dev: `kidzgo_dev`

Sau khi deploy lan dau, kiem tra hoac tao file:

- `C:\apps\kidzgo-api-main\appsettings.Production.local.json`
- `C:\apps\kidzgo-api-dev\appsettings.Production.local.json`

Trong moi file, dat connection string rieng. Vi du:

```json
{
  "ConnectionStrings": {
    "Database": "Host=127.0.0.1;Port=5432;Database=kidzgo_prod;Username=postgres;Password=your_password"
  }
}
```

Ban dev dung `kidzgo_dev`.

Neu dev chay migration, tuyet doi khong tro vao DB production.

## 6. Bootstrap publish lan dau

Lan dau tien, thu muc publish chua co `Kidzgo.API.exe`, nen ban can publish tay mot lan truoc khi tao service.

### 6.1. Publish tay cho main

```powershell
cd C:\Users\Administrator\Desktop\Projects\Kidzgo.Worktrees\main
dotnet publish .\Kidzgo.API\Kidzgo.API.csproj -c Release -o C:\apps\kidzgo-api-main
```

### 6.2. Publish tay cho dev

```powershell
cd C:\Users\Administrator\Desktop\Projects\Kidzgo.Worktrees\dev
dotnet publish .\Kidzgo.API\Kidzgo.API.csproj -c Release -o C:\apps\kidzgo-api-dev
```

Sau buoc nay, trong 2 thu muc publish se co `Kidzgo.API.exe` va ban moi nen tao service.

## 7. Tao 2 Windows Service bang NSSM

Phan nay gia dinh VPS cua ban dang dung NSSM de chay app .NET nhu mot service.

Vi du NSSM nam o:

- `C:\tools\nssm\nssm.exe`

Neu khac duong dan, doi lai cho dung.

### 7.1. Tao service main

```powershell
$nssm = "C:\tools\nssm\nssm.exe"

& $nssm install KidzgoAPI-Main "C:\apps\kidzgo-api-main\Kidzgo.API.exe"
& $nssm set KidzgoAPI-Main AppDirectory "C:\apps\kidzgo-api-main"
& $nssm set KidzgoAPI-Main DisplayName "Kidzgo API Main"
& $nssm set KidzgoAPI-Main Description "Kidzgo main production API"
& $nssm set KidzgoAPI-Main Start SERVICE_AUTO_START
& $nssm set KidzgoAPI-Main AppStdout "C:\logs\kidzgo-main-stdout.log"
& $nssm set KidzgoAPI-Main AppStderr "C:\logs\kidzgo-main-stderr.log"
& $nssm set KidzgoAPI-Main AppRotateFiles 1
& $nssm set KidzgoAPI-Main AppRotateOnline 1
```

### 7.2. Tao service dev

```powershell
$nssm = "C:\tools\nssm\nssm.exe"

& $nssm install KidzgoAPI-Dev "C:\apps\kidzgo-api-dev\Kidzgo.API.exe"
& $nssm set KidzgoAPI-Dev AppDirectory "C:\apps\kidzgo-api-dev"
& $nssm set KidzgoAPI-Dev DisplayName "Kidzgo API Dev"
& $nssm set KidzgoAPI-Dev Description "Kidzgo dev staging API"
& $nssm set KidzgoAPI-Dev Start SERVICE_AUTO_START
& $nssm set KidzgoAPI-Dev AppStdout "C:\logs\kidzgo-dev-stdout.log"
& $nssm set KidzgoAPI-Dev AppStderr "C:\logs\kidzgo-dev-stderr.log"
& $nssm set KidzgoAPI-Dev AppRotateFiles 1
& $nssm set KidzgoAPI-Dev AppRotateOnline 1
```

### 7.3. Kiem tra service

```powershell
Get-Service KidzgoAPI-Main
Get-Service KidzgoAPI-Dev
```

Neu can xoa service cu:

```powershell
$nssm = "C:\tools\nssm\nssm.exe"
& $nssm remove KidzgoAPI-Main confirm
& $nssm remove KidzgoAPI-Dev confirm
```

## 8. Deploy tung ban

### 8.1. Deploy main

```powershell
cd C:\Users\Administrator\Desktop\Projects\Kidzgo
.\deploy-main-win.ps1
```

Mac dinh wrapper nay dung:

- ProjectPath: `C:\Users\Administrator\Desktop\Projects\Kidzgo.Worktrees\main`
- PublishPath: `C:\apps\kidzgo-api-main`
- ServiceName: `KidzgoAPI-Main`
- ApiBindUrl: `http://127.0.0.1:5000`
- PublicBaseUrl: `https://api.kidzgo.vn`

### 8.2. Deploy dev

```powershell
cd C:\Users\Administrator\Desktop\Projects\Kidzgo
.\deploy-dev-win.ps1
```

Mac dinh wrapper nay dung:

- ProjectPath: `C:\Users\Administrator\Desktop\Projects\Kidzgo.Worktrees\dev`
- PublishPath: `C:\apps\kidzgo-api-dev`
- ServiceName: `KidzgoAPI-Dev`
- ApiBindUrl: `http://127.0.0.1:5001`
- PublicBaseUrl: `https://dev-api.kidzgo.vn`

Neu domain/path cua VPS khac mac dinh, sua trong:

- `deploy-main-win.ps1`
- `deploy-dev-win.ps1`

## 9. Cau hinh Caddy cho 2 domain

Chay script:

```powershell
cd C:\Users\Administrator\Desktop\Projects\Kidzgo
.\scripts\install-caddy-dual-win.ps1 -MainDomain "api.kidzgo.vn" -DevDomain "dev-api.kidzgo.vn"
```

Script se tao reverse proxy:

- `api.kidzgo.vn` -> `127.0.0.1:5000`
- `dev-api.kidzgo.vn` -> `127.0.0.1:5001`

Neu can doi domain:

```powershell
.\scripts\install-caddy-dual-win.ps1 `
  -MainDomain "api.tenmien.com" `
  -DevDomain "dev-api.tenmien.com" `
  -MainApiUpstream "127.0.0.1:5000" `
  -DevApiUpstream "127.0.0.1:5001"
```

## 10. Kiem tra sau khi deploy

### 10.1. Kiem tra service

```powershell
Get-Service KidzgoAPI-Main
Get-Service KidzgoAPI-Dev
```

### 10.2. Kiem tra port local

```powershell
netstat -ano | findstr :5000
netstat -ano | findstr :5001
```

Ban phai thay process dang listen tren 2 port nay.

### 10.3. Kiem tra HTTP local

```powershell
Invoke-WebRequest http://127.0.0.1:5000/swagger/index.html
Invoke-WebRequest http://127.0.0.1:5001/swagger/index.html
```

### 10.4. Kiem tra domain public

```powershell
Invoke-WebRequest https://api.kidzgo.vn/swagger/index.html
Invoke-WebRequest https://dev-api.kidzgo.vn/swagger/index.html
```

## 11. Quy trinh lam viec hang ngay

### Khi can deploy production

```powershell
cd C:\Users\Administrator\Desktop\Projects\Kidzgo
.\deploy-main-win.ps1
```

### Khi can deploy dev

```powershell
cd C:\Users\Administrator\Desktop\Projects\Kidzgo
.\deploy-dev-win.ps1
```

### Khi can vao source cua tung ban

Main:

```powershell
cd C:\Users\Administrator\Desktop\Projects\Kidzgo.Worktrees\main
git status
```

Dev:

```powershell
cd C:\Users\Administrator\Desktop\Projects\Kidzgo.Worktrees\dev
git status
```

## 12. Loi thuong gap

### `fatal: invalid reference: origin/dev`

Remote chua co branch `dev`.

Chay:

```powershell
cd C:\Users\Administrator\Desktop\Projects\Kidzgo
.\scripts\setup-git-worktrees-win.ps1 -CreateMissingRemoteBranches
```

### Service start len nhung app khong nghe port

Thuong do:

- sai `ConnectionStrings:Database`
- file `appsettings.Production.local.json` bi thieu secret
- service dang tro sai `AppDirectory`

Kiem tra:

```powershell
Get-Content C:\apps\kidzgo-api-main\appsettings.Production.local.json
Get-Content C:\apps\kidzgo-api-dev\appsettings.Production.local.json
```

### Caddy vao domain khong duoc

Kiem tra:

- DNS da tro ve VPS chua
- service `caddy` co running khong
- Caddyfile da map dung domain/upstream chua

```powershell
Get-Service caddy
Get-Content C:\Caddy\Caddyfile
```

## 13. Thu tu setup de nghi

Neu lam moi hoan toan tren VPS, chay theo thu tu nay:

1. clone repo goc vao `C:\Users\Administrator\Desktop\Projects\Kidzgo`
2. chay `.\scripts\setup-git-worktrees-win.ps1 -CreateMissingRemoteBranches`
3. tao 2 thu muc publish
4. tao 2 database `kidzgo_prod` va `kidzgo_dev`
5. `dotnet publish` tay lan dau cho `main` va `dev`
6. tao 2 service NSSM tro vao 2 thu muc publish
7. chay `.\deploy-main-win.ps1`
8. chay `.\deploy-dev-win.ps1`
9. chay `.\scripts\install-caddy-dual-win.ps1`
10. verify service, port va domain

Neu muon an toan hon, ban co the tao service truoc, nhung khong start service cho den khi deploy lan dau xong.
