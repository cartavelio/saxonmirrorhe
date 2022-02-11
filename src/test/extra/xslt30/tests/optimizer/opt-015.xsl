<?xml version="1.0"?> 

<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">

  <!-- test rewrite of count($x)=0 and similar expressions (1.0 mode) -->

  <xsl:template match="/">
    <out>
      <xsl:value-of select="count(//para) = 0"/>
      <xsl:value-of select="0 &lt; count(//para)"/>      
      <xsl:value-of select="count(//para[last()]) != 0"/>                                       
    </out>
  </xsl:template>

</xsl:stylesheet>