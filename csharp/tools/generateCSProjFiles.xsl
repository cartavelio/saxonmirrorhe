<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:xs="http://www.w3.org/2001/XMLSchema"
                exclude-result-prefixes="xs"
                expand-text="yes"
                version="3.0">

<xsl:output method="text" encoding="utf-8"/>

<xsl:param name="releaseBuild" as="xs:string?" select="()"/>

<xsl:param name="platform" as="xs:string" required="yes"/>
<xsl:param name="arch" as="xs:string" required="yes"/>
<xsl:param name="saxonVersion" as="xs:string" required="yes"/>
<xsl:param name="edition" as="xs:string" required="yes"/>
<xsl:param name="framework" as="xs:string" required="yes"/>

<xsl:param name="includeTestDriver" as="xs:boolean" select="empty($releaseBuild)"/>
<xsl:param name="core.csproj" as="xs:string" select="'core.csproj'"/>

<!-- what. a. hack. -->
<xsl:param name="uuid1" as="xs:string" select="'fake-uuid-1'"/>
<xsl:param name="uuid2" as="xs:string" select="'fake-uuid-2'"/>
<xsl:param name="uuid3" as="xs:string" select="'fake-uuid-3'"/>
<xsl:param name="uuid4" as="xs:string" select="'fake-uuid-4'"/>

<xsl:template name="xsl:initial-template">
  <xsl:variable name="outputPath" select="current-output-uri()"/>
  <xsl:variable name="solutionName"
                select="if (ends-with($outputPath, '/'))
                        then 'SaxonCS.sln'
                        else tokenize($outputPath, '/')[last()]"/>
  <xsl:variable name="templateName"
                select="if (contains($solutionName, '.'))
                        then string-join(tokenize($solutionName, '\.')[position() lt last()], '.')
                        else $solutionName"/>
  <xsl:variable name="pname"
                select="($releaseBuild, 'SaxonCS')[1]"/>

  <xsl:text>Microsoft Visual Studio Solution File, Format Version 12.00
Project("{{{$uuid1}}}") = "{$pname}", "{$templateName}.csproj", "{{{$uuid2}}}"
EndProject
</xsl:text>
  <xsl:if test="$includeTestDriver">
    <xsl:text>Project("{{{$uuid3}}}") = "TestDriver", "{$templateName}TestDriver.csproj", "{{{$uuid4}}}"
EndProject
</xsl:text>
  </xsl:if>
  <xsl:text>Global
        GlobalSection(SolutionConfigurationPlatforms) = preSolution
                Debug|Any CPU = Debug|Any CPU
                Release|Any CPU = Release|Any CPU
        EndGlobalSection
        GlobalSection(ProjectConfigurationPlatforms) = postSolution
                {{{$uuid2}}}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
                {{{$uuid2}}}.Debug|Any CPU.Build.0 = Debug|Any CPU
                {{{$uuid2}}}.Release|Any CPU.ActiveCfg = Release|Any CPU
                {{{$uuid2}}}.Release|Any CPU.Build.0 = Release|Any CPU
</xsl:text>
  <xsl:if test="$includeTestDriver">
    <xsl:text>                {{{$uuid4}}}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
                {{{$uuid4}}}.Debug|Any CPU.Build.0 = Debug|Any CPU
                {{{$uuid4}}}.Release|Any CPU.ActiveCfg = Release|Any CPU
                {{{$uuid4}}}.Release|Any CPU.Build.0 = Release|Any CPU
</xsl:text>
  </xsl:if>
  <xsl:text>        EndGlobalSection
