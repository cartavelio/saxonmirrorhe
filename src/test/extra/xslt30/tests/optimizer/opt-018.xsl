<xsl:stylesheet version="2.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

<!-- shouldn't cause a divide by zero error -->

<xsl:param name="p" select="0"/>

 <xsl:template match="/">
   <a>
   <xsl:for-each select="nonexistent">
     <xsl:value-of select="1 div $p"/>
   </xsl:for-each>
   </a>
 </xsl:template>

</xsl:stylesheet>
