<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:xs="http://www.w3.org/2001/XMLSchema"
                xmlns:test="https://saxonica.com/iss5701"
                exclude-result-prefixes="#all"
                version="3.0">

<xsl:output method="xml" encoding="utf-8" indent="no"/>

<xsl:mode on-no-match="shallow-copy"/>

<xsl:template match="/" name="xsl:initial-template">
  <xsl:apply-templates/>
</xsl:template>

<xsl:template match="p">
  <xsl:variable name="func"
                select="function-lookup(xs:QName('test:hello'), 0)"/>
  <xsl:copy>
    <xsl:choose>
      <xsl:when test="exists($func)">
        <xsl:sequence select="$func()"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:apply-templates/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:copy>
</xsl:template>

</xsl:stylesheet>
