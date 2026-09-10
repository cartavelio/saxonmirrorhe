<?xml version="1.0" encoding="utf-8"?> 

<xsl:stylesheet
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="4.0"
  xmlns:xs="http://www.w3.org/2001/XMLSchema"
  xmlns:saxon="http://saxon.sf.net/"
  xmlns:cx="http://saxon.sf.net/complex"
  exclude-result-prefixes="xs saxon cx"
  expand-text="yes"
  >
  
  <!-- Static type error -->
  
  <xsl:variable name="x" select="current-date()" as="union(xs:gYear, xs:gMonth, xs:gDay)"/>
  
  <xsl:template name="xsl:initial-template">
    <out>17</out>
  </xsl:template>  
  
</xsl:stylesheet>
