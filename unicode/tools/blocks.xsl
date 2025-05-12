<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:xs="http://www.w3.org/2001/XMLSchema"
                exclude-result-prefixes="xs"
                expand-text="yes"
                version="3.0">
<xsl:output method="xml" encoding="utf-8" indent="yes"/>

<xsl:param name="unicodeVersion" required="yes" as="xs:string"/>
<xsl:variable name="blocks.txt" select="'../build/cache/Blocks-' || $unicodeVersion || '.txt'"/>

<xsl:template match="/" name="xsl:initial-template">
  <xsl:variable name="blocks" select="unparsed-text($blocks.txt)"/>
  <xsl:comment> See http://www.w3.org/TR/xsd-unicode-blocknames/ </xsl:comment>
  <xsl:comment> Generated from UCD files from Unicode version {$unicodeVersion} </xsl:comment>
  <blocks>
    <xsl:for-each select="tokenize($blocks, '&#10;')">
      <xsl:variable name="line" select="normalize-space(.)"/>
      <xsl:if test="matches($line, '^[0-9A-Fa-f]+\.\.[0-9A-Fa-f]+; .*$')">
        <xsl:variable name="from" select="substring-before($line, '..')"/>
        <xsl:variable name="to" select="substring-after($line, '..') =&gt; substring-before(';')"/>
        <xsl:variable name="name" select="substring-after($line, ';') =&gt; normalize-space()"/>
        <block name="{$name}">
          <range from="#x{$from}" to="#x{$to}"/>
        </block>
      </xsl:if>
    </xsl:for-each>
  </blocks>
</xsl:template>

</xsl:stylesheet>
