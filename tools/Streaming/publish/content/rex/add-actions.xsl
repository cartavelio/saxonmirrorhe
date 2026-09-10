<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:xs="http://www.w3.org/2001/XMLSchema"
  xmlns:math="http://www.w3.org/2005/xpath-functions/math" xmlns:p="CR-xpath-30-20130108"
  xmlns:X="XSLT2" exclude-result-prefixes="math" version="3.0">
  <xsl:output method="xml" indent="yes"/>
  <xsl:namespace-alias stylesheet-prefix="X" result-prefix="xsl"/>
  <xsl:template match="xsl:stylesheet">
    <xsl:copy>
      <xsl:sequence select="@*"/>
      <p:dummy/>
      <xsl:apply-templates/>
    </xsl:copy>
  </xsl:template>
  <xsl:mode on-no-match="shallow-copy"/>

  <!--  <xsl:template match="xsl:function[starts-with(@name,'p:parse-')]">
    <p:action/>
    <xsl:next-match/>
  </xsl:template>-->
  <xsl:template use-when="true()"
    match="xsl:function[starts-with(@name,'p:parse-')]/xsl:sequence[@select='$state'][last()]">
    <X:sequence select="subsequence($state,1,$p:result - 1 )"/>
    <xsl:element name="{substring-after(../@name,'p:parse-')}">
      <X:sequence select="subsequence($state,$p:result)"/>
    </xsl:element>
  </xsl:template>

  <xsl:template
    match="xsl:when[starts-with(@test,'$state[$p:lk] =')]/
    xsl:sequence[@select='$state'][preceding-sibling::xsl:variable[starts-with(@select,'p:parse-')]] |
    xsl:otherwise/
    xsl:sequence[@select='$state'][preceding-sibling::xsl:variable[starts-with(@select,'p:parse-')]]">
    <X:sequence select="subsequence($state,1,$p:result - 1 )"/>
    <xsl:element name="{substring-before(substring-after((preceding-sibling::xsl:variable/@select)[1],'p:parse-'),'(')}">
      <X:sequence select="subsequence($state,$p:result)"/>
    </xsl:element>

  </xsl:template>
</xsl:stylesheet>
