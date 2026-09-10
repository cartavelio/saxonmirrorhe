<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:xs="http://www.w3.org/2001/XMLSchema"
                xmlns:sfn="http://www.saxonica.com/ns/doc/functions"
                xmlns:fos="http://www.w3.org/xpath-functions/spec/namespace"
                exclude-result-prefixes="#all"
                expand-text="true"
                version="3.0">

<xsl:output method="xml" encoding="utf-8" indent="no"/>
<xsl:preserve-space elements="*"/>

<xsl:variable name="qt4cg-fn-uri"
  select="resolve-uri('../../../../qt4cg/qtspecs/specifications/xpath-functions-40/src/function-catalog.xml',
                      static-base-uri())"/>
<xsl:variable name="qt4cg-xslt-uri"
  select="resolve-uri('../../../../qt4cg/qtspecs/specifications/xslt-40/src/function-catalog.xml',
                      static-base-uri())"/>

<xsl:variable name="qt4cg-functions">
  <xsl:document>
    <xsl:sequence select="doc($qt4cg-fn-uri)/*"/>
    <xsl:sequence select="doc($qt4cg-xslt-uri)/*"/>
  </xsl:document>
</xsl:variable>

<xsl:mode on-no-match="shallow-copy"/>

<xsl:template match="/">
  <xsl:apply-templates/>
</xsl:template>

<xsl:template match="section">
  <xsl:copy>
    <xsl:sequence select="@*"/>
    <xsl:choose>
      <xsl:when test="@id and sfn:prefix-to-ns(@id) != ''">
        <xsl:variable name="nsuri" select="sfn:prefix-to-ns(@id)"/>
        <xsl:variable name="ff" select="sfn:function[1]"/>
        <xsl:variable name="lf" select="sfn:function[last()]"/>
        <xsl:apply-templates select="node()[. &lt;&lt; $ff]"/>

        <xsl:variable name="sfn-functions" select="sfn:function"/>

        <xsl:for-each select="$qt4cg-functions//fos:function">
          <xsl:sort select="lower-case(@name)"/>
          <xsl:variable name="ns" select="sfn:prefix-to-ns(@prefix)"/>
          <xsl:if test="$ns = $nsuri">
            <xsl:variable name="name" select="@name/string()"/>
            <xsl:variable name="sfn-func" select="$sfn-functions[sfn:name = $name]"/>
            <xsl:choose>
              <xsl:when test="$sfn-func">
                <xsl:apply-templates select="$sfn-func"/>
              </xsl:when>
              <xsl:otherwise>
                <function xmlns="http://www.saxonica.com/ns/doc/functions">
                  <name namespace="{$nsuri}">{$name}</name>
                  <xsl:apply-templates select="fos:signatures" mode="proto"/>
                  <status>
                    <p>FIXME:</p>
                  </status>
                  <saxon-edition>EE_13</saxon-edition>
                  <description>
                    <p>FIXME:</p>
                    <xsl:for-each select="fos:summary/*">
                      <xsl:apply-templates select="." mode="addns"/>
                      <xsl:text>&#10;</xsl:text>
                    </xsl:for-each>
                  </description>
                </function>
                <xsl:text>&#10;</xsl:text>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:if>
        </xsl:for-each>

        <xsl:apply-templates select="node()[. &gt;&gt; $lf]"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:apply-templates/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:copy>
</xsl:template>

<xsl:template match="sfn:function">
  <xsl:copy>
    <xsl:apply-templates select="@*,node()"/>
    <xsl:if test="sfn:signatures/sfn:proto/sfn:description">
      <xsl:apply-templates select="(sfn:signatures/sfn:proto/sfn:description)[1]"/>
      <xsl:text>&#10;</xsl:text>
    </xsl:if>
  </xsl:copy>
  <xsl:text>&#10;</xsl:text>
</xsl:template>

