<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0"
    xmlns:g="http://www.w3.org/2001/03/XPath/grammar"
    xmlns:xlink="http://www.w3.org/1999/xlink"
    exclude-result-prefixes="g xlink">

  <xsl:import href="assemble-joint-spec.xsl"/>

  <xsl:template name="construct-gn-link">
    <xsl:param name="string"/>
    <xsl:param name="spec" select="/g:grammar/g:language/@id"/>
    <xsl:param name="label" select="'gn: '"/>
    <xsl:param name="link-prefix" select="'parse-note-'"/>
    <com>
      <phrase><xspecref ref="{$link-prefix}{$string}">
        <xsl:attribute name="spec">
          <xsl:choose>
            <xsl:when test="$spec='xpath'">
              <xsl:text>XP</xsl:text>
            </xsl:when>
            <xsl:otherwise>
              <xsl:text>XQ</xsl:text>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:attribute>
        <xsl:value-of select="$label"/>
        <xsl:value-of select="$string"/>
      </xspecref></phrase>
    </com>
    
  </xsl:template>

  <!-- xsl:template name="make-lhs">
    <xsl:param name="name"/>
    <xsl:param name="spec" select="/g:grammar/g:language/@id"/>
    <xsl:param name="link-prefix" select="'doc-'"/>
    <xsl:choose>
      <xsl:when test="ancestor-or-self::*[@if[contains(., 'fulltext')]] or @is-xml='yes'">
        <lhs>
          <xsl:call-template name="add-spec-class"/>
          <xsl:value-of select="$name"/>
        </lhs>
      </xsl:when>
      <xsl:otherwise>
        <lhs>
          <xsl:call-template name="add-spec-class"/>
          <phrase>
            <xspecref ref="{$link-prefix}{$spec}-{$name}">
              <xsl:attribute name="spec">
                <xsl:choose>
                  <xsl:when test="$spec='xpath'">
                    <xsl:text>XP</xsl:text>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:text>XQ</xsl:text>
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:attribute>
              <xsl:value-of select="$name"/>
            </xspecref>
          </phrase>
        </lhs>     
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template -->


  <xsl:template name="do-whitespace-comment">
    <xsl:call-template name="construct-gn-link">
      <xsl:with-param name="string" select="@whitespace-spec"/>
      <xsl:with-param name="label" select="'ws: '"/>
      <xsl:with-param name="link-prefix" select="'ws-'"/>
    </xsl:call-template>
  </xsl:template>


  <!-- Try and apply a space delimited list of references to tokens. -->
  <xsl:template name="apply-gns">
    <xsl:param name="string" select="''"/>
    <xsl:param name="delimiter" select="' '"/>
    <xsl:param name="count" select="0"/>
    
    <xsl:choose>
      <xsl:when test="contains($string, $delimiter)">
        <xsl:variable name="token" select="substring-before($string, $delimiter)"/>
        <xsl:call-template name="construct-gn-link">
          <xsl:with-param name="string" select="$token"/>
        </xsl:call-template>
        <xsl:call-template name="apply-gns">
          <xsl:with-param name="string" select="substring-after($string, $delimiter)"/>
          <xsl:with-param name="delimiter" select="$delimiter"/>
          <xsl:with-param name="count" select="$count+1"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
        <xsl:call-template name="construct-gn-link">
          <xsl:with-param name="string" select="$string"/>
        </xsl:call-template>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

</xsl:stylesheet>