<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:xs="http://www.w3.org/2001/XMLSchema"
    xmlns:math="http://www.w3.org/2005/xpath-functions/math" xmlns:doc="http://www.saxonica.com/ns/documentation"
    xmlns:dc="http://purl.org/dc/elements/1.1/" exclude-result-prefixes="xs math"
    version="3.0">
    
    <doc:meta>
        <dc:title>Stream - book splitting - P18.3</dc:title>
    </doc:meta>
    
    <xsl:template match="/">
        <xsl:stream href="book.xml">
            <xsl:for-each select="book">
                <xsl:for-each select="chapter">
                    <xsl:result-document href="chapter{position()}.xml">
                        <xsl:copy-of select="."/>
                    </xsl:result-document>
                </xsl:for-each>
            </xsl:for-each>
        </xsl:stream>
    </xsl:template>

</xsl:stylesheet>
