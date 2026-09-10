<?xml version="1.0"?> 

<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0"
xmlns:saxon="http://saxon.sf.net/" >

<!-- Test saxon:evaluate. Formerly test saxon014. -->


<xsl:template match="/">
  <out>
    <xsl:copy-of select="saxon:evaluate(//selected/@city)"/>
  </out>
</xsl:template>
 
</xsl:stylesheet>
