<xsl:transform xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="2.0">

 <xsl:template match="/">
   <out><xsl:copy-of select="doc('http://example.com/example.xml')"/></out>
 </xsl:template>

</xsl:transform> 