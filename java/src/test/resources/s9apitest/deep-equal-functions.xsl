<xsl:stylesheet version="3.0"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:f="urn:test">
  <xsl:function name="f:identity">
    <xsl:param name="value"/>
    <xsl:sequence select="$value"/>
  </xsl:function>

  <xsl:template name="function-pair">
    <xsl:sequence select="deep-equal(f:identity#1, f:identity#1)"/>
  </xsl:template>

  <xsl:template name="function-first">
    <xsl:sequence select="deep-equal(f:identity#1, [])"/>
  </xsl:template>

  <xsl:template name="function-last">
    <xsl:sequence select="deep-equal(1, f:identity#1)"/>
  </xsl:template>

  <xsl:template name="nested-functions">
    <xsl:sequence select="deep-equal(map{'a': [f:identity#1]}, map{'a': [f:identity#1]})"/>
  </xsl:template>

  <xsl:template name="equal-containers">
    <xsl:sequence select="deep-equal(map{'a': [1, 2]}, map{'a': [1, 2]})"/>
  </xsl:template>
</xsl:stylesheet>
