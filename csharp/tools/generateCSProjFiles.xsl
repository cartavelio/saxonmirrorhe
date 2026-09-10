<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:xs="http://www.w3.org/2001/XMLSchema"
                xmlns:f="http://saxonica.com/ns/functions"
                exclude-result-prefixes="f xs"
                expand-text="yes"
                version="3.0">

<xsl:output method="text" encoding="utf-8"/>

<xsl:param name="releaseBuild" as="xs:string?" select="()"/>
<xsl:param name="nunitBuild" as="xs:string?" select="'false'"/>

<xsl:param name="release" as="xs:string" required="yes"/>
<xsl:param name="platform" as="xs:string" required="yes"/>
<xsl:param name="arch" as="xs:string" required="yes"/>
<xsl:param name="saxonVersion" as="xs:string" required="yes"/>
<xsl:param name="edition" as="xs:string" required="yes"/>
<xsl:param name="framework" as="xs:string" required="yes"/>

<xsl:param name="includeTestDriver" as="xs:boolean"
           select="empty($releaseBuild) and not($nunitBuild = 'true')"/>
<xsl:param name="core.csproj" as="xs:string" select="'core.csproj'"/>

<!-- what. a. hack. -->
<xsl:param name="uuid1" as="xs:string" select="'fake-uuid-1'"/>
<xsl:param name="uuid2" as="xs:string" select="'fake-uuid-2'"/>
<xsl:param name="uuid3" as="xs:string" select="'fake-uuid-3'"/>
<xsl:param name="uuid4" as="xs:string" select="'fake-uuid-4'"/>

<xsl:variable name="common-packages" as="element()*"
              select="f:package-reference(('AngleSharp',
                                           'ICU4N',
                                           'ICU4N.Resources',
                                           'NSec.Cryptography',
                                           'Microsoft.AspNetCore.StaticFiles',
                                           'Singulink.Numerics.BigDecimal',
                                           'Singulink.Numerics.BigIntegerExtensions',
                                           'System.Collections.Immutable',
                                           'System.IO.Hashing',
                                           'System.Net.Http',
                                           'Microsoft.Extensions.Configuration',
                                           'Microsoft.Extensions.Configuration.Json',
                                           'Microsoft.Extensions.Configuration.Xml',
                                           'System.Text.Encoding.CodePages',
                                           'XmlResolver',
                                           'XmlResolverData'))"/>

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
    <xsl:text>Project("{{{$uuid1}}}") = "TestDriver", "{$templateName}TestDriver.csproj", "{{{$uuid3}}}"
