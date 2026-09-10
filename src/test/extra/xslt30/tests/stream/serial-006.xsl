<?xml version="1.0"?> 

<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="2.0"
  xmlns:f="http://localhost/functions"
  exclude-result-prefixes="f">

  <!-- test serial copy using document() function and multiple documents -->
  <!-- also tests taking the base URI from the node supplied as argument to document() -->
  
  <xsl:variable name="docs">
    <doc>xserial001.xml</doc>
    <doc>xserial002.xml</doc>
  </xsl:variable>  
  
  <xsl:template name="main">
    <out>
      <xsl:value-of select="count(f:customers())"/>
    </out>
  </xsl:template>
  
  <xsl:function name="f:customers">
    <xsl:sequence select="saxon:stream(document($docs/doc)//customer)" 
                          xmlns:saxon="http://saxon.sf.net/"/>
  </xsl:function>

</xsl:stylesheet>