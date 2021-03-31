<?xml version="1.0"?> 

<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="2.0">

  <!-- test promotion of independent subexpressions on r.h.s of a path expression -->

 <xsl:param name="x" as="xs:integer" select="120" xmlns:xs="http://www.w3.org/2001/XMLSchema"/>

  <xsl:template match="/">
    <out>
      <xsl:copy-of select="//section/
                            (if ($x = 120) then @nr else ())"/>
    </out>
  </xsl:template>

</xsl:stylesheet>