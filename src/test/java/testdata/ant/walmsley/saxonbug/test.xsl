<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
   xmlns:xs="http://www.w3.org/2001/XMLSchema"
   exclude-result-prefixes="xs"
   version="2.0">
   <xsl:output method="text"/>
   <xsl:variable name="baseURI" select="base-uri(.)"/>
   <xsl:variable name="content" select="string(.)"/>
    <xsl:template match="/">
      <xsl:variable name="local-baseURI" select="base-uri(.)"/>
      <xsl:variable name="local-content" select="string(.)"/>
      <xsl:value-of select="$baseURI"/>,
      <xsl:value-of select="$content"/>,
      <xsl:value-of select="$local-baseURI"/>,
      <xsl:value-of select="$local-content"/>,
      <xsl:value-of select="base-uri(.)"/>,
      <xsl:value-of select="string(.)"/>
   </xsl:template>
</xsl:stylesheet>