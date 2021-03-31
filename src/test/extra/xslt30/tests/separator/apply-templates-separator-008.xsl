<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
   xmlns:xs="http://www.w3.org/2001/XMLSchema"
   exclude-result-prefixes="xs"
   version="4.0">
   
   <xsl:param name="data" as="element(*)*">
      <one/>
      <two/>
      <three/>
      <four/>
   </xsl:param>
   <xsl:param name="sep" as="xs:string"/>
   
   <xsl:template name="xsl:initial-template">
      <out>
         <xsl:apply-templates select="$data" separator="{$sep}"/>
      </out>
   </xsl:template>
   
   
   <xsl:template match="*">
      <xsl:sequence select="."/>
   </xsl:template>
   
</xsl:stylesheet>