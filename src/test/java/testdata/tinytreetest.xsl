<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="2.0" 
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:xs="http://www.w3.org/2001/XMLSchema"
  xmlns:t="this"
  exclude-result-prefixes="xs t">

  <xsl:template match="/">
    <xsl:variable name="unit" as="element()*">
      <u>
        <xsl:variable name="d" as="element()">
          <c>
            <c>t</c>
            <c>t</c>
          </c>
        </xsl:variable>

        <xsl:sequence select="t:f($d)"/>
      </u>
    </xsl:variable>

    <xsl:apply-templates select="$unit/c/c"/>
  </xsl:template>

  <xsl:function name="t:f" as="element()">
    <xsl:param name="e" as="element()"/>

    <xsl:sequence select="$e"/>
  </xsl:function>

</xsl:stylesheet>
