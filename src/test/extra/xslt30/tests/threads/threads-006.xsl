<xsl:stylesheet version="2.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

<!-- Test of saxon:threads on xsl:for-each with a global variable
     that is first accessed from within the loop -->

<xsl:output indent="yes"/>
<xsl:strip-space elements="*"/>

<xsl:variable name="glob" select="doc('threads-001.xsl')//xsl:stylesheet/@version"/>

<xsl:template match="/">
<out>
  <xsl:variable name="cats" select="count(//CATEGORY)"/>
  <xsl:for-each select="//ITEM" saxon:threads="4" xmlns:saxon="http://saxon.sf.net/" exclude-result-prefixes="saxon">
    <xsl:variable name="price" select="PRICE"/>
    <book price="{$price}" position="{position()}" of="{last()}" version="{$glob}"/>      
  </xsl:for-each>
</out>
</xsl:template>



</xsl:stylesheet>