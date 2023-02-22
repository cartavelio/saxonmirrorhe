<xsl:stylesheet version="2.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:xs="http://www.w3.org/2001/XMLSchema">
  
  <!-- invalid xsl:sequence; but the originally published schema permitted this -->
  <xsl:template match="/">
    <xsl:sequence select="93.7" as="xs:integer"/>
  </xsl:template>
</xsl:stylesheet>