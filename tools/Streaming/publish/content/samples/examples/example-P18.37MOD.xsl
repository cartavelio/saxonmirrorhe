<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:xs="http://www.w3.org/2001/XMLSchema"
    xmlns:math="http://www.w3.org/2005/xpath-functions/math"
    xmlns:doc="http://www.saxonica.com/ns/documentation" xmlns:dc="http://purl.org/dc/elements/1.1/"
    exclude-result-prefixes="xs math" version="3.0">

    <doc:meta>
        <dc:title>MODIFIED Stream with iterate - P18.37</dc:title>
    </doc:meta>
    <xsl:template match="/">
        <chapter-start-page>
            <xsl:stream href="book.xml">
                <xsl:iterate select="book/chapter">
                    <xsl:param name="start-page" select="1"/>
                    <chapter title="{title}" start-page="{$start-page}"/>
                    <xsl:next-iteration>
                        <xsl:with-param name="start-page"
                            select="$start-page + @number-of-pages"
                        />
                    </xsl:next-iteration>
                </xsl:iterate>
            </xsl:stream>
        </chapter-start-page>
    </xsl:template>
  <xsl:template match="foo[bar][alpha]"/>
  <xsl:template match="foo[@bar][@alpha]"/>

</xsl:stylesheet>
