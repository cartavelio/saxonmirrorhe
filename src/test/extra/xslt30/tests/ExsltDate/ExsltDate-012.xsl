<?xml version="1.0"?> 
<xsl:stylesheet version="1.0"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:date="http://exslt.org/dates-and-times"
                extension-element-prefixes="date">

<!-- Test date:duration() (duration in seconds) -->

<xsl:variable name="positiveData">
  <a1>P1Y</a1>
  <a2>P2Y3M</a2>
  <a3>P5DT6H</a3>
  <a4>PT12H30M50.2S</a4>
  <a5>PT0.04S</a5>
</xsl:variable>

<xsl:variable name="negativeData">
  <a1>-P1Y</a1>
  <a2>-P2Y3M</a2>
  <a3>-P5DT6H</a3>
  <a4>-PT12H30M50.2S</a4>
  <a5>-PT0.04S</a5>
</xsl:variable>

<xsl:template match="/">
<out>
<a><xsl:value-of select="date:duration(900000)"/></a>
<a><xsl:value-of select="date:duration(-7200)"/></a>
<a><xsl:value-of select="date:duration(0)"/></a>
<a><xsl:value-of select="date:duration(0.0001)"/></a>
</out>

</xsl:template>
</xsl:stylesheet>
