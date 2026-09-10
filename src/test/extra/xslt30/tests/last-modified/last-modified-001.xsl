<?xml version="1.0"?> 
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="2.0"
xmlns:saxon="http://saxon.sf.net/" xmlns:my="http://my.net/"
exclude-result-prefixes="my saxon">

  <!-- PURPOSE test last-modified() with a file URI -->
  <xsl:output indent="yes"/>
  
  <xsl:template name="main">
    <out>
      <xsl:value-of select="saxon:last-modified('test.xml')"/>
    </out>
  </xsl:template>
 
</xsl:stylesheet>
