<?xml version="1.0"?> 

<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="3.0"
  xmlns:f="http://localhost/functions"
  exclude-result-prefixes="f">

  <!-- very basic test of serial copy with a largish source document -->
  
  <xsl:template name="main">
    <out>
      <xsl:value-of select="count(f:verses())"/>
    </out>
  </xsl:template>
  
  <xsl:function name="f:verses">
    <xsl:sequence select="saxon:stream(doc('../../docs/ot.xml')//v)" xmlns:saxon="http://saxon.sf.net/"/>
  </xsl:function>

</xsl:stylesheet>