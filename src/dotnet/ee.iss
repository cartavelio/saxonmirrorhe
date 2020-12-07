; SEE THE DOCUMENTATION FOR DETAILS ON CREATING .ISS SCRIPT FILES!
; Documentation is at http://www.jrsoftware.org/ishelp/

[Setup]
AppName=Saxon-EE @@VERSION@@
AppVerName=Saxon-EE @@VERSION@@
AppPublisher=Saxonica
AppVersion=@@VERSION@@
DefaultDirName=C:\Program Files\Saxonica\SaxonEE@@MAJOR_VERSION@@.@@MINOR_VERSION@@N
DefaultGroupName=Saxon-EE @@VERSION@@
PrivilegesRequired=none
Compression=lzma
SolidCompression=yes
SourceDir=@@SOURCE_DIR@@
OutputDir=@@OUTPUT_DIR@@
OutputBaseFilename=SaxonEE@@VERSION_H@@N-setup
AppCopyright=Copyright (c) @@COPYRIGHT_YEAR@@ Saxonica

[Files]
; GAC Files

Source: "IKVM.Runtime.dll"; DestDir: "{app}\bin"; Flags: gacinstall; StrongAssemblyName: "IKVM.Runtime, @@IKVM_STRONGNAME@@"
Source: "IKVM.OpenJDK.Core.dll"; DestDir: "{app}\bin"; Flags: gacinstall; StrongAssemblyName: "IKVM.OpenJDK.Core, @@IKVM_STRONGNAME@@"
Source: "IKVM.OpenJDK.Charsets.dll"; DestDir: "{app}\bin"; Flags: gacinstall; StrongAssemblyName: "IKVM.OpenJDK.Charsets, @@IKVM_STRONGNAME@@"
Source: "IKVM.OpenJDK.Security.dll"; DestDir: "{app}\bin"; Flags: gacinstall; StrongAssemblyName: "IKVM.OpenJDK.Security, @@IKVM_STRONGNAME@@"
Source: "IKVM.OpenJDK.Util.dll"; DestDir: "{app}\bin" ; Flags: gacinstall; StrongAssemblyName: "IKVM.OpenJDK.Util, @@IKVM_STRONGNAME@@"
Source: "IKVM.OpenJDK.Text.dll"; DestDir: "{app}\bin"; Flags: gacinstall; StrongAssemblyName: "IKVM.OpenJDK.Text, @@IKVM_STRONGNAME@@"
Source: "IKVM.OpenJDK.XML.API.dll"; DestDir: "{app}\bin"; Flags: gacinstall ; StrongAssemblyName: "IKVM.OpenJDK.XML.API, @@IKVM_STRONGNAME@@"
Source: "saxon10ee.dll"; DestDir: "{app}\bin"; Flags: gacinstall; StrongAssemblyName: "saxon10ee, ${saxon.strongname}"
Source: "saxon10ee-api.dll"; DestDir: "{app}\bin"; Flags: gacinstall; StrongAssemblyName: "saxon10ee-api, ${saxon.strongname}"
Source: "saxon10ee-api.xml"; DestDir: "{app}\bin"
Source: "..\..\notices\APACHE-RESOLVER.txt" ; DestDir: "{app}\notices"
Source: "..\..\notices\APACHE-XERCES.txt" ; DestDir: "{app}\notices"
Source: "..\..\notices\ASM.txt" ; DestDir: "{app}\notices"
Source: "..\..\notices\CERN.txt" ; DestDir: "{app}\notices"
Source: "..\..\notices\FRIJTERS.txt" ; DestDir: "{app}\notices"
Source: "..\..\notices\GPL+CLASSPATH.txt" ; DestDir: "{app}\notices"
Source: "..\..\notices\JAMESCLARK.txt" ; DestDir: "{app}\notices"
Source: "..\..\notices\LICENSE.txt" ; DestDir: "{app}\notices"
Source: "..\..\notices\THAI.txt" ; DestDir: "{app}\notices"
Source: "..\..\notices\UNICODE.txt" ; DestDir: "{app}\notices"

; Program Files
Source: "Query.exe"; DestDir: "{app}\bin"
Source: "Transform.exe"; DestDir: "{app}\bin"
Source: "Validate.exe"; DestDir: "{app}\bin"

[Registry]
Root: HKCU; Subkey: "Software\Saxonica"; Flags: uninsdeletekeyifempty
Root: HKCU; Subkey: "Software\Saxonica\SaxonEE-N"; Flags: uninsdeletekey
Root: HKCU; Subkey: "Software\Saxonica\SaxonEE-N\Settings"; ValueType: string; ValueName: "InstallPath"; ValueData: "{app}";
Root: HKLM; Subkey: "Software\Saxonica"; Flags: uninsdeletekeyifempty
Root: HKLM; Subkey: "Software\Saxonica\SaxonEE-N"; Flags: uninsdeletekey
Root: HKLM; Subkey: "Software\Saxonica\SaxonEE-N\Settings"; ValueType: string; ValueName: "InstallPath"; ValueData: "{app}";
Root: HKCU; Subkey: "Environment"; ValueType: expandsz; ValueName: "Path"; ValueData: "{olddata};{app}\bin"

[UninstallDelete]
Type: dirifempty; Name: "{app}";          
