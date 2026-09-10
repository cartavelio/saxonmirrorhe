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
<xsl:param name="configuration" as="xs:string" required="yes"/>
<xsl:param name="optimize" as="xs:string" select="'true'"/>

<xsl:template name="xsl:initial-template">
  <xsl:result-document href="NUnitTests.csproj">
<Project Sdk="Microsoft.NET.Sdk">

    <PropertyGroup>
        <TargetFramework>net8.0</TargetFramework>
        <ImplicitUsings>enable</ImplicitUsings>
        <Nullable>enable</Nullable>

        <IsPackable>false</IsPackable>
        <IsTestProject>true</IsTestProject>
    </PropertyGroup>

    <ItemGroup>
        <PackageReference Include="coverlet.collector" Version="6.0.0"/>
        <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.8.0"/>
        <PackageReference Include="NUnit" Version="4.3.2" />
        <PackageReference Include="NUnit.Analyzers" Version="3.9.0"/>
        <PackageReference Include="NUnit3TestAdapter" Version="4.5.0"/>
    </ItemGroup>

    <ItemGroup>
        <Using Include="NUnit.Framework"/>
    </ItemGroup>

    <ItemGroup>
      <ProjectReference Include="..\SaxonCS\SaxonCS.csproj" />
    </ItemGroup>

</Project>
  </xsl:result-document>
</xsl:template>

</xsl:stylesheet>