EndProject
Project("{{{$uuid1}}}") = "NUnitTests", "NUnitTests.csproj", "{{{$uuid4}}}"
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
    <xsl:text>                {{{$uuid3}}}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
                {{{$uuid3}}}.Debug|Any CPU.Build.0 = Debug|Any CPU
                {{{$uuid3}}}.Release|Any CPU.ActiveCfg = Release|Any CPU
                {{{$uuid3}}}.Release|Any CPU.Build.0 = Release|Any CPU
                {{{$uuid4}}}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
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
        <xsl:choose>
          <xsl:when test="$nunitBuild = 'true'">
            <LangVersion>10.0</LangVersion>
            <PackageSummary>NUnit tests for {upper-case($edition)}</PackageSummary>
          </xsl:when>
          <xsl:otherwise>
            <LangVersion>9.0</LangVersion>
            <PackageSummary>An XSLT 3.0 processor for .NET</PackageSummary>
          </xsl:otherwise>
        </xsl:choose>

        <TargetFramework>{$framework}</TargetFramework>
        <Version>{$saxonVersion}</Version>
        <AssemblyVersion>{$saxonVersion}</AssemblyVersion>
        <Nullable>disable</Nullable>
        <NoWarn>2200;0414;0168;0169;0649;1591</NoWarn>
        <GenerateDocumentationFile>true</GenerateDocumentationFile>
        <EnableDefaultCompileItems>false</EnableDefaultCompileItems>
        <IsTrimmable>true</IsTrimmable>

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
        <xsl:if test="not($nunitBuild = 'true')">
          <PackageLicenseFile>LICENSE.txt</PackageLicenseFile>
          <PackageReadmeFile>README.md</PackageReadmeFile>
          <DefineConstants>{upper-case($edition)}</DefineConstants>
        </xsl:if>
      </PropertyGroup>    
      
      <xsl:if test="not($nunitBuild = 'true')">
        <ItemGroup>
          <None Include="../LICENSE.txt" Pack="true" PackagePath="LICENSE.txt"/>
          <None Include="../README.md" Pack="true" PackagePath="README.md"/>
        </ItemGroup>

        <PropertyGroup Condition="'$(Configuration)' != 'Debug'">
          <PublishSingleFile>true</PublishSingleFile>
          <IncludeSymbolsInSingleFile>false</IncludeSymbolsInSingleFile>
          <Optimize>true</Optimize>
        </PropertyGroup>
      </xsl:if>

      <ItemGroup>
        <xsl:sequence select="f:package-reference('DotNet.Bundle')"/>
      </ItemGroup>

      <ItemGroup>
        <xsl:choose>
          <xsl:when test="$nunitBuild = 'true'">
            <EmbeddedResource Include="..\..\..\ee\build\resources\**\*">
              <LogicalName>$(AssemblyName).$([System.IO.Path]::GetFullPath('%(Identity)'))</LogicalName>
            </EmbeddedResource>
          </xsl:when>
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
        <xsl:sequence select="$common-packages"/>
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
      <xsl:when test="$nunitBuild = 'true'">
        <Project Sdk="Microsoft.NET.Sdk">
          <Import Project="core.csproj"/>
          <PropertyGroup>
            <Description>SaxonCS NUnit tests.</Description>
            <IsTestProject>true</IsTestProject>
            <DefineConstants>{upper-case($edition)}</DefineConstants>
          </PropertyGroup>
          <ItemGroup>
            <Compile Include="SaxonCS-NUnit\**\*.cs"/>
            <Compile Remove="SaxonCS-NUnit\**\.DS_Store"/>
          </ItemGroup>
          <ItemGroup>
            <Reference Include="SaxonCS">
              <HintPath>..\..\..\{$edition}\build\{$arch}\SaxonCS\bin\{$release}\{$framework}\{$platform}-{$arch}\SaxonCS.dll</HintPath>
            </Reference>
          </ItemGroup>
          <ItemGroup>
            <xsl:sequence select="f:package-reference(('coverlet.collector',
                                                       'Microsoft.NET.Test.Sdk',
                                                       'NUnit',
                                                       'NUnit.Analyzers',
                                                       'NUnit3TestAdapter'))"/>
            <xsl:sequence select="$common-packages"/>
          </ItemGroup>
          <ItemGroup>
            <Using Include="NUnit.Framework"/>
          </ItemGroup>
        </Project>
      </xsl:when>
      <xsl:when test="$releaseBuild = 'TestDriver'">
        <xsl:call-template name="TestDriver.csproj">
          <xsl:with-param name="relative-src-path" select="'../../../../'"/>
          <xsl:with-param name="relative-hint-path" select="'../SaxonCS/'"/>
        </xsl:call-template>
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
            <xsl:sequence select="f:package-reference(('coverlet.collector',
                                                       'Microsoft.NET.Test.Sdk',
                                                       'NUnit',
                                                       'NUnit.Analyzers',
                                                       'NUnit3TestAdapter'))"/>
            <xsl:sequence select="$common-packages"/>
          </ItemGroup>
          <ItemGroup>
            <Using Include="NUnit.Framework"/>
          </ItemGroup>
          <ItemGroup>
            <ProjectReference Include="..\SaxonCS\SaxonCS.csproj"/>
          </ItemGroup>
        </Project>
      </xsl:when>
      <xsl:when test="$releaseBuild = 'TestDrivers'">
        <xsl:call-template name="TestDriver.csproj">
          <xsl:with-param name="relative-src-path" select="'../../../../'"/>
          <xsl:with-param name="relative-hint-path" select="'../SaxonCS/'"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
        <Project Sdk="Microsoft.NET.Sdk">
          <Import Project="{$core.csproj}"/>
          <PropertyGroup>
            <StartupObject>Saxon.Cmd.Command</StartupObject>
            <OutputType>Exe</OutputType>
            <AssemblyName>SaxonCS</AssemblyName>
            <Description>SaxonCS is a high-performance XSLT 3.0 processor. It conforms with the
            latest W3C specifications (notably XSLT 3.0, XQuery 3.1, and XPath 3.1).</Description>
            <PackageReleaseNotes>Our first native C# release.</PackageReleaseNotes>
          </PropertyGroup>
          <PropertyGroup>
            <!-- This is inconsistent with generating a .nuget package
                 And didn't see to have much performance benefit
            <PublishAot>True</PublishAot>
            -->
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
                <xsl:if test="$edition != 'ee'">
                  <Compile Remove="..\..\..\..\src\main\cs\Saxon\Api\Support\Xsd*"/>
                  <Compile Remove="..\..\..\..\src\main\cs\Saxon\Impl\Overrides\Verifier.cs"/>
                  <Compile Remove="..\..\..\..\src\main\cs\Saxon\Impl\Parallel\**"/>
                </xsl:if>
              </ItemGroup>
            </xsl:when>
            <xsl:otherwise>
              <ItemGroup>
                <Compile Include="build\generated\**"/>
                <Compile Include="..\src\main\cs\**"/>
                <Compile Include="..\..\featurekeys\build\cs\**"/>
                <Compile Remove="..\**\.DS_Store"/>
                <xsl:if test="$edition != 'ee'">
                  <Compile Remove="..\src\main\cs\Saxon\Api\Support\Xsd*"/>
                  <Compile Remove="..\src\main\cs\Saxon\Impl\Overrides\Verifier.cs"/>
                  <Compile Remove="..\src\main\cs\Saxon\Impl\Parallel\**"/>
                </xsl:if>
              </ItemGroup>
            </xsl:otherwise>
          </xsl:choose>
        </Project>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:result-document>

  <xsl:if test="$includeTestDriver">
    <xsl:result-document href="{$templateName}TestDriver.csproj" method="xml" indent="yes">
      <xsl:call-template name="TestDriver.csproj">
        <xsl:with-param name="relative-src-path" select="'../'"/>
        <xsl:with-param name="relative-hint-path" select="''"/>
      </xsl:call-template>
    </xsl:result-document>
    <xsl:result-document href="NUnitTests.csproj" method="xml" indent="yes">
      <xsl:call-template name="NUnitTests.csproj">
        <xsl:with-param name="relative-src-path" select="'../'"/>
        <xsl:with-param name="relative-hint-path" select="''"/>
      </xsl:call-template>
    </xsl:result-document>
  </xsl:if>
