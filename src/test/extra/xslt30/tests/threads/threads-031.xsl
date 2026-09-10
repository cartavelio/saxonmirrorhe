<xsl:stylesheet version="2.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

<!-- Test of saxon:threads on xsl:for-each with dynamic error in select expression -->

<xsl:output indent="yes"/>

<xsl:template match="/">
<out>
  <xsl:for-each select="(//node())[10 div (position()-1) gt 0]" saxon:threads="16" xmlns:saxon="http://saxon.sf.net/" exclude-result-prefixes="saxon">
    <xsl:sequence select="TITLE"/>
  </xsl:for-each>
</out>
</xsl:template>

</xsl:stylesheet>