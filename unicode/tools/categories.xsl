<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:xs="http://www.w3.org/2001/XMLSchema"
                xmlns:f="https://www.saxonica.com/ns/functions"
                exclude-result-prefixes="#all"
                expand-text="yes"
                version="3.0">
<xsl:output method="xml" encoding="utf-8" indent="no"/>

<xsl:param name="unicodeVersion" required="yes" as="xs:string"/>
<xsl:variable name="unicode.txt" select="'../build/cache/UnicodeData-' || $unicodeVersion || '.txt'"/>

<xsl:template name="xsl:initial-template">
  <xsl:variable name="lines" select="tokenize(unparsed-text($unicode.txt), '&#10;')"/>
  <xsl:variable name="assigned" as="element()">
    <categories>
      <xsl:message>Parsing Unicode Data; expanding ranges…</xsl:message>
      <xsl:iterate select="$lines[position() gt 1]">
        <xsl:param name="current" select="$lines[1]"/>
        <xsl:param name="chars" as="element(char)*"/>
        <xsl:on-completion select="$chars"/>

        <xsl:variable name="parts" select="tokenize(., ';')"/>
        <xsl:variable name="cp" select="$parts[1]"/>
        <xsl:variable name="name" select="$parts[2]"/>
        <xsl:variable name="category" select="$parts[3]"/>

        <xsl:variable name="hex"
                      select="if ($cp = '0000')
                              then '0'
                              else replace($cp, '^0+', '')"/>

        <xsl:next-iteration>
          <xsl:with-param name="current" select="."/>
          <xsl:with-param name="chars" as="element(char)*">
            <xsl:sequence select="$chars"/>
            <xsl:choose>
              <xsl:when test="$hex = ''"/>
              <xsl:when test="contains($name, ', Last')">
                <xsl:variable name="start" select="f:hextodec(tokenize($current, ';')[1]) + 1"/>
                <xsl:variable name="end" select="f:hextodec($cp)"/>
                <xsl:for-each select="$start to $end">
                  <char cp="{f:dectohex(.)}" category="{$category}"/>
                </xsl:for-each>
              </xsl:when>
              <xsl:otherwise>
                <char cp="{$hex}" category="{$category}"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:with-param>
        </xsl:next-iteration>
      </xsl:iterate>
    </categories>
  </xsl:variable>

  <xsl:variable name="data" as="element()">
    <categories>
      <xsl:sequence select="$assigned/char"/>

      <xsl:message>Adding literal Cn characters…</xsl:message>
      <xsl:iterate select="$assigned/char[position() gt 1]">
        <xsl:param name="current" select="$assigned/char[1]"/>
        <xsl:param name="unassigned" as="element(char)*"/>
        <xsl:on-completion select="$unassigned"/>

        <xsl:next-iteration>
          <xsl:with-param name="current" select="."/>
          <xsl:with-param name="unassigned" as="element(char)*">
            <xsl:sequence select="$unassigned"/>
            <xsl:for-each select="f:hextodec($current/@cp) + 1 to f:hextodec(@cp) - 1">
              <char cp="{f:dectohex(.)}" category="Cn"/>
            </xsl:for-each>
          </xsl:with-param>
        </xsl:next-iteration>
      </xsl:iterate>
    </categories>
  </xsl:variable>

  <!--
  <xsl:result-document href="/tmp/out.xml">
    <xsl:sequence select="$data"/>
  </xsl:result-document>
  -->

  <xsl:variable name="names" select="sort(distinct-values($data/char/@category))"/>

  <xsl:message>Constructing categories.xml</xsl:message>
  <categories>
    <xsl:for-each select="$names">
      <xsl:variable name="category" select="."/>
      <xsl:variable name="chars" select="$data/char[@category = $category]"/>

      <cat name="{$category}">
        <xsl:iterate select="$chars[position() gt 1]">
          <xsl:param name="ranges" as="element(range)+">
            <range f="{$chars[1]/@cp}" t="{$chars[1]/@cp}"/>
          </xsl:param>
          <xsl:on-completion select="$ranges"/>

          <xsl:variable name="cp" select="f:hextodec(@cp)"/>

          <xsl:variable name="prefix" select="$ranges[position() lt last()]"/>
          <xsl:variable name="last-range" select="$ranges[last()]"/>
          <xsl:variable name="last" select="f:hextodec($last-range/@t)"/>

          <xsl:variable name="range" as="element(range)+">
            <xsl:choose>
              <xsl:when test="$cp = $last + 1">
                <range f="{$last-range/@f}" t="{@cp}"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:sequence select="$last-range"/>
                <range f="{@cp}" t="{@cp}"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:variable>

          <xsl:next-iteration>
            <xsl:with-param name="ranges" select="($prefix, $range)"/>
          </xsl:next-iteration>
        </xsl:iterate>
      </cat>
    </xsl:for-each>
  </categories>
</xsl:template>

<xsl:function name="f:hextodec" as="xs:integer">
  <xsl:param name="hex" as="xs:string" required="true"/>

  <xsl:iterate select="reverse(string-to-codepoints(lower-case($hex)))">
    <xsl:param name="power" select="1"/>
    <xsl:param name="acc" select="0"/>
    <xsl:on-completion select="$acc"/>

    <xsl:variable name="digit" select="if (. gt 57)
                                       then . - 87
                                       else . - 48"/>

    <xsl:next-iteration>
      <xsl:with-param name="acc" select="$acc + ($power * $digit)"/>
      <xsl:with-param name="power" select="$power * 16"/>
    </xsl:next-iteration>
  </xsl:iterate>
</xsl:function>

<xsl:variable name="hexdigits"
              select="('0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F')"/>
<xsl:function name="f:dectohex" as="xs:string">
  <xsl:param name="dec" as="xs:integer" required="true"/>
  <xsl:choose>
    <xsl:when test="$dec lt 16">
      <xsl:sequence select="$hexdigits[$dec+1]"/>
    </xsl:when>
    <xsl:otherwise>
      <xsl:variable name="whole" select="$dec idiv 16"/>
      <xsl:variable name="remainder" select="$dec - ($whole * 16)"/>
      <xsl:sequence select="concat(f:dectohex($whole), $hexdigits[$remainder + 1])"/>
    </xsl:otherwise>
  </xsl:choose>
</xsl:function>

</xsl:stylesheet>
