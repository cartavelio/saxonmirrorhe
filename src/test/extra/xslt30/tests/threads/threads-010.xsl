<xsl:stylesheet version="2.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:f="http://example.com/threads010/ns" 
  xmlns:xs="http://www.w3.org/2001/XMLSchema" exclude-result-prefixes="xs f">

<!-- Test of saxon:threads on xsl:for-each executed in pull mode -->

<xsl:output indent="yes"/>

<xsl:function name="f:prices" as="xs:double*">
  <xsl:param name="doc"/>
  <xsl:for-each select="$doc//ITEM" saxon:threads="4" xmlns:saxon="http://saxon.sf.net/" exclude-result-prefixes="saxon">
    <xsl:sequence select="number(PRICE)"/>
  </xsl:for-each>
</xsl:function>

<xsl:template match="/">
  <out>
    <xsl:value-of select="round-half-to-even(avg(f:prices(.)),2)"/>
  </out>
</xsl:template>

</xsl:stylesheet>