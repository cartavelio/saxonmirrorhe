<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:xs="http://www.w3.org/2001/XMLSchema"
  xmlns:math="http://www.w3.org/2005/xpath-functions/math"
  xmlns:doc="http://www.saxonica.com/ns/documentation" xmlns:dc="http://purl.org/dc/elements/1.1/"
  exclude-result-prefixes="xs math" version="3.0">

  <doc:meta>
    <dc:title>Odds and Ends</dc:title>
  </doc:meta>
  <xsl:mode name="s" streamable="yes"/>
  <xsl:template match="/">
    <chapter-start-page>
      <xsl:stream href="book.xml">
        <xsl:iterate select="book/chapter">
          <xsl:param name="start-page" select="1"/>
          <chapter title="{title}" start-page="{$start-page}"/>
          <xsl:next-iteration>
            <xsl:with-param name="start-page" select="$start-page + @number-of-pages"/>
          </xsl:next-iteration>
        </xsl:iterate>
      </xsl:stream>
    </chapter-start-page>
  </xsl:template>
  <xsl:template match="foo" mode="s">
    <xsl:value-of
      select="some $t in transaction, $v in foo satisfies xs:decimal($t/@value) lt -200.0"/>
    <xsl:copy-of select="../@bar"/>
  </xsl:template>

  <xsl:template match="bar" mode="s">
    <xsl:value-of select="count(//h)"/>
    <xsl:value-of select="count(//h) + count(//j)"/>
    <xsl:value-of select="count(//*[self::h|self::j])"/>
  </xsl:template>
  
  <xsl:template match="/" mode="s">
    <xsl:value-of select="count(//h)"/>
    <xsl:value-of select="count(//h) + count(//j)"/>
    <xsl:value-of select="count(//*[self::h|self::j])"/>
  </xsl:template>
  
  <xsl:template match="charlie" mode="s">
    <xsl:value-of select="count(.//h)"/>
    <xsl:value-of select="count(.//h) + count(.//j)"/>
    <xsl:value-of select="count(.//*[self::h|self::j])"/>
  </xsl:template>
  
  <xsl:template name="s-015" use-when="$RUN">
    <xsl:stream href="../docs/books.xml">
      <out>
        <xsl:value-of
          select="format-number(avg(for $d in data(//DIMENSIONS)
          return let $x := tokenize($d, '\s')!number() 
          return $x[1]*$x[2]*$x[3]), '99.999')"
        />
      </out>
    </xsl:stream>
  </xsl:template>

</xsl:stylesheet>