</xsl:template>

<xsl:template name="TestDriver.csproj">
  <xsl:param name="relative-src-path" as="xs:string" required="yes"/>
  <xsl:param name="relative-hint-path" as="xs:string" required="yes"/>

  <Project Sdk="Microsoft.NET.Sdk">
    <Import Project="{$core.csproj}"/>
    <PropertyGroup>
      <StartupObject>TestDriver.Driver</StartupObject> 
      <OutputType>Exe</OutputType>
      <AssemblyName>TestDrivers</AssemblyName>
      <Description>QT3/4 test suite test driver.</Description>
    </PropertyGroup>
    <PropertyGroup>
      <CFBundleName>TestDrivers</CFBundleName>
      <CFBundleDisplayName>TestDrivers</CFBundleDisplayName>
      <CFBundleIdentifier>com.saxonica.saxoncstestdriver</CFBundleIdentifier>
      <CFBundleVersion>{$saxonVersion}</CFBundleVersion>
      <CFBundleShortVersionString>{$saxonVersion}</CFBundleShortVersionString>
      <CFBundleExecutable>TestDrivers</CFBundleExecutable>
      <NSPrincipalClass>NSApplication</NSPrincipalClass>
      <NSHighResolutionCapable>true</NSHighResolutionCapable>
      <NSRequiresAquaSystemAppearance>false</NSRequiresAquaSystemAppearance>
    </PropertyGroup>
    <ItemGroup>
      <Compile Include="{translate($relative-src-path, '/', '\')}src\test\testrunner\**"/>
      <Compile Remove="{translate($relative-src-path, '/', '\')}**\.DS_Store"/>
    </ItemGroup>
    <ItemGroup>
      <Reference Include="SaxonCS">
        <HintPath>{translate($relative-hint-path, '/', '\')}bin\{$release}\{$framework}\{$platform}-{$arch}\SaxonCS.dll</HintPath>
      </Reference>
    </ItemGroup>
  </Project>
</xsl:template>

<xsl:template name="NUnitTests.csproj">
  <xsl:param name="relative-src-path" as="xs:string" required="yes"/>
  <xsl:param name="relative-hint-path" as="xs:string" required="yes"/>

<Project Sdk="Microsoft.NET.Sdk">
   <Import Project="core.csproj"/>
   <PropertyGroup>
      <LangVersion>10.0</LangVersion>
      <TargetFramework>net8.0</TargetFramework>
      <ImplicitUsings>enable</ImplicitUsings>
      <Nullable>enable</Nullable>
      <IsPackable>false</IsPackable>
      <IsTestProject>true</IsTestProject>
   </PropertyGroup>
   <ItemGroup>
     <Compile Include="{translate($relative-src-path, '/', '\')}src\test\nunit\SaxonCS-NUnit\**\*.cs"/>
     <Compile Remove="{translate($relative-src-path, '/', '\')}src\test\nunit\SaxonCS-NUnit\**\.DS_Store"/>
   </ItemGroup>
   <ItemGroup>
     <xsl:sequence select="f:package-reference(('coverlet.collector',
                                                'Microsoft.NET.Test.Sdk',
                                                'NUnit',
                                                'NUnit.Analyzers',
                                                'NUnit3TestAdapter'))"/>

   </ItemGroup>
   <ItemGroup>
      <Using Include="NUnit.Framework"/>
   </ItemGroup>
   <ItemGroup>
      <ProjectReference Include="SaxonCS.csproj"/>
   </ItemGroup>
