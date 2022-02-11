<?xml version="1.0"?> 

<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="3.0"
  xmlns:f="http://localhost/functions"
  xmlns:xs="http://www.w3.org/2001/XMLSchema"
  exclude-result-prefixes="f xs">

  <!-- streaming copy with a run-time error in the consuming thread -->
  
  <xsl:param name="p" select="0" as="xs:integer"/>
  
  <xsl:template name="main">
    <out>
      <xsl:value-of select="count(f:customers()[count(@a) div $p = 0])"/>
    </out>
  </xsl:template>
  
  <xsl:function name="f:customers">
    <xsl:sequence select="saxon:stream(doc('xserial002.xml')/*/customer)" 
                          xmlns:saxon="http://saxon.sf.net/"/>
  </xsl:function>

</xsl:stylesheet>