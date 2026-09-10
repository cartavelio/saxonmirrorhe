<?xml version="1.0" encoding="utf-8"?> 

<xsl:stylesheet
   xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="4.0"
   xmlns:xs="http://www.w3.org/2001/XMLSchema"
   xmlns:saxon="http://saxon.sf.net/"
   xmlns:cx="http://saxon.sf.net/complex"
   exclude-result-prefixes="xs saxon cx"
   expand-text="yes"
>

  <!-- atomic() and union() as match patterns -->
  
  <xsl:template match="atomic(xs:anyAtomicType)" priority="3">
    <AA>{.}</AA>
  </xsl:template>
  
  <xsl:template match="union(xs:date, xs:dateTime, xs:time)" priority="4">
    <TT>{.}</TT>
  </xsl:template>

  
  <xsl:template name="xsl:initial-template">
    <out>
      <xsl:apply-templates select="current-date()"/>
    </out>
  </xsl:template>  

</xsl:stylesheet>
