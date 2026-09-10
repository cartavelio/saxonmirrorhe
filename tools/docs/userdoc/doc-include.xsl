<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:xs="http://www.w3.org/2001/XMLSchema" exclude-result-prefixes="xs doc cat"
  xmlns:doc="http://www.saxonica.com/ns/doc" xmlns:cat="http://www.saxonica.com/ns/doc/catalog"
  version="3.0">
  <doc:doc>
    <p>This simple stylesheet processes embedded xinclude directives in the documentation. Its
      principal input is the catalog.xml file within src/userdoc.
      <!-- For any
        <code>cat:section</code> entry that has <code>@include='yes'</code>, it is assumed that the
      master file, which may contain inclusions, is contained at <code>{@ref}/{@ref}.xml</code> - it
      will be include-processed to generate <code>{@ref}.xml</code> as a sibling of the catalog
      file. --></p>
    <p>It requires execution with the -xi:on option to handle inclusions.</p>
    <p>Modified 2018-09-21: to also remove comment nodes.</p>
    <p>Modified 2022-01-25: to also expand feature links.</p>
  </doc:doc>
  
  <xsl:param name="output-uri" select="''"/>  

  <xsl:output method="xml" indent="no"/>
  <xsl:variable name="base-uri" select="base-uri(/)"/>

  <xsl:mode on-no-match="shallow-copy"/>
  <xsl:template match="/">
    <xsl:apply-templates select=".//cat:section" mode="include"/>
    <xsl:next-match/>
  </xsl:template>
  <xsl:template match="cat:catalog">
    <xsl:copy>
      <xsl:apply-templates select="@*" mode="#current"/>
      <xsl:comment>Processed:
        <xsl:value-of use-when="true()" select="format-dateTime(current-dateTime(),'[Y] [MNn] [D01] @ [H01]:[m]')"/>
        <xsl:value-of use-when="false()" select="current-dateTime()"/>
      </xsl:comment>
      <xsl:apply-templates select="*" mode="#current"/>
    </xsl:copy>
  </xsl:template>

  <xsl:template match="cat:section" mode="include">
    <!--<xsl:variable name="dir" select="if(@include='yes') then (@ref,'/') else ()"/>
    <xsl:variable name="src" select="resolve-uri(string-join(($dir,@ref,'.xml')),$base-uri)"/>-->
    <xsl:variable name="name" select="string-join((@ref,'.xml'))"/>
    <xsl:variable name="src" select="resolve-uri($name,$base-uri)"/>
    <xsl:choose>
      <xsl:when test="doc-available(xs:string($src))">
      <!--<xsl:variable name="href"
        select="if(exists($output))
            then resolve-uri($name,resolve-uri($output,$base-uri)) 
            else $name"/>
      <xsl:result-document href="{$href}">-->      
      <xsl:result-document href="{$output-uri}/{$name}">
        <xsl:apply-templates select="document($src)"/>
      </xsl:result-document>
      </xsl:when>
      <xsl:otherwise>
        <xsl:message select="'Failed to load: …/build/' || substring-after($src, '/build/')"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template match="xi:include" xmlns:xi="http://www.w3.org/2001/XInclude">
    <xsl:message>WHAT THE HECK? <xsl:value-of select="@href"/></xsl:message>
  </xsl:template>
  
  <xsl:template match="*:a[@class='feature']">
    <xsl:copy>
      <xsl:attribute name="class" select="'bodylink'"/>
      <xsl:attribute name="href" select="concat('/configuration/config-features@',@href)"/>
      <xsl:value-of select="@href"/>
    </xsl:copy>   
  </xsl:template>
  
  <xsl:template match="comment()"/>

</xsl:stylesheet>
