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
echo "- Packing Hell..."

echo ""
$TargetHellDir = Join-Path $Output "Hell"

if (Test-Path -Path $TargetHellDir)
{
    echo "Cleaning output folder $TargetHellDir..."
    rm $TargetHellDir -r -force
}

echo "Creating output folder $TargetHellDir..."
$null = md $TargetHellDir

$InteropFolder = "src/Interop";
$InteropDlls = Get-ChildItem "$InteropFolder/*.dll" | % { Split-Path $_ -leaf }

echo "Found $( $InteropDlls.Length ) interop DLLs."

$OtherProjects = $Projects | Where-Object { $_ -ne $Project }
$OtherProjectsDll = $OtherProjects | % { "DofusBatteriesIncluded.Plugins.$_.dll" }

foreach ($File in Get-ChildItem "src/Hell/bin/$Configuration/net6.0/publish/*.dll")
{
    $Filename = Split-Path $File -leaf
    if ( $InteropDlls.Contains($Filename))
    {
        continue;
    }

    if (-not $OtherProjectsDll -eq $Null -and $OtherProjectsDll.Contains($Filename))
    {
        continue;
    }

    echo "Copying $File to $TargetHellDir..."
    copy "$File" "$TargetHellDir"
}

echo "Done packing Hell."

echo ""
echo "- Packing Heaven..."

$TargetHeavenDir = Join-Path $Output "Heaven"

if (Test-Path -Path $TargetHeavenDir) 
{
    echo "Cleaning output folder $TargetHeavenDir..."
    rm "$TargetHeavenDir/*.dll"
    rm "$TargetHeavenDir/*.exe"
}

$SourceHeavenDir = "src/Heaven/bin/$Configuration/net8.0/publish"
echo "Copying $SourceHeavenDir to $TargetHeavenDir..."
copy "$SourceHeavenDir/*" "$TargetHeavenDir" -Recurse -Exclude "*.pdb" -Force

echo "Rename executable DBI.Heaven.exe to Heaven.exe..."
mv "$TargetHeavenDir/DBI.Heaven.exe" "$TargetHeavenDir/Heaven.exe"

echo "Done packing Heaven."