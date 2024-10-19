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
if (Test-Path -Path $Output)
{
    echo "- Cleaning output folder $Output..."
    rm $Output -r -force
}

echo "- Creating output folder $Output..."
$null = md $Output

echo ""
echo "- Packing Hell..."

$InteropFolder = "src/Interop";
$InteropDlls = Get-ChildItem "$InteropFolder/*.dll" | % { Split-Path $_ -leaf }

echo "Found $( $InteropDlls.Length ) interop DLLs."

$OtherProjects = $Projects | Where-Object { $_ -ne $Project }
$OtherProjectsDll = $OtherProjects | % { "DofusBatteriesIncluded.Plugins.$_.dll" }

$TargetHellDir = Join-Path $Output "Hell"
$null = MkDir $TargetHellDir -Force
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

$SourceHeavenDir = "src/Heaven/bin/$Configuration/net8.0/publish"
$TargetHeavenDir = Join-Path $Output "Heaven"

echo "Copying $SourceHeavenDir to $TargetHeavenDir..."
copy "$SourceHeavenDir" "$TargetHeavenDir" -Recurse -Exclude "*.pdb"

echo "Ringname executable DBI.Heaven.exe to Heaven.exe..."
mv "$TargetHeavenDir/DBI.Heaven.exe" "$TargetHeavenDir/Heaven.exe"

echo "Done packing Heaven."