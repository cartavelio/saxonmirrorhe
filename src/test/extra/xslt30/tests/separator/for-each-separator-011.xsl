<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
   xmlns:xs="http://www.w3.org/2001/XMLSchema"
   xmlns:f="http://separator/"
   exclude-result-prefixes="#all"
   version="4.0">
   
   <!-- for-each with separator="" inhibits space separation -->

   <xsl:template name="xsl:initial-template">
      <out>
         <xsl:for-each select="1 to 10" separator="">
            <xsl:sequence select=". - 1"/>
         </xsl:for-each>
      </out>
   </xsl:template>
   
 
   
</xsl:stylesheet>