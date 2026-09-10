<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:xs="http://www.w3.org/2001/XMLSchema"
                xmlns:e="http://www.w3.org/1999/XSL/Spec/ElementSyntax"
                xmlns:elem="http://www.saxonica.com/ns/doc/elements"
                exclude-result-prefixes="#all"
                expand-text="true"
                version="3.0">

<xsl:output method="xml" encoding="utf-8" indent="no"/>
<xsl:preserve-space elements="*"/>

<xsl:key name="id" match="section" use="@id"/>

<xsl:variable name="catalog"
  select="doc('../src/userdoc/elements/element-catalog.xml')/*"/>

<xsl:mode on-no-match="shallow-copy"/>

<xsl:template match="article">
  <xsl:variable name="accept" select="section[@id='accept']"/>
  <xsl:variable name="with-param" select="section[@id='with-param']"/>
  <xsl:variable name="this" select="."/>

  <xsl:copy>
    <xsl:sequence select="@*"/>
    <xsl:apply-templates select="node()[. &lt;&lt; $accept]"/>

    <xsl:for-each select="$catalog/e:element-syntax">
      <xsl:sort select="lower-case(@name)"/>
      <xsl:variable name="section" select="key('id', @name, $this)"/>
      <xsl:choose>
        <xsl:when test="$section">
          <xsl:text>&#10;    </xsl:text>
          <xsl:apply-templates select="$section">
            <xsl:with-param name="syntax" select="." tunnel="yes"/>
          </xsl:apply-templates>
          <xsl:text>&#10;</xsl:text>
        </xsl:when>
        <xsl:otherwise>
          <xsl:message select="'New:', @name/string()"/>
          <section id="{@name}" title="xsl:{@name}">
            <xsl:text>&#10;</xsl:text>
            <xsl-element xmlns="http://www.saxonica.com/ns/doc/elements">
              <xsl:text>&#10;</xsl:text>
              <name><xsl:value-of select="@name"/></name>
              <xsl:text>&#10;</xsl:text>
              <description>
                <p>FIXME:</p>
              </description>
              <xsl:text>&#10;</xsl:text>
              <attributes>
                <xsl:for-each select="e:attribute">
                  <xsl:text>&#10;</xsl:text>
                  <attribute name="{@name}">FIXME:</attribute>
                </xsl:for-each>
              </attributes>
              <xsl:text>&#10;</xsl:text>
              <in-spec>xslt40</in-spec>
              <xsl:text>&#10;</xsl:text>
              <saxon-edition>EE_13</saxon-edition>
              <xsl:text>&#10;</xsl:text>
              <status>
                <xsl:text>&#10;</xsl:text>
                <p>New in XSLT 4.0, and implemented since Saxon 13.0.</p>
              </status>
            </xsl-element>
            <xsl:text>&#10;</xsl:text>
          </section>
          <xsl:text>&#10;</xsl:text>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:for-each>

    <xsl:apply-templates select="node()[. &gt;&gt; $with-param]"/>
  </xsl:copy>
</xsl:template>

<xsl:template match="elem:attributes">
  <xsl:param name="syntax" tunnel="yes"/>
  <xsl:message select="string(ancestor::elem:xsl-element/elem:name)"/>

  <xsl:variable name="this" select="."/>
  <xsl:copy>
    <xsl:sequence select="@*"/>
    <xsl:for-each select="$syntax/e:attribute">
      <xsl:sort select="@name"/>
      <xsl:text>&#10;              </xsl:text>
      <xsl:variable name="name" select="string(@name)"/>
      <xsl:choose>
        <xsl:when test="$this/elem:attribute[@name = $name]">
          <xsl:apply-templates select="$this/elem:attribute[@name = $name]"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:message select="'  new: ' || $name"/>
          <attribute xmlns="http://www.saxonica.com/ns/doc/elements"
                     name="{$name}">FIXME:</attribute>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:for-each>

    <xsl:for-each select="elem:attribute">
      <xsl:sort select="@name"/>
      <xsl:variable name="name" select="string(@name)"/>
      <xsl:if test="empty($syntax/e:attribute[@name = $name])">
        <xsl:choose>
          <xsl:when test="starts-with($name, 'saxon:')">
            <xsl:text>&#10;              </xsl:text>
            <xsl:apply-templates select="."/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:message select="'  del: ' || $name"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:if>
    </xsl:for-each>
    <xsl:text>&#10;            </xsl:text>
  </xsl:copy>
</xsl:template>
  

</xsl:stylesheet>
