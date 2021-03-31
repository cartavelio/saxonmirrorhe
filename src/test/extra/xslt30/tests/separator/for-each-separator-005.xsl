<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
   xmlns:xs="http://www.w3.org/2001/XMLSchema"
   exclude-result-prefixes="xs"
   version="4.0">
   
   <xsl:param name="input" as="xs:anyAtomicType*"/>
   <xsl:param name="sep" as="xs:string"/>
   
   <xsl:template name="xsl:initial-template">
      <out>
         <xsl:for-each select="$input" separator="{$sep}">
            <xsl:sequence select="."/>
         </xsl:for-each>
      </out>
   </xsl:template>
   
</xsl:stylesheet>