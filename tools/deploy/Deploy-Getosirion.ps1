<#
.SYNOPSIS
    Deploys the newest getosirion.com build from GitHub Actions to the IIS site on this server.

.DESCRIPTION
    The CI job "Build the site" (.github/workflows/ci.yml) publishes the site after every check passes on a push to
    master and keeps it as the artifact getosirion-site. This script runs on the server on a schedule. It asks GitHub for the
    newest artifact, and when that is not the one already deployed it:

    1. downloads and unpacks it into a staging folder;
    2. copies the live site to a backup folder;
    3. takes the app offline with app_offline.htm, copies the new build over the site and brings it back;
    4. checks that the site answers, and copies the backup back if it does not.

    Server-specific settings (the GitHub token, the webhook secret, the repository owner) stay in the IIS environment
    variables of the site, because each deploy replaces appsettings.json with the repository's version.

    Works in Windows PowerShell 5.1. Writes its log to <StatePath>\deploy.log.

.PARAMETER TokenPath
    A file holding a fine-grained GitHub token with read access to Actions on the repository. Restrict the file to
    SYSTEM and Administrators.
#>
[CmdletBinding()]
param(
    [string]$Repository = 'demonjic/Osirion.Blazor',
    [string]$ArtifactName = 'getosirion-site',
    [string]$SitePath = 'C:\inetpub\wwwroot\getosirion.com',
    [string]$SiteUrl = 'https://getosirion.com/',
    [string]$StatePath = 'C:\deploy\getosirion',
    [string]$TokenPath = 'C:\deploy\getosirion\github-token.txt',
    [switch]$Force
)

$ErrorActionPreference = 'Stop'
[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
Add-Type -AssemblyName System.Net.Http

$logPath = Join-Path $StatePath 'deploy.log'
$lastPath = Join-Path $StatePath 'last-artifact.txt'
$lockPath = Join-Path $StatePath 'deploy.lock'

function Write-Log([string]$Message) {
    "$((Get-Date).ToString('yyyy-MM-dd HH:mm:ss')) $Message" | Add-Content -Path $logPath -Encoding UTF8
}

function Copy-Folder([string]$From, [string]$To) {
    # /E copies subfolders without deleting files the source lacks, so files only the server has (logs) survive.
    robocopy $From $To /E /R:3 /W:5 /NFL /NDL /NJH /NJS /NP | Out-Null
    if ($LASTEXITCODE -ge 8) { throw "robocopy from $From to $To failed with exit code $LASTEXITCODE" }
}

function Test-Site([string]$Url) {
    for ($attempt = 1; $attempt -le 6; $attempt++) {
        try {
            $response = Invoke-WebRequest -Uri $Url -UseBasicParsing -TimeoutSec 30
            if ($response.StatusCode -eq 200) { return $true }
        }
        catch {
            Start-Sleep -Seconds 10
        }
    }
    return $false
}

function Set-Offline([bool]$Offline) {
    $marker = Join-Path $SitePath 'app_offline.htm'
    if ($Offline) {
        Set-Content -Path $marker -Encoding UTF8 -Value '<!doctype html><title>Updating</title><p>getosirion.com is updating and will be back in a minute.</p>'
        Start-Sleep -Seconds 5
    }
    elseif (Test-Path $marker) {
        Remove-Item $marker
    }
}

New-Item -ItemType Directory -Force -Path $StatePath | Out-Null

# One run at a time: the scheduled task can start again while a slow deploy is still copying.
if ((Test-Path $lockPath) -and ((Get-Item $lockPath).LastWriteTime -gt (Get-Date).AddMinutes(-30))) { return }
Set-Content -Path $lockPath -Value $PID

try {
    $token = (Get-Content -Path $TokenPath -Raw).Trim()
    $headers = @{
        Authorization          = "Bearer $token"
        Accept                 = 'application/vnd.github+json'
        'X-GitHub-Api-Version' = '2022-11-28'
        'User-Agent'           = 'getosirion-deploy'
    }

    $list = Invoke-RestMethod -Uri "https://api.github.com/repos/$Repository/actions/artifacts?name=$ArtifactName&per_page=5" -Headers $headers
    $artifact = $list.artifacts |
        Where-Object { -not $_.expired -and $_.workflow_run.head_branch -eq 'master' } |
        Sort-Object { [DateTime]$_.created_at } -Descending |
        Select-Object -First 1
    if (-not $artifact) { return }

    $last = if (Test-Path $lastPath) { (Get-Content -Path $lastPath -Raw).Trim() } else { '' }
    if (-not $Force -and "$($artifact.id)" -eq $last) { return }

    Write-Log "Deploying artifact $($artifact.id), commit $($artifact.workflow_run.head_sha)"

    # The download answers with a redirect to a short-lived storage address, which must be fetched without the token.
    $handler = New-Object System.Net.Http.HttpClientHandler
    $handler.AllowAutoRedirect = $false
    $client = New-Object System.Net.Http.HttpClient($handler)
    $request = New-Object System.Net.Http.HttpRequestMessage([System.Net.Http.HttpMethod]::Get, $artifact.archive_download_url)
    $request.Headers.Authorization = New-Object System.Net.Http.Headers.AuthenticationHeaderValue('Bearer', $token)
    $request.Headers.UserAgent.ParseAdd('getosirion-deploy')
    $response = $client.SendAsync($request).GetAwaiter().GetResult()
    if (-not $response.Headers.Location) { throw "GitHub answered $([int]$response.StatusCode) instead of a download address" }

    $zip = Join-Path $StatePath 'site.zip'
    Invoke-WebRequest -Uri $response.Headers.Location.AbsoluteUri -OutFile $zip -UseBasicParsing

    $staging = Join-Path $StatePath 'staging'
    if (Test-Path $staging) { Remove-Item $staging -Recurse -Force }
    Expand-Archive -Path $zip -DestinationPath $staging
    if (-not (Test-Path (Join-Path $staging 'Osirion.Blazor.Example.Bootstrap.dll'))) {
        throw 'The artifact holds no site build'
    }

    $backup = Join-Path $StatePath 'previous'
    if (Test-Path $backup) { Remove-Item $backup -Recurse -Force }
    Copy-Folder $SitePath $backup

    Set-Offline $true
    try {
        Copy-Folder $staging $SitePath
    }
    finally {
        Set-Offline $false
    }

    if (Test-Site $SiteUrl) {
        Set-Content -Path $lastPath -Value $artifact.id
        Write-Log "Deployed artifact $($artifact.id); $SiteUrl answers 200"
    }
    else {
        Write-Log "The site did not answer 200 after artifact $($artifact.id); restoring the previous build"
        Set-Offline $true
        try { Copy-Folder $backup $SitePath } finally { Set-Offline $false }
        # Recorded so a broken build is not deployed again every few minutes; a newer build deploys as usual.
        Set-Content -Path $lastPath -Value $artifact.id
        Write-Log 'Previous build restored'
    }
}
catch {
    Write-Log "Deploy failed: $($_.Exception.Message)"
    throw
}
finally {
    Remove-Item $lockPath -ErrorAction SilentlyContinue
}
