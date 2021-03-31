<?xml version="1.0"?> 

<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="2.0"
  xmlns:f="http://localhost/functions"
  exclude-result-prefixes="f">

  <!-- streaming copy with a two-predicate filter expression -->
  
  <xsl:template name="main">
    <out>
      <xsl:value-of select="count(f:customers())"/>
    </out>
  </xsl:template>
  
  <xsl:function name="f:customers">
    <xsl:sequence select="saxon:stream(doc('xserial002.xml')/*/customer)[number(.) > 4][not(comment())]" 
                          xmlns:saxon="http://saxon.sf.net/"/>
  </xsl:function>

</xsl:stylesheet>