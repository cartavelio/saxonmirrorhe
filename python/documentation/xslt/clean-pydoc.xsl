<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:xs="http://www.w3.org/2001/XMLSchema"
  xmlns:math="http://www.w3.org/2005/xpath-functions/math"
  exclude-result-prefixes="xs math"
  version="3.0">

  <xsl:output method="html"/>

  <xsl:mode on-no-match="shallow-copy"/>
  <xsl:mode name="top-bar" on-no-match="shallow-copy"/>

  <xsl:template match="*:table[@summary='heading']">
    <xsl:copy>
      <xsl:apply-templates select="@*"/>
      <xsl:apply-templates mode="top-bar"/>
    </xsl:copy>
  </xsl:template>

  <!-- kill the <a> that includes file system path -->
  <xsl:template match="*:a[starts-with(@href, 'file:') and ends-with(@href, '.so')]" mode="top-bar"/>

  <!-- rewrite the <a> that links to the doc index -->
  <xsl:template match="*:a[@href = '.' and . = 'index']" mode="top-bar">
    <xsl:copy>
      <xsl:attribute name="href" select="'/saxon-c/documentation12/index.html#!api/saxon_c_python_api'"/>
      <xsl:text>index</xsl:text>
    </xsl:copy>
  </xsl:template>
</xsl:stylesheet>
