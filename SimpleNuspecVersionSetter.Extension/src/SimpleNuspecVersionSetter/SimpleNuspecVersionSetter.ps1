[CmdletBinding()]
param(
    [string]$RootDirectory,
    [string]$NewVersion
)

begin
{
    $PackagesConfigMergerExe = ".\exe\SimpleNuspecVersionSetter.exe"
}

process
{
    Write-Verbose "Executing command: $RootDirectory $NewVersion"
    & $PackagesConfigMergerExe $RootDirectory $NewVersion
}

end
{

}