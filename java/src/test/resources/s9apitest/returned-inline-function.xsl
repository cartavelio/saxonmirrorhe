<xsl:stylesheet version="3.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:param name="value"/>
  <xsl:param name="bad" select="error()"/>

  <xsl:template name="main">
    <xsl:sequence select="function($useBad) { if ($useBad) then $bad else $value }"/>
  </xsl:template>
</xsl:stylesheet>
