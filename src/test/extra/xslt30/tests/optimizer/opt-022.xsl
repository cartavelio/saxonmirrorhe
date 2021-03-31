<xsl:stylesheet version="2.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

<!-- optimization of an xsl:choose as a switch, using string values -->

 <xsl:param name="p" select="'banana'" as="xs:string" xmlns:xs="http://www.w3.org/2001/XMLSchema"/>

 <xsl:template match="/">
   <a>
     <xsl:choose>
     <xsl:when test="$p = 'apple'">A</xsl:when>
     <xsl:when test="$p = 'pear'">P</xsl:when>
     <xsl:when test="$p = 'orange'">O</xsl:when>
     <xsl:when test="$p = 'kiwi'">K</xsl:when>
     <xsl:when test="$p = 'tangerine'">T</xsl:when>
     <xsl:when test="$p = 'cherry'">C</xsl:when>
     <xsl:when test="$p = 'pear'">P (again)</xsl:when>
     <xsl:when test="$p eq 'plum'">M</xsl:when>
     <xsl:otherwise>name that fruit</xsl:otherwise>
     </xsl:choose>
   </a>
 </xsl:template>

</xsl:stylesheet>
