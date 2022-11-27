<xsl:stylesheet version="2.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:import href="CA2.xsl"/>
  
  <xsl:template match="/">
    <root>
      <xsl:call-template name="ca"  />
      <out>test</out>
      
    </root>
  </xsl:template>
</xsl:stylesheet>