<?xml version="1.0"?> 

<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="3.0"
  xmlns:f="http://localhost/functions"
  exclude-result-prefixes="f">

  <!-- very basic test of serial copy -->
  
  <xsl:template name="main">
    <out>
      <xsl:value-of select="count(f:customers())"/>
    </out>
  </xsl:template>
  
  <xsl:function name="f:customers">
    <xsl:sequence select="saxon:stream(doc('xserial001.xml')/*/customer)" xmlns:saxon="http://saxon.sf.net/"/>
  </xsl:function>

</xsl:stylesheet>