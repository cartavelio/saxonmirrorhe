<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:xs="http://www.w3.org/2001/XMLSchema"
    xmlns:math="http://www.w3.org/2005/xpath-functions/math"
    xmlns:doc="http://www.saxonica.com/ns/documentation" xmlns:dc="http://purl.org/dc/elements/1.1/"
    exclude-result-prefixes="xs math" version="3.0">

    <doc:meta>
        <dc:title>An Optimization: Pattern-Based Scanning - P19.36</dc:title>
    </doc:meta>

    <xsl:template match="/">
        <xsl:stream href="report.xml">
            <count>
                <xsl:value-of select="count(//section)"/>
            </count>
        </xsl:stream>
        <xsl:stream href="report.xml">
            <count>
                <xsl:value-of select="count(//section/head)"/>
            </count>
        </xsl:stream>
        <xsl:stream href="report.xml">
            <count>
                <xsl:value-of select="count(//head[parent::section])"/>
            </count>
        </xsl:stream>
    </xsl:template>

</xsl:stylesheet>
