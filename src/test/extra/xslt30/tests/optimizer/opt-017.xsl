<?xml version="1.0"?> 

<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="2.0">

  <!-- test rewrite of position() = 1 to n and similar expressions -->
  
  <xsl:param name="n" select="7" as="xs:integer" 
                      xmlns:xs="http://www.w3.org/2001/XMLSchema"/>
  
  <xsl:template match="/">
    <out>
      <xsl:value-of select="position() = 1 to $n"/>;
      <xsl:value-of select="('a', 'b', 'c', 'd', 'e', 'f', 'g', 'h')[position() = 3 to $n]"/>;      
      <xsl:value-of select="('a', 'b', 'c', 'd', 'e', 'f', 'g', 'h')[4 to $n = position()]"/>;                                      
    </out>
  </xsl:template>

</xsl:stylesheet>