<xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output method="text" encoding="UTF-8"/>
  <xsl:template match="/">
    <xsl:call-template name="label">
      <xsl:with-param name="value" select="string(/record/@name)"/>
    </xsl:call-template>
  </xsl:template>
  <xsl:template name="label">
    <xsl:param name="value" required="yes"/>
    <xsl:value-of select="concat('[', upper-case($value), ']')"/>
  </xsl:template>
</xsl:stylesheet>
