<xsl:stylesheet version="2.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

<!-- optimization of an xsl:choose as a switch, using numeric values -->

 <xsl:param name="p" select="0" as="xs:integer" xmlns:xs="http://www.w3.org/2001/XMLSchema"/>

 <xsl:template match="/" >
   <a>
     <xsl:choose>
     <xsl:when test="$p = 0">zero</xsl:when>
     <xsl:when test="$p = 80">eighty</xsl:when>
     <xsl:when test="$p = -4">minus four</xsl:when>
     <xsl:when test="$p = 12">twelve</xsl:when>
     <xsl:when test="$p = 93">ninety-three</xsl:when>
     <xsl:when test="$p = 5">five</xsl:when>
     <xsl:when test="$p = 80">eighty (again)</xsl:when>
     <xsl:when test="$p eq 20">twenty</xsl:when>
     <xsl:otherwise>default</xsl:otherwise>
     </xsl:choose>
   </a>
 </xsl:template>

</xsl:stylesheet>
