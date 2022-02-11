<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:xs="http://www.w3.org/2001/XMLSchema"
    xmlns:math="http://www.w3.org/2005/xpath-functions/math"
    xmlns:doc="http://www.saxonica.com/ns/documentation" xmlns:dc="http://purl.org/dc/elements/1.1/"
    exclude-result-prefixes="xs math" version="3.0">
    <doc:meta>
        <dc:title>Add hierarchy example - P18.40</dc:title>
    </doc:meta>
    <xsl:template match="/">
        <book>
            <xsl:stream href="book.xml">
                <xsl:for-each select="book">
                    <xsl:for-each-group select="chapter" group-adjacent="data(@part)" bind-group="g"
                        bind-grouping-key="k">
                        <part number="{$k}">
                            <xsl:copy-of select="$g"/>
                        </part>
                    </xsl:for-each-group>
                </xsl:for-each>
            </xsl:stream>
        </book>
    </xsl:template>

</xsl:stylesheet>
