<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:xs="http://www.w3.org/2001/XMLSchema"
    xmlns:math="http://www.w3.org/2005/xpath-functions/math" 
    xmlns:doc="http://www.saxonica.com/ns/documentation"
    xmlns:dc="http://purl.org/dc/elements/1.1/"
    exclude-result-prefixes="xs math"
    version="3.0">
    
    <doc:meta>
        <dc:title>Complex - based on P18.37</dc:title>
    </doc:meta>
    <xsl:template match="/">
        <chapter-start-page>
            <xsl:stream href="book.xml">
 <!--               <xsl:iterate select="book[summary]/chapter/flooble//bar[zoo][soo[foo]][a][b][c][count(b) gt 5]">
     
-->                 <xsl:iterate select="book[summary]/chapter//section[parent::subchapter][footnote][count(footnote) gt 5]">
                  <xsl:param name="start-page" select="1"/>
                    <chapter title="{title}" start-page="{$start-page}">
                        <xsl:value-of select="for $i in (1,2,3.7,4), $j in 2 to 6 return 'foo'"/>
                        <xsl:copy-of select="parts ! 'bar'"/>
                        <xsl:value-of select="let $p := math:pi() return 2 * $p,string(.),if(summary) then title else count(para)"/>
                        <xsl:value-of>
                            <xsl:copy-of select="sub-heading"/>
                        </xsl:value-of>
                        <xsl:if test="footnote">
                            <xsl:text>With footnotes</xsl:text>
                        </xsl:if>
                    </chapter>
                    <xsl:next-iteration>
                        <xsl:with-param name="start-page" 
                            select="$start-page + @number-of-pages + 
                            (@number-of-pages mod 2) - 45"/>
                    </xsl:next-iteration>
                </xsl:iterate>
            </xsl:stream>
        </chapter-start-page>
    </xsl:template>

</xsl:stylesheet>
