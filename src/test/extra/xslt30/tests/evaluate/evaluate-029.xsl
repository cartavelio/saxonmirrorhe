<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="2.0" xmlns:saxon="http://saxon.sf.net/">
    <xsl:output method="xml" indent="yes"/>
    <xsl:template match="/">
        <xsl:element name="results">
            <xsl:apply-templates/>
        </xsl:element>
    </xsl:template>
    <xsl:template match="test">
        <xsl:variable name="t" as="node()*" select="saxon:evaluate-node(@xpath)"/>
        <xsl:element name="result">
            <xsl:copy-of select="@xpath"/>
            <xsl:copy-of select="$t"/>
        </xsl:element>
    </xsl:template>
    <xsl:template match="node()">
        <xsl:apply-templates/>
    </xsl:template>
</xsl:stylesheet>
