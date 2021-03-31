<?xml version="1.0"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                version="1.0">

  <xsl:output method="text"/>

  <xsl:preserve-space elements="*"/>

  <xsl:template match="*">
    <xsl:choose>
      <xsl:when test="not(@role)"/>
      <xsl:when test="@role='xquery'">
        <xsl:message><xsl:value-of select="local-name(.)"/></xsl:message>
        <xsl:if test="ancestor::*[@role='xpath']">WARNING: XPath in XQuery</xsl:if>
      </xsl:when>
      <xsl:when test="@role='xpath'">
        <xsl:message><xsl:value-of select="local-name(.)"/></xsl:message>
        <xsl:if test="ancestor::*[@role='xquery']">WARNING: XQuery in XPath</xsl:if>
      </xsl:when>
      <xsl:when test="@role='lexical-state-tables'
                      or @role='spec-language'
                      or @role='spec-conditional'
                      or @role='spec-version'
                      or @role='small'
                      or @role='non-terminal-structure-expand'
                      or @role='frag-prolog-parse-test'
                      or @role='parse-test'
                      or @role='wont-parse-test'
                      or @role='bug-parse-test'">
        <!-- nop -->
      </xsl:when>
      <xsl:otherwise>
        <xsl:message>Unexpected role: <xsl:value-of select="@role"/></xsl:message>
      </xsl:otherwise>
    </xsl:choose>
    <xsl:apply-templates/>
  </xsl:template>

  <xsl:template match="comment()|processing-instruction()|text()"/>

</xsl:stylesheet>
