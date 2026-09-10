<?xml version="1.0" encoding="UTF-8"?>

<!-- call format-number within saxon:evaluate() -->

<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
 xmlns:saxon="http://saxon.sf.net/" version="2.0">
  <xsl:output method="xml" encoding="UTF-8"/>
  <xsl:param name="exp">format-number($p1, '0.00')</xsl:param>
  <xsl:template match="/" name="main">
    <Result>      
      <xsl:value-of select="saxon:evaluate($exp, 93.721)"/>
    </Result>
  </xsl:template>
</xsl:stylesheet> 
