<?xml version="1.0"?> 
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="2.0"
    xmlns:m="http://saxon.sf.net/mode-one" xmlns:saxon="http://saxon.sf.net/"
    xmlns:xs="http://www.w3.org/2001/XMLSchema"
    exclude-result-prefixes="m saxon xs">

  <!-- PURPOSE test saxon:current-mode-name() function -->
  
  <xsl:strip-space elements="*"/>

  <xsl:template match="doc">
    <out>
        <xsl:apply-templates mode="m:one"/>
        <xsl:apply-templates mode="m:two"/>
    </out>
  </xsl:template>
  
  <xsl:template match="chap" mode="#all">
    <xsl:copy>
        <xsl:copy-of select="@*"/>
        <xsl:apply-templates mode="#current"/>
    </xsl:copy>
  </xsl:template>
  
  <xsl:template match="section" mode="m:one m:two">
    <xsl:if test="saxon:current-mode-name() eq xs:QName('m:one')">
        <xsl:copy><xsl:apply-templates/></xsl:copy>
    </xsl:if>
  </xsl:template>
  
  <xsl:output indent="yes"/>
 
</xsl:stylesheet>
