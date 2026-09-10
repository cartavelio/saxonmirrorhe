<?xml version="1.0"?> 

<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="2.0">

  <!-- test promotion of independent subexpressions within a for expression -->

  <xsl:param name="x" select="120"/>

  <xsl:template match="/">
    <out>
      <xsl:value-of select="for $s in //section return $s/@nr * round($x div 10)" 
                    separator=","/>
    </out>
  </xsl:template>

</xsl:stylesheet>