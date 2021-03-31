<xsl:transform xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
 xmlns:saxon="http://saxon.sf.net/"
 version="2.0">
 
 <!-- Basic test of saxon:evaluate-node() -->
 <!-- Formerly saxon013 -->
 
<xsl:template match="doc">
<out><xsl:apply-templates select="exp"/></out>
</xsl:template>

<xsl:template match="exp">
  <ans><xsl:value-of select="saxon:evaluate-node(.)"/></ans>
</xsl:template>

</xsl:transform>