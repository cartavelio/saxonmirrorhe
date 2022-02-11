<?xml version="1.0"?> 

<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="2.0"
  xmlns:f="http://localhost/functions"
  exclude-result-prefixes="f">

  <!-- attempted streaming copy with nodes other than elements and attributes.
       Works, but without serial copy. -->
  
  <xsl:template name="main">
    <out>
      <xsl:variable name="copies" as="node()*">
       <xsl:sequence select="saxon:stream(doc('xserial003.xml')/*/customer/(@*|node()))" 
                          xmlns:saxon="http://saxon.sf.net/"/>
      </xsl:variable>
      <xsl:value-of select="count($copies)"/>                   
    </out>
  </xsl:template>
  

</xsl:stylesheet>