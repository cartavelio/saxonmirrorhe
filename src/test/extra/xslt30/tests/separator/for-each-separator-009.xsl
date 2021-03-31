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
         <xsl:for-each select="$data" separator="{$sep}">
            <xsl:variable name="d" select="."/>
            <xsl:for-each select="1 to (position()-1)">
               <xsl:sequence select="$d"/>
            </xsl:for-each>
         </xsl:for-each>
      </out>
   </xsl:template>
   
</xsl:stylesheet>