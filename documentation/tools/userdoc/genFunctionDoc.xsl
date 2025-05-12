<?xml version="1.0" encoding="UTF-8"?> 
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:xs="http://www.w3.org/2001/XMLSchema"
  xmlns:math="http://www.w3.org/2005/xpath-functions/math"
  xmlns:doc="http://www.saxonica.com/ns/doc" xmlns:sDoc="http://www.saxonica.com/ns/doc/functions"
  xmlns:fos="http://www.w3.org/xpath-functions/spec/namespace"
  xmlns:map="http://www.w3.org/2005/xpath-functions/map"
  exclude-result-prefixes="xs math sDoc fos map" version="3.0">

  <xsl:param name="GitHub" as="xs:string"/>

  <xsl:output method="xml" indent="no"/>

  <xsl:mode on-no-match="shallow-copy"/>

  <xsl:variable name="catalog"
    select="doc(resolve-uri(replace(section/@catalog,'\$\{GitHub\}',$GitHub),base-uri(.)))"
    doc:doc="The function catalog that contains the signatures for the functions in this section"/>
  <xsl:variable name="namespace" select="section/@namespace"
    doc:doc="The namespace for these functions"/>
  <xsl:variable name="spec" select="section/@spec"
    doc:doc="The specification within which this set of functions is defined"/>

  <xsl:variable name="defaults" select="section/defaults"
    doc:doc="Default values for parts of the descriptions of each function"/>
  <xsl:variable name="implementation-doc" select="doc('../../src/viewer_appJS/doc/implement.xml')"
    as="document-node()" doc:doc="Document containing implementation status descriptions"/>
  <xsl:variable name="implementation" select="$implementation-doc/implement"
    doc:doc="Defaults for the implementation status descriptions"/>

  <xsl:variable name="args" as="map(xs:string,xs:string)"
    select="map:merge($defaults/arg-descriptions/* ! map:entry(name(.),xs:string(.)))"
    doc:doc="Default descriptions for the function arguments, keyed by name.
        It helps if the use of argument names is consistent throughout the module."/>
  <xsl:variable name="use-last" as="map(*)" select="map{'duplicates':'use-last'}"/>

  <xsl:variable name="implement.saxon" as="map(xs:string,xs:string)"
    select="map:merge($implementation/saxon ! map:entry(string(@version),xs:string(.)))"/>
  <xsl:variable name="implement.xpath" as="map(xs:string,xs:string)"
    select="map:merge($implementation/xpath ! map:entry(string(@version),xs:string(.)))"/>
  <xsl:variable name="default.saxon" select="$implementation/default[@id = $spec]/@saxon"/>
  <xsl:variable name="default.xpath" select="$implementation/default[@id = $spec]/@xpath"/>

  <xsl:template match="section">
    <xsl:copy>
      <xsl:sequence select="@id|@title"/>
      <xsl:comment>Generated - <xsl:value-of select="current-dateTime()"/></xsl:comment>
      <xsl:apply-templates select="h1|function" mode="#current">
        <xsl:sort select="@name"/>
      </xsl:apply-templates>
    </xsl:copy>
  </xsl:template>
  <xsl:template match="function[@name]">
    <function xmlns="http://www.saxonica.com/ns/doc/functions">
      <xsl:variable name="args" select="map:merge(($args, (args/* !
        map:entry(name(.),xs:string(.)))), $use-last)"/>
      <name namespace="{$namespace}">
        <xsl:value-of select="@name"/>
      </name>
      <xsl:apply-templates select="$catalog//fos:function[@name=current()/@name]">
        <xsl:with-param name="args" select="$args" tunnel="yes"/>
      </xsl:apply-templates>

      <xsl:for-each select="status">
        <status>
          <xsl:copy-of select="*|text()"/>
        </status>
      </xsl:for-each>
      <xpath>
        <xsl:value-of select="(@xpath,$default.xpath,'default')[1]"/>
      </xpath>
      <saxon-edition>
        <xsl:value-of select="(@saxon,$default.saxon,' ')[1]"/>
      </saxon-edition>
    </function>
  </xsl:template>
  <xsl:template match="section/h1">
    <xsl:copy-of select="."/>
  </xsl:template>

  <xsl:template match="fos:* | *">
    <xsl:element name="{local-name(.)}" namespace="http://www.saxonica.com/ns/doc/functions">
      <xsl:apply-templates select="@*|*|text()" mode="#current"/>
    </xsl:element>
  </xsl:template>
  <xsl:template match="fos:rules | fos:examples | fos:errors | fos:notes "/>
  <xsl:template match="fos:function">
    <xsl:apply-templates select="*" mode="#current"/>
  </xsl:template>
  <xsl:template match="fos:summary">
    <description xmlns="http://www.saxonica.com/ns/doc/functions">
      <xsl:apply-templates select="*" mode="#current"/>
    </description>
  </xsl:template>
  <xsl:template match="loc">
    <a xmlns="http://www.saxonica.com/ns/doc/functions">
      <xsl:apply-templates select="@*|*|text()" mode="#current"/>
    </a>
  </xsl:template>
  <xsl:template match="fos:proto">
    <proto xmlns="http://www.saxonica.com/ns/doc/functions">
      <xsl:apply-templates select="(@* except @name),*" mode="#current"/>
      <xsl:for-each select="tokenize($spec,'\s+')">
        <in-spec>
          <xsl:value-of select="."/>
        </in-spec>
      </xsl:for-each>
    </proto>
  </xsl:template>
  <xsl:template match="fos:arg">
    <xsl:param name="args" as="map(xs:string,xs:string)" tunnel="yes"/>
    <arg xmlns="http://www.saxonica.com/ns/doc/functions">
      <xsl:apply-templates select="@*" mode="#current"/>
      <xsl:if test="$args(@name)">
        <xsl:attribute name="desc" namespace="" select="$args(@name)"/>
      </xsl:if>
      <xsl:apply-templates select="*" mode="#current"/>
    </arg>
  </xsl:template>
  
  <!-- Fix a link on the file:parent() page  -->
  <xsl:template match="loc[@href='#fn.resolve-path']">
    <a xmlns="http://www.saxonica.com/ns/doc/functions" href="../resolve-path">
      <xsl:apply-templates select="(@* except @href)|*|text()" mode="#current"/>
    </a>
  </xsl:template>


</xsl:stylesheet>
