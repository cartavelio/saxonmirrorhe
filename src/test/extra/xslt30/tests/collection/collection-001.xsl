<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="3.0"
  xmlns:xs="http://www.w3.org/2001/XMLSchema"
  exclude-result-prefixes="xs">

<!-- test of collection catalog mechanism -->
  
  <xsl:strip-space elements="*"/>

<xsl:template name="main">
  <xsl:variable name="c" select="collection('collection-catalog-001.xml')"/>
  <out white="{$c//white}">
    <named><xsl:copy-of select="$c/*/name()"/></named>
    <local><xsl:copy-of select="$c//*  (:[@id='b1']:)"/></local>
  </out>     
</xsl:template>

</xsl:stylesheet>
  