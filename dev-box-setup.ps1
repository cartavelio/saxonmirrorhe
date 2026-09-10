# Try to setup new Windows 11 box for Saxon dev.
#
# This needs to be run in a PowerShell that was run as Administrator. You
# also need to copy across the saxon-build.vsconfig file to the same folder
# as this script.
#
# You will need to set the Execution Policy for the shell to Bypass, or
# you won't be allowed to run this script:
# `Set-ExecutionPolicy Bypass`
#
# Once you have all that done, you can run the script from PowerShell with:
# .\dev-box-setup.ps1

$bad_winget = Get-Process | Where-Object {$_.ProcessName -eq 'WindowsPackageManagerServer' -and $_.Path -match "Microsoft\.DesktopAppInstaller_1\.17\.10691"}
If ($bad_winget -ne $null) {
    Write-Host -ForegroundColor Yellow "Killing running WPM instance, because it's probably the hung-by-its-own-update one"
    $bad_winget | Stop-Process
}

Write-Host -ForegroundColor Yellow "Updating winget"
$tmp = New-TemporaryFile | Rename-Item -NewName {$_ -replace 'tmp$', 'msixbundle'} -PassThru
Invoke-WebRequest -OutFile $tmp "https://github.com/microsoft/winget-cli/releases/latest/download/Microsoft.DesktopAppInstaller_8wekyb3d8bbwe.msixbundle"
$tmp | Add-AppxPackage
$tmp | Remove-Item

Write-Host -ForegroundColor Green "Install Visual Studio Community 2022"
If (Test-Path -Path "$PSScriptRoot\saxon-build.vsconfig") {
    Write-Host -ForegroundColor Yellow "Using saxon-build.vsconfig to configure unattended Visual Studio install"
    winget install Microsoft.VisualStudio.2022.Community --accept-source-agreements --accept-package-agreements --override "--passive --config $PSScriptRoot\saxon-build.vsconfig"
} Else {
    Write-Host -ForegroundColor Red "saxon-build.vsconfig is not present in the same directory as this script. We can't install Visual Studio without input, so we will just install the Installer"
    winget install Microsoft.VisualStudio.2022.Community --accept-source-agreements --accept-package-agreements --override "--installerOnly"
}

Write-Host -ForegroundColor Green "Install VS-Code with Path and Shell integration"
winget install Microsoft.VisualStudioCode --accept-source-agreements --accept-package-agreements --override "/sp- /verysilent /suppressmsgboxes /norestart /mergetasks=!runcode,!desktopicon,addcontextmenufiles,addcontextmenufolders,associatewithfiles,addtopath"

Write-Host -ForegroundColor Green "Install Git"
winget install Git.Git --accept-source-agreements --accept-package-agreements

Write-Host -ForegroundColor Green "Install MS OpenJDK 11"
winget install Microsoft.OpenJDK.11 --accept-source-agreements --accept-package-agreements

# https://github.com/graalvm/graalvm-ce-builds/releases/download/vm-22.3.1/graalvm-ce-java11-windows-amd64-22.3.1.zip
Write-Host -ForegroundColor Green "Install Graal VM (x86_64)"
$tmp = New-TemporaryFile | Rename-Item -NewName {$_ -replace 'tmp$', 'zip'} -PassThru
$JavaPath = "C:\Program Files\Java"
$GraalPath = "$JavaPath\graalvm-ce-java11-22.3.1"
Invoke-WebRequest -OutFile $tmp "https://github.com/graalvm/graalvm-ce-builds/releases/download/vm-22.3.1/graalvm-ce-java11-windows-amd64-22.3.1.zip"
If (-Not (Test-Path -Path "$JavaPath")) {
    New-Item -ItemType Directory -Path "$JavaPath"
}
Expand-Archive "$tmp" "$JavaPath"
$tmp | Remove-Item

Write-Host -ForegroundColor Green "Install Python (x86_64)"
winget install -e --id Python.Python.3.12 --architecture X64 --override "/simple /passive InstallAllUsers=1"

# Setting PATH without breaking the REG_EXPAND_SZ dynamic expansion stuff within it.
Write-Host -ForegroundColor Green "Setting PATH variable to include GraalVM\bin"
$EnvRegistryPath = "SYSTEM\CurrentControlSet\Control\Session Manager\Environment"
$RawPath = (New-Object -ComObject WScript.Shell).RegRead("HKLM\$EnvRegistryPath\Path")
Set-ItemProperty -Path "HKLM:\$EnvRegistryPath" -Name Path -Value "$GraalPath\bin;$RawPath" -Type ExpandString

# Set JAVA_HOME too
Write-Host -ForegroundColor Green "Setting JAVA_HOME variable to GraalVM path: $GraalPath"
Set-ItemProperty -Path "HKLM:\$EnvRegistryPath" -Name JAVA_HOME -Value "$GraalPath" -Type String

# Set JAVA_TOOL_OPTIONS for avoidance of UTF-8 errors
Write-Host -ForegroundColor Green "Setting JAVA_TOOL_OPTIONS variable"
Set-ItemProperty -Path "HKLM:\$EnvRegistryPath" -Name JAVA_TOOL_OPTIONS -Value "-Dfile.encoding=UTF8" -Type String
