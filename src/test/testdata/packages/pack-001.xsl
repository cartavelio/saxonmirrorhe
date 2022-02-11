<?xml version="1.0" encoding="UTF-8"?>
<xsl:package name="http://example.org/pack-001.xsl"
    package-version="1.0"  version="3.0"
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:xs="http://www.w3.org/2001/XMLSchema"
    xmlns:f="http://example.org/package">
    
    <xsl:function name="f:woot" visibility="public">
        <xsl:value-of select="'DANGGGG'"/>
    </xsl:function>
    
</xsl:package>