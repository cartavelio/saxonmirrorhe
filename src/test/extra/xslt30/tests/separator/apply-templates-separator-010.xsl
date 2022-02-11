<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
   xmlns:xs="http://www.w3.org/2001/XMLSchema"
   xmlns:f="http://separator/"
   exclude-result-prefixes="#all"
   version="4.0">
   
   <!-- for-each with separator in pull mode -->
   
   <xsl:param name="data" as="element(*)*">
      <one/>
      <two/>
      <three/>
      <four/>
   </xsl:param>
   <xsl:param name="sep" as="xs:string"/>
   
   <xsl:template name="xsl:initial-template">
      <out>
         <xsl:apply-templates select="f:separated-items(1 to 4, '-')"/>
      </out>
   </xsl:template>
   
   <xsl:template match=".">
      <in><xsl:sequence select="."/></in>
   </xsl:template>
   
   <xsl:function name="f:separated-items">
      <xsl:param name="in" as="item()*"/>
      <xsl:param name="sep" as="xs:string"/>
      <xsl:for-each select="$in" separator="{$sep}">
         <xsl:sequence select="."/>
      </xsl:for-each>
   </xsl:function>
   
</xsl:stylesheet>