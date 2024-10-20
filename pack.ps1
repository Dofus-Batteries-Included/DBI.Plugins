param (
    [string]$Configuration = "Release",
    [string]$HellOutput = "dist/Hell",
    [string]$HeavenOutput = "dist/Heaven",
    [switch]$Help
)

if ($Help)
{
    echo "Usage: pack.ps1 [-configuration CONFIGURATION] [-output OUTPUT FOLDER]"
    exit 0
}

echo "> Configuration: $Configuration"
echo "> Hell output path: $HellOutput"
echo "> Heaven output path: $HeavenOutput"

$InteropDlls = Get-ChildItem "src/interop/*.dll" | % { Split-Path $_ -leaf }
$Exceptions = @("Google.Protobuf.dll")

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
if (Test-Path -Path $HellOutput)
{
    echo "Cleaning Hell output folder..."
    rm -Recurse -Force "$HellOutput"
}
echo "Creating output folder $HellOutput..."
$null = md $HellOutput -Force

echo ""
echo "- Packing Hell..."

mkdir $HellOutput -Force

$SourceHellDir = "src/Hell/bin/$Configuration/net6.0/publish"

echo "Copying $SourceHellDir to $HellOutput..."

foreach ($File in Get-ChildItem "$SourceHellDir/*.dll")
{
    $Filename = Split-Path $File -leaf
    if ( -not $Exceptions.Contains($Filename) -and $InteropDlls.Contains($Filename))
    {
        continue;
    }

    echo "Copying $File to $HellOutput..."
    copy $File $HellOutput
}

$SourceHellLauncherDir = "src/Hell.RedirectMessages/bin/$Configuration/net6.0/publish"

echo "Copying $SourceHellLauncherDir/DBI.Hell.RedirectMessages.dll to $HellOutput/DBI.Hell.RedirectMessages.dll..."
copy "$SourceHellLauncherDir/DBI.Hell.RedirectMessages.dll" "$HellOutput/DBI.Hell.RedirectMessages.dll" -Force

echo "Done packing Hell."

echo ""
echo "- Packing Heaven..."

mkdir $HeavenOutput -Force

$SourceHeavenDir = "src/Heaven.Application/bin/$Configuration/net8.0/publish"

echo "Copying $SourceHeavenDir to $HeavenOutput..."
copy "$SourceHeavenDir/*" "$HeavenOutput" -Recurse -Force

echo "Rename executable DBI.Heaven.Application.exe to Heaven.exe..."
mv "$HeavenOutput/DBI.Heaven.Application.exe" "$HeavenOutput/Heaven.exe" -Force

echo "Done packing Heaven."

echo ""
echo "- Packing Heaven Launcher..."

$SourceHeavenDir = "src/Heaven.Launcher/bin/$Configuration/net8.0/publish"
echo "Copying $SourceHeavenDir to $HeavenOutput..."
copy "$SourceHeavenDir/*" "$HeavenOutput" -Recurse -Force

echo "Rename executable DBI.Heaven.Launcher.exe to Heaven Launcher.exe..."
mv "$HeavenOutput/DBI.Heaven.Launcher.exe" "$HeavenOutput/Heaven Launcher.exe" -Force

echo "Done packing Heaven."

if ($Configuration -ne "Debug")
{
    echo ""
    echo "Deleting .pdb files because Configuration is $Configuration (not Debug)..."
    rm "$HellOutput/*.pdb" -Force
    rm "$HeavenOutput/*.pdb" -Force
}