</Project>
</xsl:template>

<!-- This seems like a bit of a hack, but we want a single point of definition
     so that we get consistent references. -->
<xsl:function name="f:package-reference" as="element(PackageReference)+">
  <xsl:param name="package" as="xs:string+"/>
  <xsl:for-each select="$package">
    <xsl:choose>
      <xsl:when test=". = 'AngleSharp'">
        <PackageReference Include="AngleSharp" Version="1.2.0" />
      </xsl:when>
      <xsl:when test=". = 'DotNet.Bundle'">
        <PackageReference Include="DotNet.Bundle" Version="0.9.13" />
      </xsl:when>
      <xsl:when test=". = 'ICU4N'">
        <PackageReference Include="ICU4N" Version="60.1.0-alpha.436" />
      </xsl:when>
      <xsl:when test=". = 'ICU4N.Resources'">
        <PackageReference Include="ICU4N.Resources" Version="60.1.0-alpha.436" />
      </xsl:when>
      <xsl:when test=". = 'Microsoft.AspNetCore.StaticFiles'">
        <PackageReference Include="Microsoft.AspNetCore.StaticFiles" Version="2.3.8" />
      </xsl:when>
      <xsl:when test=". = 'Microsoft.Extensions.Configuration'">
        <PackageReference Include="Microsoft.Extensions.Configuration" Version="10.0.7" />
      </xsl:when>
      <xsl:when test=". = 'Microsoft.Extensions.Configuration.Json'">
        <PackageReference Include="Microsoft.Extensions.Configuration.Json" Version="10.0.7" />
      </xsl:when>
      <xsl:when test=". = 'Microsoft.Extensions.Configuration.Xml'">
        <PackageReference Include="Microsoft.Extensions.Configuration.Xml" Version="10.0.7" />
      </xsl:when>
      <xsl:when test=". = 'Microsoft.NET.Test.Sdk'">
        <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.8.0"/>
      </xsl:when>
      <xsl:when test=". = 'NSec.Cryptography'">
        <PackageReference Include="NSec.Cryptography" Version="24.4.0" />
      </xsl:when>
      <xsl:when test=". = 'NUnit'">
        <PackageReference Include="NUnit" Version="4.3.2"/>
      </xsl:when>
      <xsl:when test=". = 'NUnit.Analyzers'">
        <PackageReference Include="NUnit.Analyzers" Version="3.9.0"/>
      </xsl:when>
      <xsl:when test=". = 'NUnit3TestAdapter'">
        <PackageReference Include="NUnit3TestAdapter" Version="4.5.0"/>
      </xsl:when>
      <xsl:when test=". = 'Singulink.Numerics.BigDecimal'">
        <PackageReference Include="Singulink.Numerics.BigDecimal" Version="3.4.0" />
      </xsl:when>
      <xsl:when test=". = 'Singulink.Numerics.BigIntegerExtensions'">
        <PackageReference Include="Singulink.Numerics.BigIntegerExtensions" Version="2.3.2" />
      </xsl:when>
      <xsl:when test=". = 'System.Collections.Immutable'">
        <PackageReference Include="System.Collections.Immutable" Version="10.0.7" />
      </xsl:when>
      <xsl:when test=". = 'System.IO.Hashing'">
        <PackageReference Include="System.IO.Hashing" Version="10.0.7" />
      </xsl:when>
      <xsl:when test=". = 'System.Net.Http'">
        <PackageReference Include="System.Net.Http" Version="4.3.4" />
      </xsl:when>
      <xsl:when test=". = 'System.Text.Encoding.CodePages'">
        <PackageReference Include="System.Text.Encoding.CodePages" Version="10.0.7" />
      </xsl:when>
      <xsl:when test=". = 'XmlResolver'">
        <PackageReference Include="XmlResolver" Version="6.0.23" />
      </xsl:when>
      <xsl:when test=". = 'XmlResolverData'">
        <PackageReference Include="XmlResolverData" Version="6.0.23" />
      </xsl:when>
      <xsl:when test=". = 'coverlet.collector'">
        <PackageReference Include="coverlet.collector" Version="6.0.4"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:message terminate="yes" select="'Unknown package:', ."/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:for-each>
</xsl:function>

</xsl:stylesheet>
