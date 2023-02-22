<xsl:stylesheet version="2.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

<!-- Basic test of saxon:threads on xsl:for-each -->

<xsl:output indent="yes"/>

<xsl:template match="/">
<out>
  <xsl:for-each select="//ITEM" saxon:threads="4" xmlns:saxon="http://saxon.sf.net/" exclude-result-prefixes="saxon">
    <xsl:sequence select="10 div (position()-1)"/>
  </xsl:for-each>
</out>
</xsl:template>

</xsl:stylesheet>