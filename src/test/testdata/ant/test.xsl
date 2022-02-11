<xsl:transform xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">

 <xsl:template match="*">
   <xsl:copy><xsl:copy-of select="@*"/><xsl:apply-templates/></xsl:copy>
 </xsl:template>
 
 <xsl:template match="title">
   <xsl:value-of select="'((', ., '))'"/>
 </xsl:template>
 
</xsl:transform> 