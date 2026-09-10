<?xml version="1.0"?> 

<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">

  <!-- test rewrite of [last()] and [position()=last()] -->

  <xsl:template match="/">
    <out>
      <xsl:copy-of select="//para[last()]"/>
      <xsl:copy-of select="//para[position()=last()]"/>      
      <xsl:copy-of select="//para[last()=position()]"/>                                         
    </out>
  </xsl:template>

</xsl:stylesheet>