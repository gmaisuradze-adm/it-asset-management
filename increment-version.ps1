#!/usr/bin/env pwsh
# Auto Version Increment Script for Hospital Asset Tracker
# This script automatically increments the build version on each build

param(
    [Parameter(Mandatory=$false)]
    [ValidateSet("major", "minor", "patch", "build")]
    [string]$IncrementType = "build"
)

$projectFile = "HospitalAssetTracker.csproj"

if (-not (Test-Path $projectFile)) {
    Write-Error "Project file not found: $projectFile"
    exit 1
}

Write-Host "🔄 Auto-incrementing version..." -ForegroundColor Cyan

# Read the project file
[xml]$proj = Get-Content $projectFile

# Find the Version element
$versionElement = $proj.Project.PropertyGroup.Version

if (-not $versionElement) {
    Write-Error "Version element not found in project file"
    exit 1
}

# Parse current version
$currentVersion = [System.Version]$versionElement
Write-Host "📋 Current version: $currentVersion" -ForegroundColor Yellow

# Increment version based on type
switch ($IncrementType) {
    "major" {
        $newVersion = [System.Version]::new($currentVersion.Major + 1, 0, 0, 0)
    }
    "minor" {
        $newVersion = [System.Version]::new($currentVersion.Major, $currentVersion.Minor + 1, 0, 0)
    }
    "patch" {
        $newVersion = [System.Version]::new($currentVersion.Major, $currentVersion.Minor, $currentVersion.Build + 1, 0)
    }
    "build" {
        $revision = if ($currentVersion.Revision -eq -1) { 0 } else { $currentVersion.Revision }
        $newVersion = [System.Version]::new($currentVersion.Major, $currentVersion.Minor, $currentVersion.Build, $revision + 1)
    }
}

# Format version string (remove .0 if revision is 0)
if ($newVersion.Revision -eq 0) {
    $versionString = "$($newVersion.Major).$($newVersion.Minor).$($newVersion.Build)"
} else {
    $versionString = $newVersion.ToString()
}

Write-Host "🆕 New version: $versionString" -ForegroundColor Green

# Update version in project file
$proj.Project.PropertyGroup.Version = $versionString
$proj.Project.PropertyGroup.AssemblyVersion = "$($newVersion.Major).$($newVersion.Minor).$($newVersion.Build).0"
$proj.Project.PropertyGroup.FileVersion = "$($newVersion.Major).$($newVersion.Minor).$($newVersion.Build).0"

# Save the project file
$proj.Save((Resolve-Path $projectFile))

Write-Host "✅ Version updated successfully!" -ForegroundColor Green
Write-Host "📁 Updated files: $projectFile" -ForegroundColor Gray

# Optional: Create a version info file
$versionInfo = @{
    Version = $versionString
    BuildDate = (Get-Date -Format "yyyy-MM-dd HH:mm:ss")
    BuildMachine = $env:COMPUTERNAME
    BuildUser = $env:USERNAME
    GitCommit = if (Get-Command git -ErrorAction SilentlyContinue) { 
        (git rev-parse HEAD 2>$null) 
    } else { 
        "Unknown" 
    }
}

$versionInfo | ConvertTo-Json | Out-File "version.json" -Encoding UTF8
Write-Host "📋 Version info saved to version.json" -ForegroundColor Gray
