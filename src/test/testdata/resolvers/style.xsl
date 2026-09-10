<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:xs="http://www.w3.org/2001/XMLSchema"
                xmlns:h="http://www.w3.org/1999/xhtml"
                xmlns="http://www.w3.org/1999/xhtml"
                exclude-result-prefixes="#all"
                version="1.0">

<xsl:mode on-no-match="shallow-copy"/>

<xsl:template match="pre[@href]">
  <pre>
    <xsl:sequence select="unparsed-text('http://example.com/text')"/>
  </pre>
</xsl:template>

<xsl:template match="*:head">
  <xsl:copy>
    <xsl:apply-templates select="@*,node()"/>
    <meta name="stylesheet" content="resolved"/>
  </xsl:copy>
</xsl:template>  

</xsl:stylesheet>
