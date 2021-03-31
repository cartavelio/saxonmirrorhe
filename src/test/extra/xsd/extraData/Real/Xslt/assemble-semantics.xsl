<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	xmlns:g="http://www.w3.org/2001/03/XPath/grammar"
		version="1.0">

  <xsl:import href="assemble-spec.xsl"/>

  <!-- get the doctype right -->
  <xsl:output method="xml" indent="no"
              doctype-system="../xquery-semantics.dtd" />
		
  <xsl:template match="prodrecap[@id='XQueryDefinedLexemesForXML']">
    <xsl:variable name="fn"><xsl:call-template name="get-gfn"/></xsl:variable>
    <xsl:variable name="grammar" select="document($fn)"/>
    <xsl:variable name="orig">
      <xsl:call-template name="get-gnotation"/>
    </xsl:variable>
    <xsl:variable name="name-prefix">
      <xsl:text>prod-</xsl:text>
      <xsl:if test="$orig">
        <xsl:value-of select="$grammar/g:grammar/g:language/@id"/>
        <xsl:text>-</xsl:text>
      </xsl:if>
    </xsl:variable>
    <xsl:for-each select="$grammar/g:grammar">
      <xsl:call-template name="add-terminals">
        <xsl:with-param name="orig" select="$orig"/>
        <xsl:with-param name="name-prefix" select="$name-prefix"/>
		<xsl:with-param name="xml-only" select="true()"/>
      </xsl:call-template>
    </xsl:for-each>
  </xsl:template>

</xsl:stylesheet>
