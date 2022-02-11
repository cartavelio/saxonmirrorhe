<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:xs="http://www.w3.org/2001/XMLSchema"
    exclude-result-prefixes="#all"
    version="2.0">
    
    <xsl:output method="text" omit-xml-declaration="yes"/>
    <xsl:variable name="newline"><xsl:text>&#xa;</xsl:text></xsl:variable>
    
    <xsl:template match="/">
        <xsl:apply-templates/>
        <xsl:value-of select="$newline"/>
    </xsl:template>
    
    <!-- processing instructions -->
    <xsl:template match="processing-instruction()"/>
    
    <!-- text -->
    <xsl:template match="text()"/>
    
    <!-- elements -->
    <xsl:template match="element()">
        <xsl:value-of select="$newline"/>
        <xsl:value-of select="name(.)"/>
        <xsl:apply-templates/>
    </xsl:template>
    
</xsl:stylesheet>