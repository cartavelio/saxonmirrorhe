<?xml version="1.0"?> 
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="2.0"
xmlns:saxon="http://saxon.sf.net/" xmlns:my="http://my.net/"
exclude-result-prefixes="my saxon">

  <!-- PURPOSE test expression() with a literal argument calling a stylesheet function -->

  
  <xsl:template name="main">
    <out>
      <xsl:variable name="e" select="saxon:expression('my:f()')"/>
      <xsl:value-of select="saxon:eval($e)"/>
    </out>
  </xsl:template>
  
  <xsl:output indent="yes"/>
  
  <xsl:function name="my:f" visibility="public">
    <xsl:sequence select="$p"/>
  </xsl:function>
  
  <xsl:param name="p" select="42"/>
 
</xsl:stylesheet>
