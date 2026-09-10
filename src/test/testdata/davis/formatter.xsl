<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="2.0"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:xs="http://www.w3.org/2001/XMLSchema">

    <xsl:output method="xml" indent="yes" encoding="UTF8"/>
    
    <xsl:template match="testData">
        <xsl:element name="outputData">
            <xsl:apply-templates select="testRow"/>
        </xsl:element>
    </xsl:template>
    
    <xsl:template match="testRow">
        <xsl:element name="outputRow">
            <xsl:attribute name="text"><xsl:value-of select="@data"/></xsl:attribute>
        </xsl:element>
    </xsl:template>
    
</xsl:stylesheet>
                