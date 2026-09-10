<xsl:stylesheet version="2.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

<!-- Test of saxon:threads on xsl:for-each with trace() to demonstrate asynchronous operation.
     Order of messages is unpredictable but the messages themselves are atomic. -->

<xsl:output indent="yes"/>
<xsl:strip-space elements="*"/>



<xsl:template match="/">
<out>
  <xsl:variable name="prices" select="//PRICE"/>
  <xsl:for-each select="1 to 20" saxon:threads="4" xmlns:saxon="http://saxon.sf.net/" exclude-result-prefixes="saxon">
    <xsl:variable name="pos1" select="trace(position(), ' Enter ')"/>
    <xsl:variable name="p" select="sum(1 to $pos1*50)"/>
    <xsl:variable name="pos2" select="trace(position(), ' Exit ')"/>
    <e pos1="{$pos1}" pos2="{$pos1 + $pos2 - $pos1}"/>     
  </xsl:for-each>
</out>
</xsl:template>



</xsl:stylesheet>