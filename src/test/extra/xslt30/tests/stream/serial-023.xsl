<?xml version="1.0"?> 

<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="3.0"
  xmlns:f="http://localhost/functions"
  exclude-result-prefixes="f">

  <!-- From 9.6, saxon:stream delivers snapshots rather than copies -->
  
  <xsl:template name="main">
    <out>
      <xsl:value-of select="avg(f:customers()/count(ancestor::node()))"/>
    </out>
  </xsl:template>
  
  <xsl:function name="f:customers">
    <xsl:sequence select="saxon:stream(doc('xserial003.xml')//customer)" xmlns:saxon="http://saxon.sf.net/"/>
  </xsl:function>

</xsl:stylesheet>