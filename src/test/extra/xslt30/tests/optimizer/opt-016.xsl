<?xml version="1.0"?> 

<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="2.0"
xmlns:xs="http://www.w3.org/2001/XMLSchema" exclude-result-prefixes="xs">

   <xsl:param name="i" as="xs:integer" select="3"/>

  <!-- test rewrite of position() = 1 to n and similar expressions -->

  <xsl:template match="/">
    <out>
      <xsl:value-of select="position() = $i to 5"/>;
      <xsl:value-of select="('a', 'b', 'c', 'd', 'e', 'f', 'g', 'h')[position() = $i to 7]"/>;      
      <xsl:value-of select="('a', 'b', 'c', 'd', 'e', 'f', 'g', 'h')[$i to 7 = position()]"/>;                                      
    </out>
  </xsl:template>

</xsl:stylesheet>