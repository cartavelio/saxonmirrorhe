<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:xs="http://www.w3.org/2001/XMLSchema"
                exclude-result-prefixes="xs"
                expand-text="yes"
                version="3.0">

<xsl:output method="xml" encoding="utf-8" indent="yes"/>

<!-- Assumed to be an absolute file reference, or relative to the static-base-uri() -->
<xsl:param name="saxonVersion" as="xs:string" required="yes"/>
<xsl:param name="csrid" as="xs:string" required="yes"/>
<xsl:param name="edition" as="xs:string" required="yes"/>
<xsl:param name="optimize" as="xs:string" select="'true'"/>

<xsl:template name="xsl:initial-template">
  <xsl:result-document href="SaxonCS.csproj">
    <Project Sdk="Microsoft.NET.Sdk">
      <Import Project="core.csproj"/>
      <ItemGroup>
        <None Include="../../../../LICENSE.txt" Pack="true" PackagePath=""/>
        <None Include="../../../../README.md" Pack="true" PackagePath=""/>
      </ItemGroup>
      <ItemGroup>
        <Compile Include="..\..\generated\**"/>
        <Compile Include="..\..\..\..\src\main\cs\**"/>
        <Compile Include="..\..\..\..\keys\build\cs\**"/>
      </ItemGroup>
    </Project>
  </xsl:result-document>

  <Project>
    <PropertyGroup>
      <LangVersion>9.0</LangVersion>
      <TargetFramework>net6.0</TargetFramework>
      <Nullable>disable</Nullable>
      <OutputType>Exe</OutputType>
      <NoWarn>2200;0414;0168;0169;0649;1591</NoWarn>
      <GenerateDocumentationFile>true</GenerateDocumentationFile>
      <GeneratePackageOnBuild>True</GeneratePackageOnBuild>
      <EnableDefaultCompileItems>false</EnableDefaultCompileItems>

      <PlatformTarget>AnyCPU</PlatformTarget>
      <DebugType>pdbonly</DebugType>
      <Optimize>{$optimize}</Optimize>
      <OutputType>Exe</OutputType>
      <PublishSingleFile>true</PublishSingleFile>
      <SelfContained>true</SelfContained>
      <PublishReadyToRun>true</PublishReadyToRun>
      <RuntimeIdentifier>{$csrid}</RuntimeIdentifier>
      <UseHardenedRuntime>true</UseHardenedRuntime>
      <IncludeNativeLibrariesForSelfExtract>true</IncludeNativeLibrariesForSelfExtract>
      <IncludeSymbolsInSingleFile>false</IncludeSymbolsInSingleFile>
      <AssemblyName>SaxonCS</AssemblyName>

      <Authors>Saxonica Ltd.</Authors>
      <Description>SaxonCS is a high-performance XSLT 3.0 processor. It conforms with the
latest W3C specifications (notably XSLT 3.0, XQuery 3.1, and XPath 3.1).</Description>
      <PackageTags>XML,XSLT,Saxonica</PackageTags>
      <Copyright>Copyright © 2021-{year-from-date(current-date())} Saxonica, Ltd.</Copyright>
      <PackageProjectUrl>https://saxonica.com/</PackageProjectUrl>
      <PackageReleaseNotes>Our first native C# release.</PackageReleaseNotes>
      <PackageSummary>An XSLT 3.0 processor for .NET 6</PackageSummary>
      <PackageLicenseFile>LICENSE.txt</PackageLicenseFile>
      <PackageReadmeFile>README.md</PackageReadmeFile>
    </PropertyGroup>    

    <PropertyGroup>
      <CFBundleName>SaxonCS</CFBundleName>
      <CFBundleDisplayName>SaxonCS</CFBundleDisplayName>
      <CFBundleIdentifier>com.saxonica.saxoncs</CFBundleIdentifier>
      <CFBundleVersion>{$saxonVersion}</CFBundleVersion>
      <CFBundleShortVersionString>{$saxonVersion}</CFBundleShortVersionString>
      <CFBundleExecutable>SaxonCS</CFBundleExecutable>
      <CFBundleIconFile>SaxonicaDisk.icns</CFBundleIconFile>
      <NSPrincipalClass>NSApplication</NSPrincipalClass>
      <NSHighResolutionCapable>true</NSHighResolutionCapable>
      <NSRequiresAquaSystemAppearance>false</NSRequiresAquaSystemAppearance>
    </PropertyGroup>

    <ItemGroup>
      <PackageReference Include="DotNet.Bundle" Version="0.9.13" />
    </ItemGroup>

    <ItemGroup>
      <EmbeddedResource Include="..\..\resources\**\*">
        <LogicalName>$(AssemblyName).$([System.IO.Path]::GetFullPath('%(Identity)'))</LogicalName>
      </EmbeddedResource>
    </ItemGroup>

    <ItemGroup>
      <PackageReference Include="AngleSharp" Version="0.17.1" />
      <PackageReference Include="ICU4N" Version="60.1.0-alpha.402" />
      <!-- Is this implied by ICU4N? I'm getting duplicate package reference errors for it
      <PackageReference Include="ICU4N.Resources" Version="60.1.0-alpha.402" />
      -->
      <PackageReference Include="NSec.Cryptography" Version="22.4.0" />
      <PackageReference Include="Microsoft.AspNetCore.StaticFiles" Version="2.2.0" />
      <PackageReference Include="Singulink.Numerics.BigDecimal" Version="2.0.2" />
      <PackageReference Include="Singulink.Numerics.BigIntegerExtensions" Version="2.1.0" />
      <PackageReference Include="System.Collections.Immutable" Version="6.0.0" />
      <PackageReference Include="StringComparer" Version="1.0.1"/>
      <PackageReference Include="System.IO.Hashing" Version="8.0.0" />
      <PackageReference Include="System.Net.Http" Version="4.3.4" />
      <PackageReference Include="Microsoft.Extensions.Configuration" Version="8.0.0" />
      <PackageReference Include="Microsoft.Extensions.Configuration.Json" Version="8.0.0" />
      <PackageReference Include="Microsoft.Extensions.Configuration.Xml" Version="8.0.0" />
      <PackageReference Include="System.Text.Encoding.CodePages" Version="6.0.0" />
      <PackageReference Include="XmlResolver" Version="6.0.2" />
      <PackageReference Include="XmlResolverData" Version="6.0.2" />
    </ItemGroup>

   <xsl:if test="$edition != 'ee'">
     <ItemGroup>
       <Compile Remove="..\..\..\..\cs\Saxon\Impl\Helpers\UcaCollatorUsingCSharp.cs" />
       <Compile Remove="..\..\..\..\cs\Saxon\Ejava\text\CollationKeyUsingCSharp.cs" />
     </ItemGroup>
   </xsl:if>
  </Project>
</xsl:template>

</xsl:stylesheet>
