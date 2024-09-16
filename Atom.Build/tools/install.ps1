param($installPath, $toolsPath, $package, $project)

if($project.Object.SupportsPackageDependencyResolution)
{
    if($project.Object.SupportsPackageDependencyResolution())
    {
        # Do not install analyzers via install.ps1, instead let the project system handle it.
        return
    }
}
$buildSourcePath = Join-Path (Split-Path -Path $toolsPath -Parent) "build"

$projectDirectory = Split-Path -Path $project.FullName -Parent
$slnDirectory = Split-Path -Path $projectDirectory -Parent
$destinationPath = Join-Path $slnDirectory "_AtomBuild"

if (-not (Test-Path $destinationPath)) {
    New-Item -ItemType Directory -Path $destinationPath
}

# Copy only files that are not present at the destination
Get-ChildItem -Path $buildSourcePath -Recurse | ForEach-Object {
    $destinationFile = $_.FullName -replace [regex]::Escape($buildSourcePath), $destinationPath
    if (-not (Test-Path $destinationFile)) {
        Copy-Item -Path $_.FullName -Destination $destinationFile
    }
}

# Copy all files
#Copy-Item -Path $buildSourcePath\* -Destination $destinationPath -Recurse -Force
