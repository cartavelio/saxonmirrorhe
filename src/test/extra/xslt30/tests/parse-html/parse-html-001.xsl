<?xml version="1.0"?> 
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="2.0"
xmlns:saxon="http://saxon.sf.net/" xmlns:my="http://my.net/"
exclude-result-prefixes="my saxon">

  <!-- PURPOSE test parse-html() with a literal argument -->
  
  <xsl:template name="main">
    <out>
      <xsl:variable name="html"><![CDATA[<html><body><P>Some text <br>second line</body></html>]]></xsl:variable>
      <xsl:variable name="e" select="saxon:parse-html($html)"/>
      <xsl:copy-of select="$e"/>
    </out>
  </xsl:template>
 
</xsl:stylesheet>
