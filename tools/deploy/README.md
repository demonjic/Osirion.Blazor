# Deploying getosirion.com

getosirion.com is the Bootstrap example app, hosted in IIS on a Windows server. Deploys are pulled by the server, so
nothing in this public repository can reach the server.

1. CI passes on a push to `master`.
2. The workflow **Build the site** (`.github/workflows/deploy-site.yml`) publishes the app and keeps it for 14 days as
   the artifact `getosirion-site`.
3. On the server, a scheduled task runs `Deploy-Getosirion.ps1` every five minutes. When a newer artifact exists, it
   downloads it, backs up the live site, swaps the build in behind `app_offline.htm`, checks that the site answers
   200, and restores the backup if it does not. It logs to `C:\deploy\getosirion\deploy.log`.

Server-specific settings stay in the site's IIS environment variables, because every deploy replaces
`appsettings.json`: `Osirion__Cms__Web__GitHub__Blog__ApiToken`, `Osirion__Cms__Web__GitHub__Blog__WebhookSecret` and
`Osirion__Cms__Web__GitHub__Blog__Owner`.

## One-time setup on the server

Run in PowerShell as administrator.

1. Create a fine-grained GitHub token for the deploy (github.com/settings/personal-access-tokens/new): resource owner
   demonjic, only the repository `demonjic/Osirion.Blazor`, repository permission **Actions: Read-only**, one year.
2. Put the script and the token in place and restrict the token file to SYSTEM and Administrators:

   ```powershell
   New-Item -ItemType Directory -Force C:\deploy\getosirion | Out-Null
   Invoke-WebRequest https://raw.githubusercontent.com/demonjic/Osirion.Blazor/master/tools/deploy/Deploy-Getosirion.ps1 -OutFile C:\deploy\getosirion\Deploy-Getosirion.ps1 -UseBasicParsing
   Set-Content C:\deploy\getosirion\github-token.txt 'PASTE_THE_DEPLOY_TOKEN' -NoNewline
   icacls C:\deploy\getosirion\github-token.txt /inheritance:r /grant:r "SYSTEM:R" "Administrators:F"
   ```

3. Run it once by hand and read the log:

   ```powershell
   powershell.exe -NoProfile -ExecutionPolicy Bypass -File C:\deploy\getosirion\Deploy-Getosirion.ps1
   Get-Content C:\deploy\getosirion\deploy.log -Tail 5
   ```

4. Schedule it every five minutes as SYSTEM:

   ```powershell
   $action = New-ScheduledTaskAction -Execute 'powershell.exe' -Argument '-NoProfile -ExecutionPolicy Bypass -File C:\deploy\getosirion\Deploy-Getosirion.ps1'
   $trigger = New-ScheduledTaskTrigger -Once -At (Get-Date) -RepetitionInterval (New-TimeSpan -Minutes 5)
   $principal = New-ScheduledTaskPrincipal -UserId 'SYSTEM' -LogonType ServiceAccount -RunLevel Highest
   Register-ScheduledTask -TaskName 'Deploy getosirion.com' -Action $action -Trigger $trigger -Principal $principal
   ```

To deploy the newest build again, run the script with `-Force`. To roll back by hand, copy
`C:\deploy\getosirion\previous` over the site folder behind an `app_offline.htm`.

## NuGet

`.github/workflows/nuget-publish.yml` runs on every push to `master` and publishes only when the version in the
project files has no tag yet. To release: run **Prepare Release** with the new version, read and merge its pull
request, and the packages, the tag and the GitHub release follow on their own. Prepare Release opens its pull request
with the repository secret `PAT_TOKEN`, a token of an account with write access (Contents and Pull requests).
