<xsl:stylesheet version="2.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:f="http://example.com/threads010/ns" 
  xmlns:xs="http://www.w3.org/2001/XMLSchema" exclude-result-prefixes="xs f">

<!-- Test of saxon:threads on xsl:for-each executed in pull mode using position() and last() -->

<xsl:output indent="yes"/>

<xsl:function name="f:prices" as="element()*">
  <xsl:param name="doc"/>
  <xsl:for-each select="$doc//ITEM" saxon:threads="4" xmlns:saxon="http://saxon.sf.net/" exclude-result-prefixes="saxon">
    <a price="{PRICE}" position="{position()}" last="{last()}"/>
  </xsl:for-each>
</xsl:function>

<xsl:template match="/">
  <out>
    <xsl:copy-of select="f:prices(.)"/>
  </out>
</xsl:template>

</xsl:stylesheet>