<?xml version="1.0"?> 

<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="2.0">

  <!-- test rewrite of count(x) = 0 etc -->

  <xsl:template match="/">
    <out>
      <xsl:value-of select="count(/doc/section) = 0"/>
      <xsl:value-of select="count(/doc/section) > 0"/>
      <xsl:value-of select="count(/doc/section) != 0"/>
    </out>
  </xsl:template>

</xsl:stylesheet>