<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:xs="http://www.w3.org/2001/XMLSchema"
                xmlns:fots="http://www.w3.org/2012/08/qt-fots-results"
                xmlns:xslts="http://www.w3.org/2012/11/xslt30-test-results"
                xmlns:xsdts="http://www.w3.org/XML/2004/xml-schema-test-suite/"
                exclude-result-prefixes="xs"
                version="3.0">

<xsl:output method="text" encoding="utf-8" indent="no"/>

<xsl:param name="path" as="xs:string" required="true"/>

<xsl:template name="xsl:initial-template">
  <xsl:variable name="reports" select="collection($path||'?recurse=yes;select=*.xml')"/>
  <xsl:variable name="summary" as="element()+">
    <xsl:for-each select="$reports[not(contains(base-uri(/), '-errors.xml'))]">
      <summary href="{tokenize(base-uri(/), '/')[last()]}">
        <xsl:choose>
          <xsl:when test="//xsdts:*">
            <xsl:variable name="pass"
                          select="count(//xsdts:testResult[@expectedValidity=@actualValidity])"/>
            <xsl:variable name="else" select="count(//xsdts:testResult) - $pass"/>
            <xsl:sequence select="fots:number($pass)"/>
            <xsl:text> </xsl:text>
            <xsl:sequence select="fots:number($else)"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:variable name="pass" select="count(//*:test-case[@result='pass'])"/>
            <xsl:variable name="else" select="count(//*:test-case) - $pass"/>
            <xsl:sequence select="fots:number($pass)"/>
            <xsl:text> </xsl:text>
            <xsl:sequence select="fots:number($else)"/>
          </xsl:otherwise>
        </xsl:choose>
      </summary>
    </xsl:for-each>
  </xsl:variable>

  <xsl:text>  PASS  ¬PASS Test suite&#10;</xsl:text>

  <xsl:for-each select="$summary">
    <xsl:sort select="@href"/>
    <xsl:sequence select="string(.)"/>
    <xsl:text> </xsl:text>
    <xsl:sequence select="string(@href)"/>
    <xsl:text>&#10;</xsl:text>
  </xsl:for-each>
</xsl:template>

<xsl:function name="fots:number" as="xs:string">
  <xsl:param name="number" as="xs:integer"/>
  <xsl:variable name="string" select="'          '||$number"/>
  <xsl:sequence select="substring($string, string-length($string) - 5)"/>
</xsl:function>

</xsl:stylesheet>
