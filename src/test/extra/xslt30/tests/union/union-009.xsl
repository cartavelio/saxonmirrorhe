<?xml version="1.0" encoding="utf-8"?> 

<xsl:stylesheet
   xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="4.0"
   xmlns:xs="http://www.w3.org/2001/XMLSchema"
   xmlns:saxon="http://saxon.sf.net/"
   xmlns:cx="http://saxon.sf.net/complex"
   exclude-result-prefixes="xs saxon cx"
   expand-text="yes"
>

  <!-- Default priorities for atomic and union patterns -->
  
  <xsl:template match="union(xs:integer, xs:double, xs:float)">
    <unionIDF><xsl:next-match/></unionIDF>
  </xsl:template>
  
  <xsl:template match="atomic(xs:integer)">
    <integer><xsl:next-match/></integer>
  </xsl:template>
  
  <xsl:template match="union(xs:numeric)">
    <numeric><xsl:next-match/></numeric>
  </xsl:template>
  
  <xsl:template match="atomic(xs:int)">
    <should-not-match><xsl:next-match/></should-not-match>
  </xsl:template>
  
  <xsl:template match="union(xs:long, xs:date)">
    <unionLD><xsl:next-match/></unionLD>
  </xsl:template>
  
  <xsl:template match="union(xs:integer, xs:date)">
    <unionID><xsl:next-match/></unionID>
  </xsl:template>
  
  <xsl:template match="atomic(xs:long)">
    <long><xsl:next-match/></long>
  </xsl:template>
  
  <xsl:template match="atomic(xs:decimal)">
    <decimal><xsl:next-match/></decimal>
  </xsl:template>
  
  <xsl:template name="xsl:initial-template">
    <out>
      <xsl:apply-templates select="xs:long(1234)"/>
    </out>
  </xsl:template>  

</xsl:stylesheet>