<xsl:template match="sfn:signatures">
  <xsl:variable name="qtf" select="sfn:saxon-to-qt4cg(..)"/>
  <xsl:variable name="prefix" select="sfn:ns-to-prefix(../sfn:name/@namespace)"/>
  <xsl:variable name="name" select="string(../sfn:name)"/>
  <xsl:choose>
    <xsl:when test="exists($qtf)">
      <xsl:copy>
        <xsl:sequence select="@*"/>
        <xsl:text>&#10;</xsl:text>

        <xsl:variable name="proto" select="$qtf/fos:signatures/fos:proto"/>

        <xsl:if test="count($proto) gt 1">
          <xsl:message
              select="'Update: ' || $prefix || ':' || $name || ' ' || count($proto)"/>
        </xsl:if>
        <xsl:apply-templates select="$proto" mode="proto">
          <xsl:with-param name="protos" select="sfn:proto" tunnel="yes"/>
        </xsl:apply-templates>
      </xsl:copy>
    </xsl:when>
    <xsl:otherwise>
      <xsl:next-match/>
    </xsl:otherwise>
  </xsl:choose>
</xsl:template>

<!-- ============================================================ -->

<xsl:template match="fos:signatures" mode="proto">
  <signatures xmlns="http://www.saxonica.com/ns/doc/functions">
    <xsl:sequence select="@*"/>

    <xsl:if test="count(fos:proto) gt 1">
      <xsl:message
          select="'New   : ' || ../@name/string() || ' ' || count(fos:proto)"/>
    </xsl:if>

    <xsl:apply-templates mode="proto"/>
  </signatures>
</xsl:template>

<xsl:template match="fos:proto" mode="proto">
  <xsl:param name="protos" as="element(sfn:proto)*" tunnel="yes"/>

  <xsl:variable name="num" select="count(preceding-sibling::fos:proto)+1"/>

  <proto xmlns="http://www.saxonica.com/ns/doc/functions">
    <xsl:sequence select="@return-type"/>
    <xsl:text>&#10;</xsl:text>
    <xsl:apply-templates select="fos:arg" mode="proto"/>
    <xsl:for-each select="$protos[1]/sfn:in-spec">
      <xsl:sequence select="."/>
      <xsl:text>&#10;</xsl:text>
    </xsl:for-each>
    <in-spec xmlns="http://www.saxonica.com/ns/doc/functions">xpath40</in-spec>
    <xsl:text>&#10;</xsl:text>
    <xsl:if test="count(../fos:proto) gt 1">
      <xsl:apply-templates select="$protos[$num]/sfn:description"/>
    </xsl:if>
  </proto>
  <xsl:text>&#10;</xsl:text>
</xsl:template>

<xsl:template match="fos:arg" mode="proto">
  <xsl:param name="protos" as="element(sfn:proto)*" tunnel="yes"/>
  <xsl:variable name="pos" select="count(preceding-sibling::fos:arg)+1"/>
  <arg xmlns="http://www.saxonica.com/ns/doc/functions">
    <xsl:sequence select="@name, @type, @default"/>
    <xsl:sequence select="($protos//sfn:arg[position() = $pos]/@desc)[1]"/>
    <xsl:if test="empty($protos//sfn:arg/@desc)">
      <xsl:attribute name="desc" select="'FIXME:'"/>
    </xsl:if>
  </arg>
  <xsl:text>&#10;</xsl:text>
</xsl:template>

<!-- ============================================================ -->

<xsl:template match="p|code" mode="addns">
  <xsl:element name="{local-name(.)}" namespace="http://www.saxonica.com/ns/doc/functions">
    <xsl:apply-templates mode="addns"/>
  </xsl:element>
</xsl:template>

<xsl:template match="function" mode="addns">
  <em namespace="http://www.saxonica.com/ns/doc/functions">
    <xsl:apply-templates mode="addns"/>
  </em>
</xsl:template>

<xsl:template match="xtermref" mode="addns">
  <xsl:text>XTERMREF:</xsl:text>
  <xsl:value-of select="@spec"/>
  <xsl:text>.</xsl:text>
  <xsl:value-of select="@ref"/>
  <xsl:text>:</xsl:text>
</xsl:template>

<xsl:template match="termref" mode="addns">
  <xsl:text>TERMREF:</xsl:text>
  <xsl:value-of select="@def"/>
  <xsl:text>:</xsl:text>
</xsl:template>

<xsl:template match="specref" mode="addns">
  <xsl:text>SPECREF:</xsl:text>
  <xsl:value-of select="@ref"/>
  <xsl:text>:</xsl:text>
</xsl:template>

<xsl:template match="term" mode="addns">
  <xsl:apply-templates/>
