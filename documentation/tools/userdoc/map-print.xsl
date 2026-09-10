<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:map="http://www.w3.org/2005/xpath-functions/map"
  xmlns:X="IndirectXSLT" exclude-result-prefixes="xs" version="3.0">
  <xsl:template name="map-print">
    <xsl:param name="items" as="item()*"/>
    <xsl:for-each select="$items" xmlns="IndirectXSLT">
      <xsl:choose>
        <xsl:when test=". instance of map(*)">
          <map>
            <xsl:variable name="this" select="."/>
            <xsl:for-each select="map:keys($this)">
              <xsl:sort select="."/>
              <map-entry key="'{.}'">
                <xsl:call-template name="map-print">
                  <xsl:with-param name="items" select="$this(.)"/>
                </xsl:call-template>
              </map-entry>
            </xsl:for-each>
          </map>
        </xsl:when>
        <xsl:when test=". instance of element()">
          <xsl:copy-of select="."/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="."/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:for-each>
  </xsl:template>
  
  <xsl:variable name="map-joiner">,<xsl:text>
  </xsl:text></xsl:variable>
  <xsl:variable name="quot">'</xsl:variable>
  <xsl:variable name="quoter" select="function($s) {string-join(($quot,replace($s,$quot,$quot||$quot),$quot))}"/>
  <xsl:template name="map-print-XPath">
    <xsl:param name="items" as="item()*"/>
    <xsl:for-each select="$items" xmlns="IndirectXSLT">
      <xsl:choose>
        <xsl:when test=". instance of map(*)">
          <xsl:variable name="this" select="."/>
          <xsl:variable name="entries" as="xs:string*">
            <xsl:for-each select="map:keys($this)">
              <xsl:sort select="."/>
              <xsl:variable name="key" as="xs:string">
                <xsl:choose>
                  <xsl:when test=". instance of xs:string">
                    <xsl:value-of select="''''||.||''''"/>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:value-of select="."/>
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:variable>
              <xsl:variable name="value" as="xs:string">
                <xsl:variable name="parts" as="xs:string*">
                  <xsl:call-template name="map-print-XPath">
                    <xsl:with-param name="items" select="$this(.)"/>
                  </xsl:call-template>
                </xsl:variable>
                <xsl:value-of
                  select="if(count($parts) ne 1) then '('||string-join($parts,',')||')' else $parts"
                />
              </xsl:variable>
              <xsl:value-of select="$key,':=',$value"/>
            </xsl:for-each>
          </xsl:variable>
          <xsl:value-of select="'map {', string-join($entries,$map-joiner),'}'"/>
        </xsl:when>
        <xsl:when test=". instance of element()">
          <xsl:copy-of select="."/>
        </xsl:when>
        <xsl:when test=". instance of xs:string">
          <xsl:value-of select="$quoter(.)"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="."/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:for-each>
  </xsl:template>
</xsl:stylesheet>
