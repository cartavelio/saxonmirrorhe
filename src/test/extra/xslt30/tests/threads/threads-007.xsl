<xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

<!-- Test of saxon:threads on xsl:for-each with a local variable
     that will be evaluated lazily and incrementally from within the loop -->

<xsl:output indent="yes"/>
<xsl:strip-space elements="*"/>

<xsl:template match="/">
<out>
  <xsl:variable name="prices" select="//PRICE"/>
  <xsl:for-each select="//ITEM" saxon:threads="4" xmlns:saxon="http://saxon.sf.net/" exclude-result-prefixes="saxon">
    <xsl:variable name="pos" select="position()"/>
    <book title="{TITLE}" position="{$pos}" price="{$prices[$pos]}"/>      
  </xsl:for-each>
</out>
</xsl:template>



</xsl:stylesheet>