<?xml version="1.0"?> 

<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="2.0">

  <!-- test promotion of independent subexpressions within the condition of an if expression -->

 <xsl:param name="x" select="120"/>

  <xsl:template match="/">
    <out>
      <xsl:copy-of select="for $s in //section return 
                            if ($x = 120) then $s else ()" />
    </out>
  </xsl:template>

</xsl:stylesheet>