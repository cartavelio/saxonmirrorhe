<?xml version="1.0"?>
<!-- test saxon:evaluate calling an extension function -->
<!-- formerly test saxon037 -->

<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
xmlns:saxon="http://saxon.sf.net/" extension-element-prefixes="saxon">

<xsl:param name="exp" select="'saxon:system-id()'"/>

<xsl:template match="/">
<out>
   <xsl:choose>
   <xsl:when test="saxon:evaluate($exp) = saxon:system-id()">OK</xsl:when>
   <xsl:otherwise>TEST FAILED: <xsl:value-of select="saxon:system-id()"/></xsl:otherwise>
   </xsl:choose>
</out>
</xsl:template>

</xsl:stylesheet>