EndGlobal
  </xsl:text>

  <xsl:result-document href="{$core.csproj}" method="xml" indent="yes">
    <Project>
      <PropertyGroup>
        <LangVersion>9.0</LangVersion>
        <TargetFramework>{$framework}</TargetFramework>
        <Version>{$saxonVersion}</Version>
        <AssemblyVersion>{$saxonVersion}</AssemblyVersion>
        <Nullable>disable</Nullable>
        <OutputType>Exe</OutputType>
        <NoWarn>2200;0414;0168;0169;0649;1591</NoWarn>
        <GenerateDocumentationFile>true</GenerateDocumentationFile>
        <GeneratePackageOnBuild>True</GeneratePackageOnBuild>
        <EnableDefaultCompileItems>false</EnableDefaultCompileItems>

        <PlatformTarget>AnyCPU</PlatformTarget>
        <DebugType>pdbonly</DebugType>
        <OutputType>Exe</OutputType>
        <SelfContained>true</SelfContained>
        <PublishReadyToRun>true</PublishReadyToRun>
        <RuntimeIdentifier>{$platform}-{$arch}</RuntimeIdentifier>
        <UseHardenedRuntime>true</UseHardenedRuntime>
        <IncludeNativeLibrariesForSelfExtract>true</IncludeNativeLibrariesForSelfExtract>
        <Authors>Saxonica Ltd.</Authors>
        <PackageTags>XML,XSLT,Saxonica</PackageTags>
        <Copyright>Copyright © 2021-{year-from-date(current-date())} Saxonica, Ltd.</Copyright>
        <PackageProjectUrl>https://saxonica.com/</PackageProjectUrl>
        <PackageSummary>An XSLT 3.0 processor for .NET</PackageSummary>
        <PackageLicenseFile>LICENSE.txt</PackageLicenseFile>
        <PackageReadmeFile>README.md</PackageReadmeFile>
        <DefineConstants>{upper-case($edition)}</DefineConstants>
      </PropertyGroup>    
      
      <ItemGroup>
        <None Include="../LICENSE.txt" Pack="true" PackagePath="LICENSE.txt"/>
        <None Include="../README.md" Pack="true" PackagePath="README.md"/>
      </ItemGroup>

      <PropertyGroup Condition="'$(Configuration)' != 'Debug'">
        <PublishSingleFile>true</PublishSingleFile>
        <IncludeSymbolsInSingleFile>false</IncludeSymbolsInSingleFile>
        <Optimize>true</Optimize>
      </PropertyGroup>


      <ItemGroup>
        <PackageReference Include="DotNet.Bundle" Version="0.9.13" />
      </ItemGroup>

      <ItemGroup>
        <xsl:choose>
          <xsl:when test="exists($releaseBuild)">
            <EmbeddedResource Include="..\..\resources\**\*">
              <LogicalName>$(AssemblyName).$([System.IO.Path]::GetFullPath('%(Identity)'))</LogicalName>
            </EmbeddedResource>
          </xsl:when>
          <xsl:otherwise>
            <EmbeddedResource Include="build\resources\**\*">
              <LogicalName>$(AssemblyName).$([System.IO.Path]::GetFullPath('%(Identity)'))</LogicalName>
            </EmbeddedResource>
          </xsl:otherwise>
        </xsl:choose>
      </ItemGroup>

      <ItemGroup>
        <PackageReference Include="AngleSharp" Version="1.2.0" />
        <PackageReference Include="ICU4N" Version="60.1.0-alpha.436" />
        <PackageReference Include="ICU4N.Resources" Version="60.1.0-alpha.436" />
        <PackageReference Include="NSec.Cryptography" Version="24.4.0" />
        <PackageReference Include="Microsoft.AspNetCore.StaticFiles" Version="2.3.0" />
        <PackageReference Include="Singulink.Numerics.BigDecimal" Version="3.3.3" />
        <PackageReference Include="Singulink.Numerics.BigIntegerExtensions" Version="2.3.0" />
        <PackageReference Include="System.Collections.Immutable" Version="9.0.4" />
        <PackageReference Include="System.IO.Hashing" Version="9.0.4" />
        <PackageReference Include="System.Net.Http" Version="4.3.4" />
        <PackageReference Include="Microsoft.Extensions.Configuration" Version="9.0.4" />
        <PackageReference Include="Microsoft.Extensions.Configuration.Json" Version="9.0.4" />
        <PackageReference Include="Microsoft.Extensions.Configuration.Xml" Version="9.0.4" />
        <PackageReference Include="System.Text.Encoding.CodePages" Version="9.0.4" />
        <PackageReference Include="XmlResolver" Version="6.0.3" />
        <PackageReference Include="XmlResolverData" Version="6.0.3" />
      </ItemGroup>

      <xsl:if test="$edition != 'ee'">
        <ItemGroup>
          <Compile Remove="..\..\..\..\cs\Saxon\Impl\Helpers\UcaCollatorUsingCSharp.cs" />
          <Compile Remove="..\..\..\..\cs\Saxon\Ejava\text\CollationKeyUsingCSharp.cs" />
        </ItemGroup>
      </xsl:if>
    </Project>
  </xsl:result-document>

  <xsl:result-document href="{$templateName}.csproj" method="xml" indent="yes">
    <xsl:choose>
      <xsl:when test="$releaseBuild = 'TestDriver'">
        <Project Sdk="Microsoft.NET.Sdk">
          <Import Project="{$core.csproj}"/>
          <PropertyGroup>
            <StartupObject>TestDriver.Driver</StartupObject> 
            <AssemblyName>SaxonCSTestDriver</AssemblyName>
            <Description>QT3/4 test suite test driver.</Description>
          </PropertyGroup>
          <PropertyGroup>
            <CFBundleName>SaxonCSTestDriver</CFBundleName>
            <CFBundleDisplayName>SaxonCSTestDriver</CFBundleDisplayName>
            <CFBundleIdentifier>com.saxonica.saxoncstestdriver</CFBundleIdentifier>
            <CFBundleVersion>{$saxonVersion}</CFBundleVersion>
            <CFBundleShortVersionString>{$saxonVersion}</CFBundleShortVersionString>
            <CFBundleExecutable>SaxonCSTestDriver</CFBundleExecutable>
            <NSPrincipalClass>NSApplication</NSPrincipalClass>
            <NSHighResolutionCapable>true</NSHighResolutionCapable>
            <NSRequiresAquaSystemAppearance>false</NSRequiresAquaSystemAppearance>
          </PropertyGroup>
          <ItemGroup>
            <Compile Include="..\..\..\..\src\test\testrunner\**"/>
            <Compile Remove="..\..\..\..\**\.DS_Store"/>
          </ItemGroup>
          <ItemGroup>
            <Reference Include="SaxonCS">
              <HintPath>../SaxonCS/bin/Debug/{$framework}/{$platform}-{$arch}/SaxonCS.dll</HintPath>
            </Reference>
          </ItemGroup>
        </Project>
      </xsl:when>
      <xsl:when test="$releaseBuild = 'NUnitTests'">
        <Project Sdk="Microsoft.NET.Sdk">
          <PropertyGroup>
            <TargetFramework>{$framework}</TargetFramework>
            <ImplicitUsings>enable</ImplicitUsings>
            <Nullable>enable</Nullable>
            <IsPackable>false</IsPackable>
            <IsTestProject>true</IsTestProject>
          </PropertyGroup>
          <ItemGroup>
            <PackageReference Include="coverlet.collector" Version="6.0.4"/>
            <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.8.0"/>
            <PackageReference Include="NUnit" Version="4.3.2"/>
            <PackageReference Include="NUnit.Analyzers" Version="3.9.0"/>
            <PackageReference Include="NUnit3TestAdapter" Version="4.5.0"/>
          </ItemGroup>
          <ItemGroup>
            <Using Include="NUnit.Framework"/>
          </ItemGroup>
          <ItemGroup>
            <ProjectReference Include="..\SaxonCS\SaxonCS.csproj"/>
          </ItemGroup>
        </Project>
      </xsl:when>
      <xsl:otherwise>
        <Project Sdk="Microsoft.NET.Sdk">
          <Import Project="{$core.csproj}"/>
          <PropertyGroup>
            <AssemblyName>SaxonCS</AssemblyName>
            <Description>SaxonCS is a high-performance XSLT 3.0 processor. It conforms with the
            latest W3C specifications (notably XSLT 3.0, XQuery 3.1, and XPath 3.1).</Description>
            <PackageReleaseNotes>Our first native C# release.</PackageReleaseNotes>
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
          <xsl:choose>
            <xsl:when test="exists($releaseBuild)">
              <ItemGroup>
                <Compile Include="..\..\generated\**"/>
                <Compile Include="..\..\..\..\src\main\cs\**"/>
                <Compile Include="..\..\..\..\..\featurekeys\build\cs\**"/>
                <Compile Remove="..\..\..\..\**\.DS_Store"/>
              </ItemGroup>
            </xsl:when>
            <xsl:otherwise>
              <ItemGroup>
                <Compile Include="build\generated\**"/>
                <Compile Include="..\src\main\cs\**"/>
                <Compile Include="..\..\featurekeys\build\cs\**"/>
                <Compile Remove="..\**\.DS_Store"/>
              </ItemGroup>
            </xsl:otherwise>
          </xsl:choose>
        </Project>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:result-document>

  <xsl:if test="$includeTestDriver">
    <xsl:result-document href="{$templateName}TestDriver.csproj" method="xml" indent="yes">
      <Project Sdk="Microsoft.NET.Sdk">
        <Import Project="{$core.csproj}"/>
        <PropertyGroup>
          <StartupObject>TestDriver.Driver</StartupObject> 
          <AssemblyName>SaxonCSTestDriver</AssemblyName>
          <Description>QT3/4 test suite test driver.</Description>
        </PropertyGroup>
        <PropertyGroup>
          <CFBundleName>SaxonCSTestDriver</CFBundleName>
          <CFBundleDisplayName>SaxonCSTestDriver</CFBundleDisplayName>
          <CFBundleIdentifier>com.saxonica.saxoncstestdriver</CFBundleIdentifier>
          <CFBundleVersion>{$saxonVersion}</CFBundleVersion>
          <CFBundleShortVersionString>{$saxonVersion}</CFBundleShortVersionString>
          <CFBundleExecutable>SaxonCSTestDriver</CFBundleExecutable>
          <NSPrincipalClass>NSApplication</NSPrincipalClass>
          <NSHighResolutionCapable>true</NSHighResolutionCapable>
          <NSRequiresAquaSystemAppearance>false</NSRequiresAquaSystemAppearance>
        </PropertyGroup>
        <ItemGroup>
          <Compile Include="..\src\test\testrunner\**"/>
          <Compile Remove="..\**\.DS_Store"/>
        </ItemGroup>
        <ItemGroup>
          <Reference Include="SaxonCS">
            <HintPath>build/{$arch}/SaxonCS/bin/Debug/{$framework}/{$platform}-{$arch}/SaxonCS.dll</HintPath>
          </Reference>
        </ItemGroup>
      </Project>
    </xsl:result-document>
  </xsl:if>
</xsl:template>

</xsl:stylesheet>
