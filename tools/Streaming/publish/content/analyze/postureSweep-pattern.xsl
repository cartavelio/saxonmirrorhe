<?xml version="1.0" encoding="UTF-8"?>

<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:xs="http://www.w3.org/2001/XMLSchema"
  xmlns:math="http://www.w3.org/2005/xpath-functions/math"
  xmlns:map="http://www.w3.org/2005/xpath-functions/map" xmlns:xp="http://saxonica.com/xpathParse"
  xmlns:fos="http://www.w3.org/xpath-functions/spec/namespace"
  xmlns:e="http://www.w3.org/1999/XSL/Spec/ElementSyntax" xmlns:f="myfunctions"
  xmlns:doc="http://www.saxonica.com/ns/documentation" xmlns:dc="http://purl.org/dc/elements/1.1/"
  xmlns:X="IndirectXSLT" xmlns:s="StreamAnalysis" exclude-result-prefixes="xs math fos"
  version="3.0">

  <xsl:template match="xp:*[@s:role='match']" mode="addPosture" priority="1.5">
    <xsl:variable name="temp" as="element()">
      <xsl:next-match/>
    </xsl:variable>
    <xsl:for-each select="$temp">
      <xsl:copy>
        <xsl:sequence select="@*"/>
        <xsl:variable name="top-level-predicates"
          select="descendant-or-self::xp:Predicate[not(ancestor::xp:Predicate)]"/>
        <xsl:choose>
          <xsl:when test="descendant-or-self::xp:RootedPath">
            <xsl:attribute name="s:sweep">free-ranging</xsl:attribute>
          </xsl:when>
          <xsl:when test="$top-level-predicates/*[@s:sweep ne 'motionless']">
            <xsl:attribute name="s:sweep">free-ranging</xsl:attribute>
          </xsl:when>
          <xsl:when
            test="$top-level-predicates//xp:FunctionCall[@name=('position','last','function-lookup')]"
            doc:doc="Check for not in nested predicate">
            <xsl:attribute name="s:sweep">free-ranging</xsl:attribute>
          </xsl:when>
          <xsl:when
            test="some $t in $top-level-predicates/*/@s:staticType 
            satisfies xp:base-type($t)=('xs:anyAtomicType','xs:decimal','xs:integer','xs:double','xs:float')"
            doc:doc="Type could yield numeric" doc:TODO="full subtype check">
            <xsl:attribute name="s:sweep">free-ranging</xsl:attribute>
          </xsl:when>
          <xsl:when test="descendant-or-self::xp:VarRef">
            <xsl:attribute name="s:sweep">free-ranging</xsl:attribute>
          </xsl:when>
          <xsl:otherwise>
            <xsl:attribute name="s:sweep">motionless</xsl:attribute>
          </xsl:otherwise>
        </xsl:choose>
        <xsl:sequence select="*"/>
      </xsl:copy>
    </xsl:for-each>
  </xsl:template>

  <xsl:template match="xp:*[@s:role='match']//xp:Predicate[not(ancestor::xp:Predicate)]"
    mode="addPosture" priority="1.5">
    <xsl:copy>
      <xsl:sequence select="@*"/>
      <xsl:apply-templates select="*" mode="#current">
        <xsl:with-param name="contextPosture" tunnel="yes">striding</xsl:with-param>
      </xsl:apply-templates>
    </xsl:copy>
  </xsl:template>
</xsl:stylesheet>
