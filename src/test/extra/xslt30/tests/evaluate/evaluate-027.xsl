<?xml version="1.0" encoding="UTF-8"?>

<!-- call an overloaded extension function within saxon:evaluate() -->
<!-- bug 2859861 -->

<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
xmlns:Math="java:java.lang.Math" xmlns:saxon="http://saxon.sf.net/" version="2.0">
  <xsl:output method="xml" encoding="UTF-8"/>
  <xsl:template match="/" name="main">
    <Result>
      <xsl:value-of select="saxon:evaluate('Math:abs(-1)')"/>
    </Result>
  </xsl:template>
</xsl:stylesheet> 
