<?xml version="1.0"?> 

<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="2.0"
  xmlns:f="http://localhost/functions"
  exclude-result-prefixes="f">

  <!-- show that dynamic errors are propagated down the pipeline -->
  
  <xsl:template name="main">
    <out>
      <xsl:value-of select="count(f:customers('file:+++ill formed URI'))"/>
    </out>
  </xsl:template>
  <xsl:template name="not-found">
    <out>
      <xsl:value-of select="count(f:customers('nonexistent.xml'))"/>
    </out>
  </xsl:template>
  
  <xsl:function name="f:customers">
    <xsl:param name="file"/>
    <xsl:sequence select="saxon:stream(doc($file)/*/customer)"
     xmlns:saxon="http://saxon.sf.net/"/>
  </xsl:function>
  
</xsl:stylesheet>