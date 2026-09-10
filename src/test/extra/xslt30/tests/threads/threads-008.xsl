<xsl:stylesheet version="2.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

<!-- Test of saxon:threads on xsl:for-each with xsl:message to demonstrate asynchronous operation.
     Order of messages is unpredictable but the messages themselves are atomic. -->

<xsl:output indent="yes"/>
<xsl:strip-space elements="*"/>

<xsl:variable name="glob" select="doc('threads001.xsl')//xsl:stylesheet/@version"/>

<xsl:template match="/">
<out>
  <xsl:variable name="prices" select="//PRICE"/>
  <xsl:for-each select="1 to 20" saxon:threads="4" xmlns:saxon="http://saxon.sf.net/" exclude-result-prefixes="saxon">
    <xsl:variable name="pos" select="position()"/>
    <xsl:message select="position()"> Enter </xsl:message>
    <xsl:variable name="p" select="sum(1 to position()*50)"/>
    <xsl:message select="$p + position() - $p"> Exit </xsl:message>
    <e/>     
  </xsl:for-each>
</out>
</xsl:template>



</xsl:stylesheet>