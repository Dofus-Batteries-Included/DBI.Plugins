param (
    [string]$Configuration = "Release",
    [string]$Output = "dist",
    [switch]$Help
)

if ($Help)
{
    echo "Usage: pack.ps1 [-configuration CONFIGURATION] [-output OUTPUT FOLDER]"
    exit 0
}

echo "> Configuration: $Configuration"
echo "> Output path: $Output"

echo ""
$RunningDofusProcesses = Get-Process -Name "Dofus" -ErrorAction Ignore
if ($RunningDofusProcesses -ne $null)
{
    echo "Stopping Dofus processes..."
    Stop-Process -InputObject $RunningDofusProcesses
    Start-Sleep -Seconds 1
}

$RunningHeavenProcesses = Get-Process -Name "Heaven" -ErrorAction Ignore
if ($RunningHeavenProcesses -ne $null)
{
    echo "Stopping Heaven processes..."
    Stop-Process -InputObject $RunningHeavenProcesses
    Start-Sleep -Seconds 1
}

echo ""
if (Test-Path -Path $Output)
{
    echo "Cleaning output folder..."
    rm -Recurse -Force "$Output"
}
echo "Creating output folder $Output..."
$null = md $Output -Force

echo ""
echo "- Packing Hell..."

echo "Copying DBI.Hell.merged.dll to DBI.Hell.dll..."
copy "src/Hell/bin/$Configuration/net6.0/publish/DBI.Hell.merged.dll" "$Output/DBI.Hell.dll" -Force

echo "Done packing Hell."

echo ""
echo "- Packing Heaven..."

$SourceHeavenDir = "src/Heaven/bin/$Configuration/net8.0/publish"
echo "Copying $SourceHeavenDir to $Output..."
copy "$SourceHeavenDir/*" "$Output" -Recurse -Force

echo "Rename executable DBI.Heaven.exe to Heaven.exe..."
mv "$Output/DBI.Heaven.exe" "$Output/Heaven.exe"

echo "Done packing Heaven."

echo ""
echo "- Packing Heaven Launcher..."

$SourceHeavenDir = "src/Heaven.Launcher/bin/$Configuration/net8.0/publish"
echo "Copying $SourceHeavenDir to $Output..."
copy "$SourceHeavenDir/*" "$Output" -Recurse -Force

echo "Rename executable DBI.Heaven.Launcher.exe to Heaven Launcher.exe..."
mv "$Output/DBI.Heaven.Launcher.exe" "$Output/Heaven Launcher.exe"

echo "Done packing Heaven."

if ($Configuration -ne "Debug")
{
    echo ""
    echo "Deleting .pdb files because Configuration is $Configuration (not Debug)..."
    rm "$Output/*.pdb" -Force
}