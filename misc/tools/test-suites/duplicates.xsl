<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:xs="http://www.w3.org/2001/XMLSchema"
                xmlns:t="http://www.w3.org/2010/09/qt-fots-catalog"
                exclude-result-prefixes="#all"
                version="3.0">

<xsl:output method="text" encoding="utf-8" indent="no"/>

<xsl:mode on-no-match="shallow-copy"/>

<xsl:template name="xsl:initial-template">
  <xsl:variable name="root" select="/"/>
  <xsl:if test="/t:test-set|/t:test-case">
    <xsl:variable name="names" select="distinct-values(//t:test-case/@name/string())"/>
    <xsl:variable name="dups" as="xs:string*">
      <xsl:for-each select="$names">
        <xsl:variable name="name" select="."/>
        <xsl:if test="count($root//t:test-case[@name=$name]) gt 1">
          <xsl:sequence select="$name"/>
        </xsl:if>
      </xsl:for-each>
    </xsl:variable>
    <xsl:if test="exists($dups)">
      <xsl:sequence select="base-uri($root), ':', $dups, '&#10;'"/>
    </xsl:if>
  </xsl:if>
</xsl:template>

</xsl:stylesheet>
