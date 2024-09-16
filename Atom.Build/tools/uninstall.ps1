param($installPath, $toolsPath, $package, $project)

if($project.Object.SupportsPackageDependencyResolution)
{
    if($project.Object.SupportsPackageDependencyResolution())
    {
        # Do not uninstall analyzers via uninstall.ps1, instead let the project system handle it.
        return
    }
}

# Determine the project directory and solution directory
$projectDirectory = Split-Path -Path $project.FullName -Parent
$slnDirectory = Split-Path -Path $projectDirectory -Parent
$destinationPath = Join-Path $slnDirectory "_AtomBuild"

# Check if the _AtomBuild folder exists and remove it
if (Test-Path $destinationPath) {
    Remove-Item -Path $destinationPath -Recurse -Force
    Write-Host "Removed _AtomBuild folder from the solution directory."
}
