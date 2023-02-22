<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:xs="http://www.w3.org/2001/XMLSchema"
  xmlns:math="http://www.w3.org/2005/xpath-functions/math"
  xmlns:doc="http://www.saxonica.com/ns/documentation" xmlns:dc="http://purl.org/dc/elements/1.1/"
  exclude-result-prefixes="xs math" version="3.0">

  <doc:meta>
    <dc:title>Abel Examples</dc:title>
  </doc:meta>

  <xsl:mode name="streaming" streamable="yes"/>
  <xsl:mode name="non-streaming" streamable="no"/>
  
  <xsl:template match="employees" mode="streaming">
    <xsl:for-each select="employee[@name = 'john']/copy-of()">
      <xsl:apply-templates select="." mode="non-streaming"/>
    </xsl:for-each>
  </xsl:template>
  
  <xsl:template match="employees" mode="streaming">
    <xsl:for-each select="employee[@name = 'john']/copy-of()">
      <xsl:apply-templates select="." mode="streaming"/>
    </xsl:for-each>
  </xsl:template>
  
  <xsl:template match="(articles | Articles)" mode="streaming">
    <xsl:copy-of
      select="
            article/info[position() lt 10]/
            copy-of()[author = 'john' or name = 'John']
            /(abstract | title)"
    />
  </xsl:template>


</xsl:stylesheet>
