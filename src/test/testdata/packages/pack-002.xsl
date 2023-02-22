<?xml version="1.0" encoding="UTF-8"?>
<xsl:package name="http://example.org/pack-002.xsl"
    package-version="1.0"  version="3.0"
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:xs="http://www.w3.org/2001/XMLSchema"
    xmlns:f="http://example.org/package">
    
    <xsl:use-package name="http://example.org/pack-001.xsl" package-version="1.0"/>
    
    <xsl:mode/>
    
    <xsl:template match="/">
        <xsl:variable name="output-text" select="f:woot()"/>
        <xsl:value-of select="$output-text"/>       
    </xsl:template>
</xsl:package>