param (
    [string]$Configuration = "Release",
    [string]$Output = "dist/",
    [switch]$Help
)

if ($Help)
{
    echo "Usage: pack.ps1 [-configuration CONFIGURATION] [-output OUTPUT FOLDER]"
    exit 0
}

echo "> Configuration: $Configuration"
echo "> Output path: $Output"

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
if (Test-Path -Path $Output)
{
    echo "Cleaning Hell output folder..."
    rm -Recurse -Force "$Output"
}
echo "Creating output folder $Output..."
$null = md $Output -Force

echo ""
echo "- Packing Hell..."

mkdir $Output -Force

$SourceHellDir = "src/Hell/bin/$Configuration/net6.0/publish"

echo "Copying $SourceHellDir to $Output..."

foreach ($File in Get-ChildItem "$SourceHellDir/*.dll")
{
    $Filename = Split-Path $File -leaf
    if ( -not $Exceptions.Contains($Filename) -and $InteropDlls.Contains($Filename))
    {
        continue;
    }

    echo "Copying $File to $Output..."
    copy $File $Output
}

$SourceHellLauncherDir = "src/Hell.RedirectMessages/bin/$Configuration/net6.0/publish"

echo "Copying $SourceHellLauncherDir/DBI.Hell.RedirectMessages.dll to $Output/DBI.Hell.RedirectMessages.dll..."
copy "$SourceHellLauncherDir/DBI.Hell.RedirectMessages.dll" "$Output/DBI.Hell.RedirectMessages.dll" -Force

echo "Done packing Hell."

echo ""
echo "- Packing Heaven..."

mkdir $Output -Force

$SourceHeavenDir = "src/Heaven.Application/bin/$Configuration/net8.0/win-x64/publish"

echo "Copying $SourceHeavenDir to $Output..."
copy "$SourceHeavenDir/*" "$Output" -Recurse -Force

echo "Rename executable DBI.Heaven.Application.exe to Heaven.exe..."
mv "$Output/DBI.Heaven.Application.exe" "$Output/Heaven.exe" -Force

echo "Done packing Heaven."

echo ""
echo "- Packing Heaven Launcher..."

$SourceHeavenDir = "src/Heaven.Launcher/bin/$Configuration/net8.0/win-x64/publish"
echo "Copying $SourceHeavenDir to $Output..."
copy "$SourceHeavenDir/*" "$Output" -Recurse -Force

echo "Rename executable DBI.Heaven.Launcher.exe to Heaven Launcher.exe..."
mv "$Output/DBI.Heaven.Launcher.exe" "$Output/Heaven Launcher.exe" -Force

echo "Done packing Heaven."

if ($Configuration -ne "Debug")
{
    echo ""
    echo "Deleting .pdb files because Configuration is $Configuration (not Debug)..."
    rm "$Output/*.pdb" -Force
    rm "$Output/*.pdb" -Force
}