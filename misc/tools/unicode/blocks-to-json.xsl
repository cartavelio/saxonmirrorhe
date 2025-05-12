<?xml version="1.0" encoding="UTF-8"?>
<!-- Converts unicodeBlocks.xml and categories.xml to JSON format -->
<!-- Principal source document is unicodeBlocks.xml -->
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:xs="http://www.w3.org/2001/XMLSchema"
    exclude-result-prefixes="xs"
    version="3.0"
    expand-text="yes">
    <xsl:output method="text"/>
    <xsl:strip-space elements="*"/>
    <xsl:template match="/">
        <xsl:text>{{</xsl:text>
        <xsl:apply-templates/>
        <xsl:apply-templates select="document('categories.xml', /)/*"/>
        <xsl:text>}}</xsl:text>
    </xsl:template>
    <xsl:template match="block">
        <xsl:if test="position() > 1">,</xsl:if>
        <xsl:value-of>&#xa;'Is{translate(@name, ' _', '')}':[</xsl:value-of>
        <xsl:apply-templates/>
        <xsl:text>]</xsl:text>
    </xsl:template>
    <xsl:template match="block/range">
        <xsl:if test="position() > 1">,</xsl:if>
        <xsl:value-of>['{substring(@from, 3)}','{substring(@to, 3)}']</xsl:value-of>
    </xsl:template>
    <xsl:template match="cat">
        <xsl:value-of>,&#xa;'{@name}':[</xsl:value-of>
        <xsl:apply-templates/>
        <xsl:text>]</xsl:text>
    </xsl:template>
    <xsl:template match="cat/range">
        <xsl:if test="position() > 1">,</xsl:if>
        <xsl:value-of>['{@f}','{@t}']</xsl:value-of>
    </xsl:template>
</xsl:stylesheet>