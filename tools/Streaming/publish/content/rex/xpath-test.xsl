<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:xs="http://www.w3.org/2001/XMLSchema"
  xmlns:math="http://www.w3.org/2005/xpath-functions/math" xmlns:p="CR-xpath-30-20130108"
  xmlns:xp="http://saxonica.com/xpathParse" xmlns:s="StreamAnalysis"
  exclude-result-prefixes="xs math" version="3.0">
  <xsl:include href="xpath3.0-parse.xslt"/>

  <xsl:param name="xp">xsl:foo/bar[@flobble='foo']</xsl:param>
  <xsl:param name="xp1">some $a in /, $t in //transaction/../attribute::bar[.='//'], $z in B//C, $q
    in A/ancestor::BAR/text(), $v in /descendant-or-self::node()/foo satisfies xs:decimal($t/@value)
    lt -200.0 + 3</xsl:param>
  <xsl:param name="xp2">.//section/@bar</xsl:param>

  <xsl:variable name="xp3">/,/A,(root(self::node()) treat as document-node()),(root(self::node())
    treat as document-node())/A</xsl:variable>
  <xsl:variable name="xp4">for $i in expr, $j in foo return (34,1)</xsl:variable>
  <xsl:variable name="xp5">data(//dimensions)</xsl:variable>
  <xsl:output method="xml" indent="yes"/>
  <xsl:variable name="raw" select="false()" static="yes"/>




  <xsl:template match="/">
    <xsl:variable name="s" select="$xp3"/>
    <xsl:variable name="expr" select="xp:modify.xpath($s)"/>
    <xsl:variable name="parse" select="p:parse-XPath($expr)"/>
    <raw xsl:use-when="$raw" xsl:expand-text="yes">
      <expression>{$s}</expression>
      <modified>{$expr}</modified>
      <xsl:sequence select="$parse"/>
    </raw>
    <result xsl:use-when="not($raw)" xsl:expand-text="yes">
      <expression>{$s}</expression>
      <modified>{$expr}</modified>
      <xsl:apply-templates select="xp:parse.xpath($expr)" mode="construct"/>
      <raw>
        <xsl:sequence select="$parse"/>
      </raw>
    </result>
  </xsl:template>


  <xsl:mode name="construct" on-no-match="shallow-copy"/>

  <xsl:template
    match="RelativePathExpr[AxisStep[@axis='descendant-or-self']/
    following-sibling::*[1]/self::AxisStep[@axis='child']]"
    mode="construct" xpath-default-namespace="http://saxonica.com/xpathParse"
    xmlns="http://saxonica.com/xpathParse">
    <xsl:variable name="temp" as="element()">
      <xsl:variable name="children" as="element()*">
        <xsl:apply-templates select="*" mode="#current"/>
      </xsl:variable>
      <xsl:variable name="parts" as="element()*">
        <xsl:for-each-group select="$children"
          group-starting-with="AxisStep[@axis=('descendant-or-self','attribute')]" bind-group="g">
          <xsl:choose>
            <xsl:when test="$g[1]/self::AxisStep[@axis='descendant-or-self']">
              <AxisStep axis="descendant-or-self">
                <xsl:variable name="node-tests" select="reverse($g/@nodeTest)"/>
                <xsl:sequence select="head($node-tests)"/>
                <PredicateList>
                  <Predicate>
                    <xsl:variable name="tests" as="element()*">
                      <xsl:for-each select="tail(reverse($g))">
                        <xsl:copy>
                          <xsl:attribute name="axis">parent</xsl:attribute>
                          <xsl:sequence select="@nodeTest"/>
                        </xsl:copy>
                      </xsl:for-each>
                    </xsl:variable>
                    <xsl:choose>
                      <xsl:when test="count($tests) gt 1">
                        <RelativePathExpr>
                          <xsl:sequence select="$tests"/>
                        </RelativePathExpr>
                      </xsl:when>
                      <xsl:otherwise>
                        <xsl:sequence select="$tests"/>
                      </xsl:otherwise>
                    </xsl:choose>
                  </Predicate>
                </PredicateList>
              </AxisStep>
            </xsl:when>
            <xsl:otherwise>
              <xsl:sequence select="$g"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:for-each-group>
      </xsl:variable>
      <xsl:choose>
        <xsl:when test="count($parts) = 1">
          <xsl:sequence select="$parts"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:copy>
            <xsl:sequence select="$parts"/>
          </xsl:copy>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:apply-templates select="$temp" mode="#current"/>
  </xsl:template>

</xsl:stylesheet>
