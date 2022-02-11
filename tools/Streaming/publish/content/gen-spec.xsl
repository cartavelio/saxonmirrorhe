<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:pkg="http://expath.org/ns/pkg"
  xmlns:web="http://expath.org/ns/webapp" xmlns:app="http://saxonica.com/tools/analyze-stream"
  xmlns:saxon="http://saxon.sf.net/" xmlns:doc="http://www.saxonica.com/ns/documentation"
  xmlns:H="http://www.w3.org/1999/xhtml" exclude-result-prefixes="#all" version="3.0">

  <pkg:import-uri>http://saxonica.com/tools/analyze-stream/gen-spec.xsl</pkg:import-uri>

  <xsl:output name="xml" method="xml" indent="yes"/>
  <!--<xsl:output method="html" omit-xml-declaration="yes"/>-->

  <xsl:variable name="spec.name"
    doc:doc="The XLST3.0 spec URL relative to the static URI (this stylesheet)"
    >http://www.w3.org/TR/xslt-30/</xsl:variable>
  <xsl:variable name="spec" select="doc($spec.name)"/>
  <xsl:variable name="spec.ref" select="$spec//H:h2[H:a/@id='w3c-doctype']"/>
  <xsl:variable name="spec.out"
    doc:doc="The name to use for the XSLT3.0 spec, relative to the output URI"
    >XSLT.3.0.Spec.xhtml</xsl:variable>
  <xsl:variable name="GSR" select="$spec//H:div[H:h4/H:a/@id='general-streamability-rules']"/>

  <xsl:template match="/">

    <web:response status="200" message="Ok">
      <!-- Depends on the Accept header actually, see
              http://www.w3.org/TR/xhtml-media-types/.  Good candidate
              for a tool function provided by Servlex itself. -->
      <web:body content-type="text/html" method="html"/>
    </web:response>
    <xsl:apply-templates select="$spec/*"/>
  </xsl:template>

  <xsl:mode on-no-match="shallow-copy"/>
  
  <xsl:template match="H:object/@data[starts-with(.,'img/')]">
    <xsl:attribute name="{local-name(.)}" select="$spec.name||."/>
  </xsl:template>
  <xsl:template match="H:div[H:h4/H:a/@id='general-streamability-rules']">
    <xsl:copy>
      <xsl:sequence select="@*"/>
      <xsl:attribute name="id">gsr</xsl:attribute>
      <xsl:apply-templates select="*" mode="id-GSR"/>
    </xsl:copy>
  </xsl:template>

  <xsl:mode name="id-GSR" on-no-match="shallow-copy"/>
  <xsl:template match="H:li" mode="id-GSR">
    <xsl:variable name="index">
      <xsl:number level="multiple" format="1.a.i.A.I"/>
    </xsl:variable>
    <xsl:copy>
      <xsl:sequence select="@*"/>
      <!--<xsl:attribute name="index" select="$index"/>-->
      <a name="gsr-{$index}" xmlns="http://www.w3.org/1999/xhtml"/>
      <xsl:apply-templates select="*|text()" mode="#current"/>
    </xsl:copy>
  </xsl:template>
  <xsl:template match="H:li//H:td" mode="id-GSR">
    <xsl:variable name="index">
      <xsl:number count="H:li|H:tr|H:td" level="multiple" format="1.a.i.A.1.1"/>
    </xsl:variable>
    <xsl:copy>
      <xsl:sequence select="@*"/>
      <!--<xsl:attribute name="index" select="$index"/>-->
      <a name="gsr-{$index}" xmlns="http://www.w3.org/1999/xhtml"/>
      <xsl:apply-templates select="*|text()" mode="#current"/>
    </xsl:copy>
  </xsl:template>
</xsl:stylesheet>