</xsl:template>

<xsl:template match="elcode|var" mode="addns">
  <code><xsl:apply-templates/></code>
</xsl:template>

<xsl:template match="*" mode="addns">
  <xsl:message>Unexpected: <xsl:value-of select="node-name(.)"/></xsl:message>
  <!--<xsl:message select="ancestor::fos:function"/>-->
  <xsl:apply-templates mode="addns"/>
</xsl:template>

<!-- ============================================================ -->

<xsl:function name="sfn:ns-to-prefix" as="xs:string">
  <xsl:param name="ns" as="xs:string?"/>
  <xsl:choose>
    <xsl:when test="$ns = 'http://exslt.org/sets'">exslt-sets</xsl:when>
    <xsl:when test="$ns = 'http://exslt.org/math'">exslt-math</xsl:when>
    <xsl:when test="$ns = 'http://exslt.org/dates-and-times'">exslt-dt</xsl:when>
    <xsl:when test="$ns = 'http://exslt.org/random'">exslt-random</xsl:when>
    <xsl:when test="$ns = 'http://exslt.org/common'">exslt-common</xsl:when>
    <xsl:when test="$ns = 'http://saxon.sf.net/'">saxon</xsl:when>
    <xsl:when test="$ns = 'http://www.w3.org/2005/xpath-functions/array'">array</xsl:when>
    <xsl:when test="$ns = 'http://www.w3.org/2005/xpath-functions/map'">map</xsl:when>
    <xsl:when test="$ns = 'http://www.w3.org/2005/xpath-functions/math'">math</xsl:when>
    <xsl:when test="$ns = 'http://www.w3.org/2005/xpath-functions'">fn</xsl:when>
    <xsl:otherwise>
      <xsl:message select="'ns: ' || $ns"/>
      <xsl:sequence select="''"/>
    </xsl:otherwise>
  </xsl:choose>
</xsl:function>

<xsl:function name="sfn:prefix-to-ns" as="xs:string">
  <xsl:param name="prefix" as="xs:string?"/>
  <xsl:choose>
    <xsl:when test="empty($prefix)">http://www.w3.org/2005/xpath-functions</xsl:when>
    <xsl:when test="$prefix = 'fn'">http://www.w3.org/2005/xpath-functions</xsl:when>
    <xsl:when test="$prefix = 'map'">http://www.w3.org/2005/xpath-functions/map</xsl:when>
    <xsl:when test="$prefix = 'array'">http://www.w3.org/2005/xpath-functions/array</xsl:when>
    <xsl:when test="$prefix = 'math'">http://www.w3.org/2005/xpath-functions/math</xsl:when>
    <xsl:when test="$prefix = 'op'">irrelevant</xsl:when>
    <xsl:otherwise>
      <xsl:message select="'prefix: ' || $prefix"/>
      <xsl:sequence select="''"/>
    </xsl:otherwise>
  </xsl:choose>
</xsl:function>

<xsl:function name="sfn:saxon-to-qt4cg" as="element(fos:function)?">
  <xsl:param name="function" as="element(sfn:function)"/>
  <xsl:variable name="sfn:name" as="element(sfn:name)" select="$function/sfn:name"/>

  <xsl:variable name="prefix" select="sfn:ns-to-prefix($sfn:name/@namespace)"/>
  <xsl:variable name="name" select="$function/sfn:name/string()"/>

  <xsl:sequence select="$qt4cg-functions//fos:function[@name=$name and @prefix=$prefix]"/>
</xsl:function>

<!--
<xsl:template match="fos:signatures">
  <signatures xmlns="http://www.saxonica.com/ns/doc/functions">
    <xsl:sequence select="@*"/>
    <xsl:apply-templates/>
  </signatures>
</xsl:template>

<xsl:template match="fos:proto">
  <proto xmlns="http://www.saxonica.com/ns/doc/functions">
    <xsl:sequence select="@return-type"/>
    <xsl:apply-templates/>
  </proto>
</xsl:template>

<xsl:template match="fos:arg">
  <arg xmlns="http://www.saxonica.com/ns/doc/functions">
    <xsl:sequence select="@*"/>
    <xsl:attribute name="desc" select="'FIXME:'"/>
  </arg>
</xsl:template>
-->

</xsl:stylesheet>
