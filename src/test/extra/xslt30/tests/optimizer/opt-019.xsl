<xsl:stylesheet version="2.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

<!-- shouldn't cause a divide by zero error -->

<xsl:variable name="p" select="0"/>

 <xsl:template match="/">
   <a>

     <xsl:choose>
     <xsl:when test="$p = 0">0</xsl:when>
     <xsl:otherwise><xsl:value-of select="1 div $p"/></xsl:otherwise>
     </xsl:choose>

   </a>
 </xsl:template>

</xsl:stylesheet>
